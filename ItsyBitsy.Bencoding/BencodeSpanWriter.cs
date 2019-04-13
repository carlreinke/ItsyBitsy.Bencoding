//
// Copyright (C) 2019  Carl Reinke
//
// This file is part of ItsyBitsy.Bencoding.
//
// This program is free software; you can redistribute it and/or modify it under the terms of the
// GNU Lesser General Public License as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
// even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with this program;
// if not, write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
// 02110-1301, USA.
//
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// Represents a writer that provides generation of bencoded data.
    /// </summary>
    public ref struct BencodeSpanWriter
    {
        private const int _intMaxDigits = 10;

        private const int _longMaxDigits = 19;

        private readonly Span<byte> _span;

        private readonly Stack<PreviousKeyRange> _previousKeyStack;

        private int _index;

        private State _state;

        private BitStack _scopeStack;

        private PreviousKeyRange _previousKey;

        /// <summary>
        /// Initializes a new <see cref="BencodeSpanWriter"/> instance using the specified
        /// destination buffer.
        /// </summary>
        /// <param name="destination">The span to write the encoded data into.</param>
        /// <param name="skipValidation">If <see langword="true"/>, keys are not checked for
        ///     mis-ordering and duplication as they are being written.</param>
        public BencodeSpanWriter(Span<byte> destination, bool skipValidation = false)
        {
            _span = destination;
            _index = 0;
            _state = State.Initial;
            _scopeStack = new BitStack();
            _previousKey = PreviousKeyRange.None;
            _previousKeyStack = skipValidation ? null : new Stack<PreviousKeyRange>();
        }

        /// <summary>
        /// Gets the current length of the encoded data.
        /// </summary>
        public int Length => _index;

        /// <summary>
        /// Writes an integer.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows an
        ///     integer to be written.</exception>
        /// <exception cref="ArgumentException">The writer reached the end of the destination buffer
        ///     while writing the integer.</exception>
        public void WriteInteger(long value)
        {
            _state = GetStateAfterValue(_state);

            try
            {
                Span<byte> numberBuffer = stackalloc byte[1 + _longMaxDigits];
                if (!Utf8Formatter.TryFormat(value, numberBuffer, out int numberLength))
                    throw new InvalidOperationException("Unreachable!");

                var span = CheckCapacity(1 + numberLength + 1);

                span[0] = (byte)'i';
                span = span.Slice(1);
                numberBuffer.Slice(0, numberLength).CopyTo(span);
                span = span.Slice(numberLength);
                span[0] = (byte)'e';

                Advance(1 + numberLength + 1);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     string to be written.</exception>
        /// <exception cref="ArgumentException">The writer reached the end of the destination buffer
        ///     while writing the string.</exception>
        public void WriteString(ReadOnlySpan<byte> value)
        {
            _state = GetStateAfterValue(_state);

            WriteStringInternal(value);
        }

        /// <summary>
        /// Writes the beginning of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     list head to be written.</exception>
        /// <exception cref="ArgumentException">The writer reached the end of the destination buffer
        ///     while writing the list head.</exception>
        public void WriteListHead()
        {
            _state = EnterListScope(_state, ref _scopeStack);

            try
            {
                var span = CheckCapacity(1);

                span[0] = (byte)'l';

                Advance(1);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the end of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     list tail to be written.</exception>
        /// <exception cref="ArgumentException">The writer reached the end of the destination buffer
        ///     while writing the list tail.</exception>
        public void WriteListTail()
        {
            _state = ExitListScope(_state, ref _scopeStack);

            try
            {
                var span = CheckCapacity(1);

                span[0] = (byte)'e';

                Advance(1);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the beginning of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     dictionary head to be written.</exception>
        /// <exception cref="ArgumentException">The writer reached the end of the destination buffer
        ///     while writing the dictionary head.</exception>
        public void WriteDictionaryHead()
        {
            _state = EnterDictionaryScope(_state, ref _scopeStack);

            if (_previousKeyStack != null)
            {
                _previousKeyStack.Push(_previousKey);
                _previousKey = PreviousKeyRange.None;
            }

            try
            {
                var span = CheckCapacity(1);

                span[0] = (byte)'d';

                Advance(1);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes the end of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     dictionary tail to be written.</exception>
        /// <exception cref="ArgumentException">The writer reached the end of the destination buffer
        ///     while writing the dictionary tail.</exception>
        public void WriteDictionaryTail()
        {
            _state = ExitDictionaryScope(_state, ref _scopeStack);

            if (_previousKeyStack != null)
                _previousKey = _previousKeyStack.Pop();

            try
            {
                var span = CheckCapacity(1);

                span[0] = (byte)'e';

                Advance(1);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Writes a key.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     key to be written.</exception>
        /// <exception cref="ArgumentException">The writer reached the end of the destination buffer
        ///     while writing the key.</exception>
        public void WriteKey(ReadOnlySpan<byte> key)
        {
            _state = GetStateAfterKey(_state);

            if (_previousKey.HasValue && !IsLess(_previousKey.Slice(_span), key))
                throw new InvalidOperationException("Keys must be ordered and unique.");

            WriteStringInternal(key);

            if (_previousKeyStack != null)
                _previousKey = new PreviousKeyRange(_index - key.Length, key.Length);
        }

        internal static State GetStateAfterValue(State state)
        {
            switch (state)
            {
                case State.Initial:
                    return State.Final;
                case State.DictionaryValue:
                    return State.DictionaryKey;
                case State.ListItem:
                    return State.ListItem;
                default:
                    throw new InvalidOperationException("The writer is not in a state that allows a value to be written.");
            }
        }

        internal static State EnterListScope(State state, ref BitStack scopeStack)
        {
            switch (state)
            {
                case State.Initial:
                    break;
                case State.DictionaryValue:
                    scopeStack.PushTrue();
                    break;
                case State.ListItem:
                    scopeStack.PushFalse();
                    break;
                default:
                    throw new InvalidOperationException("The writer is not in a state that allows a list head to be written.");
            }

            return State.ListItem;
        }

        internal static State ExitListScope(State state, ref BitStack scopeStack)
        {
            switch (state)
            {
                case State.ListItem:
                    return scopeStack.Count == 0 ? State.Final :
                           scopeStack.Pop() ? State.DictionaryKey : State.ListItem;
                default:
                    throw new InvalidOperationException("The writer is not in a state that allows a list tail to be written.");
            }
        }

        internal static State EnterDictionaryScope(State state, ref BitStack scopeStack)
        {
            switch (state)
            {
                case State.Initial:
                    break;
                case State.DictionaryValue:
                    scopeStack.PushTrue();
                    break;
                case State.ListItem:
                    scopeStack.PushFalse();
                    break;
                default:
                    throw new InvalidOperationException("The writer is not in a state that allows a dictionary head to be written.");
            }

            return State.DictionaryKey;
        }

        internal static State ExitDictionaryScope(State state, ref BitStack scopeStack)
        {
            switch (state)
            {
                case State.DictionaryKey:
                    return scopeStack.Count == 0 ? State.Final :
                           scopeStack.Pop() ? State.DictionaryKey : State.ListItem;
                default:
                    throw new InvalidOperationException("The writer is not in a state that allows a dictionary tail to be written.");
            }
        }

        internal static State GetStateAfterKey(State state)
        {
            switch (state)
            {
                case State.DictionaryKey:
                    return State.DictionaryValue;
                default:
                    throw new InvalidOperationException("The writer is not in a state that allows a key to be written.");
            }
        }

        internal static bool IsLess(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
        {
            int minLength = x.Length;
            if (minLength > y.Length)
                minLength = y.Length;

            for (int i = 0; i < minLength; ++i)
                if (x[i] < y[i])
                    return true;

            return x.Length < y.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Span<byte> CheckCapacity(int capacity)
        {
            var span = _span.Slice(_index);
            if (span.Length < capacity)
                throw new ArgumentException("Reached the end of the destination buffer while attempting to write.");
            return span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Advance(int length)
        {
            _index += length;
        }

        private void WriteStringInternal(ReadOnlySpan<byte> value)
        {
            try
            {
                Span<byte> numberBuffer = stackalloc byte[_intMaxDigits];
                if (!Utf8Formatter.TryFormat(value.Length, numberBuffer, out int numberLength))
                    throw new InvalidOperationException("Unreachable!");

                var span = CheckCapacity(numberLength + 1 + value.Length);

                numberBuffer.Slice(0, numberLength).CopyTo(span);
                span = span.Slice(numberLength);
                span[0] = (byte)':';
                span = span.Slice(1);
                value.CopyTo(span);

                Advance(numberLength + 1 + value.Length);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        internal enum State
        {
            Initial = 0,
            DictionaryKey,
            DictionaryValue,
            ListItem,
            Final,
            Error,
        }

        private struct PreviousKeyRange
        {
            public static readonly PreviousKeyRange None = new PreviousKeyRange(-1, 0);

            private readonly int _index;

            private readonly int _length;

            public bool HasValue => _index >= 0;

            public PreviousKeyRange(int index, int length)
            {
                _index = index;
                _length = length;
            }

            public ReadOnlySpan<byte> Slice(ReadOnlySpan<byte> span) => span.Slice(_index, _length);
        }
    }
}
