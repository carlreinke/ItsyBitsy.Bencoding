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
using System.Buffers;
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

        private readonly BencodeWriter _parent;

        private readonly IBufferWriter<byte> _buffer;

        private readonly Stack<PreviousKey> _previousKeyStack;

        private Span<byte> _span;

        private int _bufferedLength;

        private State _state;

        private BitStack _scopeStack;

        private PreviousKey _previousKey;

        /// <summary>
        /// Initializes a new <see cref="BencodeSpanWriter"/> instance using the specified
        /// destination buffer.
        /// </summary>
        /// <param name="destination">The destination to write the encoded data into.</param>
        /// <param name="skipValidation">If <see langword="true"/>, keys are not checked for
        ///     mis-ordering and duplication as they are being written.</param>
        public BencodeSpanWriter(Span<byte> destination, bool skipValidation = false)
        {
            _parent = null;
            _buffer = null;
            _span = destination;
            _bufferedLength = 0;
            _state = State.Initial;
            _scopeStack = new BitStack();
            _previousKey = PreviousKey.None;
            _previousKeyStack = skipValidation ? null : new Stack<PreviousKey>();
        }

        /// <summary>
        /// Initializes a new <see cref="BencodeSpanWriter"/> instance using the specified
        /// destination buffer.
        /// </summary>
        /// <param name="destination">The destination to write the encoded data into.</param>
        /// <param name="skipValidation">If <see langword="true"/>, keys are not checked for
        ///     mis-ordering and duplication as they are being written.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is
        ///     <see langword="null"/>.</exception>
        public BencodeSpanWriter(IBufferWriter<byte> destination, bool skipValidation = false)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            _parent = null;
            _buffer = destination;
            _span = destination.GetSpan();
            _bufferedLength = 0;
            _state = State.Initial;
            _scopeStack = new BitStack();
            _previousKey = PreviousKey.None;
            _previousKeyStack = skipValidation ? null : new Stack<PreviousKey>();
        }

        internal BencodeSpanWriter(BencodeWriter parent, IBufferWriter<byte> destination, State state, BitStack scopeStack, PreviousKey previousKey, Stack<PreviousKey> previousKeyStack)
        {
            _parent = parent;
            _buffer = destination;
            _span = destination.GetSpan();
            _bufferedLength = 0;
            _state = state;
            _scopeStack = scopeStack;
            _previousKey = previousKey;
            _previousKeyStack = previousKeyStack;
        }

        /// <summary>
        /// Gets the number of bytes that have been written but not yet flushed to the underlying
        /// <see cref="IBufferWriter{T}"/>.
        /// </summary>
        /// <remarks>
        /// If <see cref="BencodeSpanWriter"/> was constructed with a <see cref="Span{T}"/> as the
        /// destination then <see cref="BufferedLength"/> is the number of bytes written to the
        /// <see cref="Span{T}"/>.
        /// </remarks>
        public int BufferedLength => _bufferedLength;

        /// <summary>
        /// Advances the underlying <see cref="IBufferWriter{T}"/>.
        /// </summary>
        /// <param name="final">Indicates whether the value should be complete.</param>
        /// <exception cref="InvalidOperationException"><paramref name="final"/> is
        ///     <see langword="true"/> and the value is incomplete.</exception>
        public void Flush(bool final = true)
        {
            if (final && _state != State.Final)
                throw new InvalidOperationException("The value is incomplete.");

            Flush();
        }

        /// <summary>
        /// Restores the state of the parent writer.
        /// </summary>
        /// <remarks>
        /// It is only necessary to call <see cref="Dispose"/> if the
        /// <see cref="BencodeSpanWriter"/> was created using
        /// <see cref="BencodeWriter.CreateSpanWriter"/>.
        /// </remarks>
        public void Dispose()
        {
            if (_state == State.Disposed)
                return;

            Flush();

            if (_parent != null)
                _parent.RestoreState(_state, _scopeStack, _previousKey);

            _state = State.Disposed;
        }

        /// <summary>
        /// Writes an integer.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows an
        ///     integer to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the integer.</exception>
        public void WriteInteger(long value)
        {
            _state = GetStateAfterValue(_state);

            try
            {
                Span<byte> numberBuffer = stackalloc byte[1 + _longMaxDigits];
                if (!Utf8Formatter.TryFormat(value, numberBuffer, out int numberLength))
                    throw new InvalidOperationException("Unreachable!");

                EnsureCapacity(1 + numberLength + 1);

                var span = _span;
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
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the string.</exception>
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
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the list head.</exception>
        public void WriteListHead()
        {
            _state = EnterListScope(_state, ref _scopeStack);

            try
            {
                EnsureCapacity(1);

                _span[0] = (byte)'l';

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
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the list tail.</exception>
        public void WriteListTail()
        {
            _state = ExitListScope(_state, ref _scopeStack);

            try
            {
                EnsureCapacity(1);

                _span[0] = (byte)'e';

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
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the dictionary head.</exception>
        public void WriteDictionaryHead()
        {
            _state = EnterDictionaryScope(_state, ref _scopeStack);

            if (_previousKeyStack != null)
            {
                _previousKeyStack.Push(_previousKey);
                _previousKey = PreviousKey.None;
            }

            try
            {
                EnsureCapacity(1);

                _span[0] = (byte)'d';

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
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the dictionary tail.</exception>
        public void WriteDictionaryTail()
        {
            _state = ExitDictionaryScope(_state, ref _scopeStack);

            if (_previousKeyStack != null)
            {
                _previousKey = _previousKeyStack.Pop();
            }

            try
            {
                EnsureCapacity(1);

                _span[0] = (byte)'e';

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
        /// <exception cref="InvalidOperationException">The writer reached the end of the
        ///     destination buffer while writing the key.</exception>
        /// <exception cref="InvalidOperationException">The writer was constructed with validation
        ///     enabled and the key being written is mis-ordered or duplicated.</exception>
        public void WriteKey(ReadOnlySpan<byte> key)
        {
            _state = GetStateAfterKey(_state);

            if (_previousKey.HasValue && !IsLess(_previousKey.Span, key))
                throw new InvalidOperationException("Keys must be ordered and unique.");

            if (_previousKeyStack != null)
                _previousKey.CopyFrom(key);

            WriteStringInternal(key);
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
        private void EnsureCapacity(int pendingLength)
        {
            if (_span.Length < pendingLength)
                FlushAndEnsureCapacity(pendingLength);
        }

        private void FlushAndEnsureCapacity(int pendingLength)
        {
            if (_buffer != null)
            {
                if (_bufferedLength > 0)
                {
                    _buffer.Advance(_bufferedLength);
                    _bufferedLength = 0;
                }
                _span = _buffer.GetSpan(pendingLength);
            }

            if (_span.Length < pendingLength)
                throw new InvalidOperationException("Reached the end of the destination buffer while attempting to write.");
        }

        private void FlushAndRequestCapacity(int pendingLength)
        {
            if (_buffer != null)
            {
                if (_bufferedLength > 0)
                {
                    _buffer.Advance(_bufferedLength);
                    _bufferedLength = 0;
                }
                _span = _buffer.GetSpan(pendingLength);
            }

            if (_span.Length == 0)
                throw new InvalidOperationException("Reached the end of the destination buffer while attempting to write.");
        }

        private void Flush()
        {
            if (_buffer != null)
            {
                if (_bufferedLength > 0)
                {
                    _buffer.Advance(_bufferedLength);
                    _bufferedLength = 0;
                }
                _span = Span<byte>.Empty;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Advance(int length)
        {
            _span = _span.Slice(length);
            _bufferedLength += length;
        }

        private void WriteStringInternal(ReadOnlySpan<byte> value)
        {
            try
            {
                Span<byte> numberBuffer = stackalloc byte[_intMaxDigits];
                if (!Utf8Formatter.TryFormat(value.Length, numberBuffer, out int numberLength))
                    throw new InvalidOperationException("Unreachable!");

                EnsureCapacity(numberLength + 1);

                var span = _span;
                numberBuffer.Slice(0, numberLength).CopyTo(span);
                span = span.Slice(numberLength);
                span[0] = (byte)':';

                Advance(numberLength + 1);

                WriteIncrementally(value);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        private void WriteIncrementally(ReadOnlySpan<byte> value)
        {
            for (; ; )
            {
                Span<byte> span = _span;
                int length = span.Length;

                if (length >= value.Length)
                {
                    value.CopyTo(span);
                    Advance(value.Length);
                    return;
                }

                value.Slice(0, length).CopyTo(span);
                Advance(length);

                value = value.Slice(length);

                FlushAndRequestCapacity(0);
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
            Disposed,
        }

        internal struct PreviousKey
        {
            public static readonly PreviousKey None = default;

            private byte[] _array;

            private int _length;

            public bool HasValue => _array != null;

            public ReadOnlySpan<byte> Span => _array.AsSpan(0, _length);

            public void CopyFrom(ReadOnlySpan<byte> key)
            {
                if (_array == null || key.Length > _array.Length)
                    _array = new byte[key.Length];

                key.CopyTo(_array);
                _length = key.Length;
            }
        }
    }
}
