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

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// Represents a writer that provides generation of bencoded data.
    /// </summary>
    /// <remarks>
    /// <see cref="BencodeSpanWriter"/> does not validate the order of keys. 
    /// </remarks>
    public ref struct BencodeSpanWriter
    {
        private readonly Span<byte> _span;

        private int _index;

        private State _state;

        private BitStack _scopeStack;

        /// <summary>
        /// Initializes a new <see cref="BencodeSpanWriter"/> instance using the specified
        /// destination buffer.
        /// </summary>
        /// <param name="destination">The span to write the encoded data into.</param>
        public BencodeSpanWriter(Span<byte> destination)
        {
            _span = destination;
            _index = 0;
            _state = State.Initial;
            _scopeStack = new BitStack();
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
                WriteIntegerInternal(_span, ref _index, value);
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

            try
            {
                WriteStringInternal(_span, ref _index, value);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
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
                WriteListHeadInternal(_span, ref _index);
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
                WriteListTailInternal(_span, ref _index);
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

            try
            {
                WriteDictionaryHeadInternal(_span, ref _index);
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

            try
            {
                WriteDictionaryTailInternal(_span, ref _index);
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

            try
            {
                WriteStringInternal(_span, ref _index, key);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
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

        internal static void WriteIntegerInternal(Span<byte> span, ref int index, long value)
        {
            const string exceptionMessage = "Reached the end of the destination buffer while writing an integer.";

            if (span.Length - index < 1)
                throw new ArgumentException(exceptionMessage);

            span[index] = (byte)'i';
            index += 1;

            if (!Utf8Formatter.TryFormat(value, span.Slice(index), out int bytesWritten))
                throw new ArgumentException(exceptionMessage);
            index += bytesWritten;

            if (span.Length - index < 1)
                throw new ArgumentException(exceptionMessage);

            span[index] = (byte)'e';
            index += 1;
        }

        internal static void WriteStringInternal(Span<byte> span, ref int index, ReadOnlySpan<byte> value)
        {
            const string exceptionMessage = "Reached the end of the destination buffer while writing a string.";

            if (!Utf8Formatter.TryFormat(value.Length, span.Slice(index), out int bytesWritten))
                throw new ArgumentException(exceptionMessage);
            index += bytesWritten;

            if (span.Length - index < 1 + value.Length)
                throw new ArgumentException(exceptionMessage);

            span[index] = (byte)':';
            index += 1;

            value.CopyTo(span.Slice(index));
            index += value.Length;
        }

        internal static void WriteListHeadInternal(Span<byte> span, ref int index)
        {
            if (span.Length - index < 1)
                throw new ArgumentException("Reached the end of the destination buffer while writing a list head.");

            span[index] = (byte)'l';
            index += 1;
        }

        internal static void WriteListTailInternal(Span<byte> span, ref int index)
        {
            if (span.Length - index < 1)
                throw new ArgumentException("Reached the end of the destination buffer while writing a list tail.");

            span[index] = (byte)'e';
            index += 1;
        }

        internal static void WriteDictionaryHeadInternal(Span<byte> span, ref int index)
        {
            if (span.Length - index < 1)
                throw new ArgumentException("Reached the end of the destination buffer while writing a dictionary head.");

            span[index] = (byte)'d';
            index += 1;
        }

        internal static void WriteDictionaryTailInternal(Span<byte> span, ref int index)
        {
            if (span.Length - index < 1)
                throw new ArgumentException("Reached the end of the destination buffer while writing a dictionary tail.");

            span[index] = (byte)'e';
            index += 1;
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
    }
}
