﻿//
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
    /// Represents a reader that provides access to bencoded data.
    /// </summary>
    public ref struct BencodeSpanReader
    {
        private readonly BencodeReader? _parent;

        private readonly ReadOnlySpan<byte> _span;

        private int _index;

        private State _state;

        private BitStack _scopeStack;

        private int _stringHeadLength;

        private int _stringLength;

        /// <summary>
        /// Initializes a new <see cref="BencodeSpanReader"/> instance using the specified encoded
        /// data.
        /// </summary>
        /// <param name="source">The span to read encoded data from.</param>
        public BencodeSpanReader(ReadOnlySpan<byte> source)
        {
            _parent = null;
            _span = source;
            _index = 0;
            _state = State.Initial;
            _scopeStack = new BitStack();
            _stringHeadLength = 0;
            _stringLength = 0;
        }

        internal BencodeSpanReader(BencodeReader parent, ReadOnlySpan<byte> source, int index, State state, BitStack scopeStack)
        {
            _parent = parent;
            _span = source;
            _index = index;
            _state = state;
            _scopeStack = scopeStack;
            _stringHeadLength = 0;
            _stringLength = 0;
        }

        /// <summary>
        /// Gets or sets the position of the reader in the source span.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.
        ///     </exception>
        /// <remarks>
        /// Setting the position puts the reader into a state in which it can read a value.  For
        /// example, if the position of the reader is set to the position of an element in a list,
        /// the reader will be able to read only that element and not any subsequent elements.  If
        /// the position of the reader is set to position of a key, the reader will be able to read
        /// the key as a value using <see cref="ReadString"/> rather than <see cref="ReadKey"/>.
        /// </remarks>
        public int Position
        {
            readonly get => _index;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _index = value;
                _state = State.Initial;
                _scopeStack.Clear();
                _stringHeadLength = 0;
                _stringLength = 0;
            }
        }

        /// <summary>
        /// Restores the state of the parent reader.
        /// </summary>
        /// <remarks>
        /// It is only necessary to call <see cref="Dispose"/> if the
        /// <see cref="BencodeSpanReader"/> was created using
        /// <see cref="BencodeReader.CreateSpanReader"/>.
        /// </remarks>
        public void Dispose()
        {
            if (_state == State.Disposed)
                return;

            if (_parent != null)
                _parent.RestoreState(_index, _state, _scopeStack);

            _state = State.Disposed;
        }

        /// <summary>
        /// Determines the type of token that the reader must read next.
        /// </summary>
        /// <returns>The type of token that the reader must read next.</returns>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a token discriminator.</exception>
        /// <remarks>
        /// <para>
        /// <see cref="ReadTokenType()"/> will continue to return the same result until the token is
        /// read or skipped.
        /// </para>
        /// <para>
        /// <see cref="ReadTokenType()"/> returns <see cref="BencodeTokenType.None"/> if the reader
        /// has previously encountered an error or if the reader is positioned after the end of the
        /// value.
        /// </para>
        /// </remarks>
        public BencodeTokenType ReadTokenType()
        {
            try
            {
                return ReadTokenTypeInternal(_span, ref _index, _state);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Skips a value.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     value to be skipped.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to skip a value.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered an integer that is
        ///     not in the supported range.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a string with a
        ///     length that is not in the supported range.</exception>
        public void SkipValue()
        {
            _state = GetStateAfterValue(_state);

            try
            {
                SkipValueInternal(_span, ref _index, ref _stringHeadLength, ref _stringLength);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads a value and writes it to a bencode writer.
        /// </summary>
        /// <param name="writer">The bencode writer that the value read from the reader is written
        ///     to.</param>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     value to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a value.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered an integer that is
        ///     not in the supported range.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a string with a
        ///     length that is not in the supported range.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     value to be written.</exception>
        public void ReadValueTo(ref BencodeSpanWriter writer)
        {
            _state = GetStateAfterValue(_state);

            try
            {
                ReadValueToInternal(_span, ref _index, ref _stringHeadLength, ref _stringLength, ref writer);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads a value and writes it to a bencode writer.
        /// </summary>
        /// <param name="writer">The bencode writer that the value read from the reader is written
        ///     to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is
        ///     <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     value to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a value.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered an integer that is
        ///     not in the supported range.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a string with a
        ///     length that is not in the supported range.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     value to be written.</exception>
        public void ReadValueTo(BencodeWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            _state = GetStateAfterValue(_state);

            try
            {
                ReadValueToInternal(_span, ref _index, ref _stringHeadLength, ref _stringLength, writer);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads an integer.
        /// </summary>
        /// <returns>The integer that was read.</returns>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows an
        ///     integer to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read an integer.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered an integer that is
        ///     not in the supported range.</exception>
        public long ReadInteger()
        {
            _state = GetStateAfterValue(_state);

            try
            {
                return ReadIntegerInternal(_span, ref _index);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Determines the length of the string to be read.
        /// </summary>
        /// <returns>The length of the string.</returns>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     string to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a string.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a string with a
        ///     length that is not in the supported range.</exception>
        /// <remarks>
        /// <see cref="ReadStringLength()"/> will continue to return the same result until the
        /// string is read or skipped.
        /// </remarks>
        public int ReadStringLength()
        {
            _ = GetStateAfterValue(_state);

            try
            {
                ReadStringLengthInternal(_span, ref _index, ref _stringHeadLength, ref _stringLength);

                return _stringLength;
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads a string.
        /// </summary>
        /// <returns>The string that was read.</returns>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     string to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a string.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a string with a
        ///     length that is not in the supported range.</exception>
        public ReadOnlySpan<byte> ReadString()
        {
            _state = GetStateAfterValue(_state);

            try
            {
                return ReadStringInternal(_span, ref _index, ref _stringHeadLength, ref _stringLength);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads a list and returns the positions of the elements.
        /// </summary>
        /// <returns>The list of positions of the list element values.</returns>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     list to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a value.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered an integer that is
        ///     not in the supported range.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a string with a
        ///     length that is not in the supported range.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        public BencodeList ReadList()
        {
            ReadListHead();

            var builder = new BencodeList.Builder();

            while (ReadTokenType() != BencodeTokenType.ListTail)
            {
                builder.Add(_index);

                SkipValue();
            }

            ReadListTail();

            return builder.ToList();
        }

        /// <summary>
        /// Reads the beginning of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     list head to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read the beginning of a list.</exception>
        public void ReadListHead()
        {
            _state = EnterListScope(_state, ref _scopeStack);

            try
            {
                ReadListHeadInternal(_span, ref _index);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads the end of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     list tail to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read the end of a list.</exception>
        public void ReadListTail()
        {
            _state = ExitListScope(_state, ref _scopeStack);

            try
            {
                ReadListTailInternal(_span, ref _index);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads a dictionary and returns a <see cref="BencodeSpanDictionary"/>.
        /// </summary>
        /// <param name="skipDuplicateKeys">If <see langword="true"/>, duplicate keys are skipped
        ///     rather than causing <see cref="InvalidBencodeException"/> to be thrown.</param>
        /// <returns>The dictionary.</returns>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     dictionary to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered a duplicate key while
        ///     reading the dictionary.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a value.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered an integer that is
        ///     not in the supported range.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a string with a
        ///     length that is not in the supported range.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        public BencodeSpanDictionary ReadDictionary(bool skipDuplicateKeys = false)
        {
            ReadDictionaryHead();

            var builder = new BencodeSpanDictionary.Builder(_span);

            int duplicateKeyIndex = -1;

            while (ReadTokenType() != BencodeTokenType.DictionaryTail)
            {
                int keyIndex = _index;

                ReadOnlySpan<byte> key = ReadKey();

                if (!(builder.TryAdd(_index - key.Length, key.Length) || skipDuplicateKeys))
                    if (duplicateKeyIndex == -1)
                        duplicateKeyIndex = keyIndex;

                SkipValue();
            }

            ReadDictionaryTail();

            if (duplicateKeyIndex != -1)
                throw new InvalidBencodeException("The keys of the dictionary are not unique.", duplicateKeyIndex);

            return builder.ToDictionary();
        }

        /// <summary>
        /// Reads the beginning of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     dictionary head to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read the beginning of a dictionary.</exception>
        public void ReadDictionaryHead()
        {
            _state = EnterDictionaryScope(_state, ref _scopeStack);

            try
            {
                ReadDictionaryHeadInternal(_span, ref _index);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads the end of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     dictionary tail to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read the end of a dictionary.</exception>
        public void ReadDictionaryTail()
        {
            _state = ExitDictionaryScope(_state, ref _scopeStack);

            try
            {
                ReadDictionaryTailInternal(_span, ref _index);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Skips a key.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     key to be skipped.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to skip a key.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        public void SkipKey()
        {
            _state = GetStateAfterKey(_state);

            try
            {
                _ = ReadStringInternal(_span, ref _index, ref _stringHeadLength, ref _stringLength);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Determines the length of the key to be read.
        /// </summary>
        /// <returns>The length of the key.</returns>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     key to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a key.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        /// <remarks>
        /// <see cref="ReadKeyLength()"/> will continue to return the same result until the key is
        /// read or skipped.
        /// </remarks>
        public int ReadKeyLength()
        {
            _ = GetStateAfterKey(_state);

            try
            {
                ReadStringLengthInternal(_span, ref _index, ref _stringHeadLength, ref _stringLength);

                return _stringLength;
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads a key.
        /// </summary>
        /// <returns>The key that was read.</returns>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     key to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a key.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        public ReadOnlySpan<byte> ReadKey()
        {
            _state = GetStateAfterKey(_state);

            try
            {
                return ReadStringInternal(_span, ref _index, ref _stringHeadLength, ref _stringLength);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        internal static BencodeTokenType ReadTokenTypeInternal(ReadOnlySpan<byte> span, ref int index, State state)
        {
            int b = PeekByte(span, index);

            switch (state)
            {
                case State.Initial:
                case State.DictionaryValue:
                    switch (b)
                    {
                        case 'd':
                            return BencodeTokenType.DictionaryHead;
                        case 'i':
                            return BencodeTokenType.Integer;
                        case 'l':
                            return BencodeTokenType.ListHead;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            return BencodeTokenType.String;
                        default:
                            index += 1;
                            goto case -1;
                        case -1:
                            throw MismatchError("'d', 'i', 'l', or '0'-'9'", b, index);
                    }
                case State.DictionaryKey:
                    switch (b)
                    {
                        case 'e':
                            return BencodeTokenType.DictionaryTail;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            return BencodeTokenType.Key;
                        default:
                            index += 1;
                            goto case -1;
                        case -1:
                            throw MismatchError("'e' or '0'-'9'", b, index);
                    }
                case State.ListItem:
                    switch (b)
                    {
                        case 'd':
                            return BencodeTokenType.DictionaryHead;
                        case 'e':
                            return BencodeTokenType.ListTail;
                        case 'i':
                            return BencodeTokenType.Integer;
                        case 'l':
                            return BencodeTokenType.ListHead;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            return BencodeTokenType.String;
                        default:
                            index += 1;
                            goto case -1;
                        case -1:
                            throw MismatchError("'d', 'e', 'i', 'l', or '0'-'9'", b, index);
                    }
                case State.Final:
                    return BencodeTokenType.None;
                case State.Error:
                default:
                    return BencodeTokenType.None;
            }

            static Exception MismatchError(string expected, int unexpected, int position)
            {
                string message = unexpected == -1 ? $"Expected {expected} but reached the end of the source buffer while reading the token type."
                        : IsPrintable(unexpected) ? $"Expected {expected} but found '{(char)unexpected}' while reading the token type."
                                                  : $"Expected {expected} but found '\\x{unexpected:X02}' while reading the token type.";
                return new InvalidBencodeException(message, unexpected == -1 ? position : position - 1);
            }
        }

        internal static State GetStateAfterValue(State state)
        {
            return state switch
            {
                State.Initial => State.Final,
                State.ListItem => State.ListItem,
                State.DictionaryValue => State.DictionaryKey,
                _ => throw new InvalidOperationException("The reader is not in a state that allows a value to be read."),
            };
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
                    throw new InvalidOperationException("The reader is not in a state that allows a list head to be read.");
            }

            return State.ListItem;
        }

        internal static State ExitListScope(State state, ref BitStack scopeStack)
        {
            return state switch
            {
                State.ListItem => scopeStack.Count == 0 ? State.Final :
                                  scopeStack.Pop() ? State.DictionaryKey : State.ListItem,
                _ => throw new InvalidOperationException("The reader is not in a state that allows a list tail to be read."),
            };
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
                    throw new InvalidOperationException("The reader is not in a state that allows a dictionary head to be read.");
            }

            return State.DictionaryKey;
        }

        internal static State ExitDictionaryScope(State state, ref BitStack scopeStack)
        {
            return state switch
            {
                State.DictionaryKey => scopeStack.Count == 0 ? State.Final :
                                       scopeStack.Pop() ? State.DictionaryKey : State.ListItem,
                _ => throw new InvalidOperationException("The reader is not in a state that allows a dictionary tail to be read."),
            };
        }

        internal static State GetStateAfterKey(State state)
        {
            return state switch
            {
                State.DictionaryKey => State.DictionaryValue,
                _ => throw new InvalidOperationException("The reader is not in a state that allows a key to be read."),
            };
        }

        internal static void SkipValueInternal(ReadOnlySpan<byte> span, ref int index, ref int stringHeadLength, ref int stringLength)
        {
            State state = State.Initial;
            BitStack scopeStack = default;

            do
            {
                var tokenType = ReadTokenTypeInternal(span, ref index, state);

                switch (tokenType)
                {
                    case BencodeTokenType.Integer:
                        state = GetStateAfterValue(state);
                        _ = ReadIntegerInternal(span, ref index);
                        break;
                    case BencodeTokenType.String:
                        state = GetStateAfterValue(state);
                        _ = ReadStringInternal(span, ref index, ref stringHeadLength, ref stringLength);
                        break;
                    case BencodeTokenType.ListHead:
                        state = EnterListScope(state, ref scopeStack);
                        ReadListHeadInternal(span, ref index);
                        break;
                    case BencodeTokenType.ListTail:
                        state = ExitListScope(state, ref scopeStack);
                        ReadListTailInternal(span, ref index);
                        break;
                    case BencodeTokenType.DictionaryHead:
                        state = EnterDictionaryScope(state, ref scopeStack);
                        ReadDictionaryHeadInternal(span, ref index);
                        break;
                    case BencodeTokenType.DictionaryTail:
                        state = ExitDictionaryScope(state, ref scopeStack);
                        ReadDictionaryTailInternal(span, ref index);
                        break;
                    case BencodeTokenType.Key:
                        state = GetStateAfterKey(state);
                        _ = ReadStringInternal(span, ref index, ref stringHeadLength, ref stringLength);
                        break;
                    default:
                        throw new UnreachableException();
                }
            }
            while (state != State.Final);
        }

        internal static void ReadValueToInternal(ReadOnlySpan<byte> span, ref int index, ref int stringHeadLength, ref int stringLength, ref BencodeSpanWriter writer)
        {
            State state = State.Initial;
            BitStack scopeStack = default;

            do
            {
                var tokenType = ReadTokenTypeInternal(span, ref index, state);

                switch (tokenType)
                {
                    case BencodeTokenType.Integer:
                        state = GetStateAfterValue(state);
                        long integer = ReadIntegerInternal(span, ref index);
                        writer.WriteInteger(integer);
                        break;
                    case BencodeTokenType.String:
                        state = GetStateAfterValue(state);
                        var @string = ReadStringInternal(span, ref index, ref stringHeadLength, ref stringLength);
                        writer.WriteString(@string);
                        break;
                    case BencodeTokenType.ListHead:
                        state = EnterListScope(state, ref scopeStack);
                        ReadListHeadInternal(span, ref index);
                        writer.WriteListHead();
                        break;
                    case BencodeTokenType.ListTail:
                        state = ExitListScope(state, ref scopeStack);
                        ReadListTailInternal(span, ref index);
                        writer.WriteListTail();
                        break;
                    case BencodeTokenType.DictionaryHead:
                        state = EnterDictionaryScope(state, ref scopeStack);
                        ReadDictionaryHeadInternal(span, ref index);
                        writer.WriteDictionaryHead();
                        break;
                    case BencodeTokenType.DictionaryTail:
                        state = ExitDictionaryScope(state, ref scopeStack);
                        ReadDictionaryTailInternal(span, ref index);
                        writer.WriteDictionaryTail();
                        break;
                    case BencodeTokenType.Key:
                        state = GetStateAfterKey(state);
                        var key = ReadStringInternal(span, ref index, ref stringHeadLength, ref stringLength);
                        writer.WriteKey(key);
                        break;
                    default:
                        throw new UnreachableException();
                }
            }
            while (state != State.Final);
        }

        internal static void ReadValueToInternal(ReadOnlySpan<byte> span, ref int index, ref int stringHeadLength, ref int stringLength, BencodeWriter writer)
        {
            State state = State.Initial;
            BitStack scopeStack = default;

            do
            {
                var tokenType = ReadTokenTypeInternal(span, ref index, state);

                switch (tokenType)
                {
                    case BencodeTokenType.Integer:
                        state = GetStateAfterValue(state);
                        long integer = ReadIntegerInternal(span, ref index);
                        writer.WriteInteger(integer);
                        break;
                    case BencodeTokenType.String:
                        state = GetStateAfterValue(state);
                        var @string = ReadStringInternal(span, ref index, ref stringHeadLength, ref stringLength);
                        writer.WriteString(@string);
                        break;
                    case BencodeTokenType.ListHead:
                        state = EnterListScope(state, ref scopeStack);
                        ReadListHeadInternal(span, ref index);
                        writer.WriteListHead();
                        break;
                    case BencodeTokenType.ListTail:
                        state = ExitListScope(state, ref scopeStack);
                        ReadListTailInternal(span, ref index);
                        writer.WriteListTail();
                        break;
                    case BencodeTokenType.DictionaryHead:
                        state = EnterDictionaryScope(state, ref scopeStack);
                        ReadDictionaryHeadInternal(span, ref index);
                        writer.WriteDictionaryHead();
                        break;
                    case BencodeTokenType.DictionaryTail:
                        state = ExitDictionaryScope(state, ref scopeStack);
                        ReadDictionaryTailInternal(span, ref index);
                        writer.WriteDictionaryTail();
                        break;
                    case BencodeTokenType.Key:
                        state = GetStateAfterKey(state);
                        var key = ReadStringInternal(span, ref index, ref stringHeadLength, ref stringLength);
                        writer.WriteKey(key);
                        break;
                    default:
                        throw new UnreachableException();
                }
            }
            while (state != State.Final);
        }

        internal static long ReadIntegerInternal(ReadOnlySpan<byte> span, ref int index)
        {
            int b = ReadByte(span, ref index);

            if (b != 'i')
                throw MismatchError("'i'", b, index);

            int integerIndex = index;

            b = ReadByte(span, ref index);

            bool isNegative = false;
            if (b == -1)
                throw MismatchError("'-' or '0'-'9'", b, index);
            if (b == '-')
            {
                isNegative = true;

                b = ReadByte(span, ref index);
            }
            if (b == '0')
            {
                if (isNegative)
                    throw MismatchError("'1'-'9'", b, index);

                b = ReadByte(span, ref index);
            }
            else
            {
                if (b < '1' || b > '9')
                    throw MismatchError(isNegative ? "'1'-'9'" : "'0'-'9'", b, index);

                b = ReadByte(span, ref index);

                while (b != 'e')
                {
                    if (b < '0' || b > '9')
                        throw MismatchError("'e' or '0'-'9'", b, index);

                    b = ReadByte(span, ref index);
                }
            }

            if (b != 'e')
                throw MismatchError("'e'", b, index);

            var integerSpan = span.Slice(integerIndex, index - 1 - integerIndex);

            long value;

            if (!Utf8Parser.TryParse(integerSpan, out value, out int bytesConsumed) || bytesConsumed != integerSpan.Length)
                throw new UnsupportedBencodeException($"The integer is not in the supported range from '{long.MinValue}' to '{long.MaxValue}'.", integerIndex - 1);

            return value;

            static Exception MismatchError(string expected, int unexpected, int position)
            {
                string message = unexpected == -1 ? $"Expected {expected} but reached the end of the source buffer while reading an integer."
                        : IsPrintable(unexpected) ? $"Expected {expected} but found '{(char)unexpected}' while reading an integer."
                                                  : $"Expected {expected} but found '\\x{unexpected:X02}' while reading an integer.";
                return new InvalidBencodeException(message, unexpected == -1 ? position : position - 1);
            }
        }

        internal static void ReadStringLengthInternal(ReadOnlySpan<byte> span, ref int index, ref int stringHeadLength, ref int stringLength)
        {
            if (stringHeadLength != 0)
                return;

            int integerIndex = index;

            int b = ReadByte(span, ref index);

            if (b == '0')
            {
                b = ReadByte(span, ref index);
            }
            else
            {
                if (b < '1' || b > '9')
                    throw MismatchError("'0'-'9'", b, index);

                b = ReadByte(span, ref index);

                while (b != ':')
                {
                    if (b < '0' || b > '9')
                        throw MismatchError("'0'-'9' or ':'", b, index);

                    b = ReadByte(span, ref index);
                }
            }

            if (b != ':')
                throw MismatchError("':'", b, index);

            var integerSpan = span.Slice(integerIndex, index - 1 - integerIndex);

            index = integerIndex;

            int length;

            if (!Utf8Parser.TryParse(integerSpan, out length, out int bytesConsumed) || bytesConsumed != integerSpan.Length)
                throw new UnsupportedBencodeException($"The string length is not in the supported range from '0' to '{int.MaxValue}'.", integerIndex);

            stringHeadLength = integerSpan.Length + 1;
            stringLength = length;

            static Exception MismatchError(string expected, int unexpected, int position)
            {
                string message = unexpected == -1 ? $"Expected {expected} but reached the end of the source buffer while reading a string."
                        : IsPrintable(unexpected) ? $"Expected {expected} but found '{(char)unexpected}' while reading a string."
                                                  : $"Expected {expected} but found '\\x{unexpected:X02}' while reading a string.";
                return new InvalidBencodeException(message, unexpected == -1 ? position : position - 1);
            }
        }

        internal static ReadOnlySpan<byte> ReadStringInternal(ReadOnlySpan<byte> span, ref int index, ref int stringHeadLength, ref int stringLength)
        {
            ReadStringLengthInternal(span, ref index, ref stringHeadLength, ref stringLength);

            index += stringHeadLength;
            stringHeadLength = 0;

            if (span.Length - index < stringLength)
            {
                index = span.Length;
                throw new InvalidBencodeException($"Expected to read {stringLength} bytes but reached the end of the source buffer while reading a string.", index);
            }

            var value = span.Slice(index, stringLength);

            index += stringLength;
            stringLength = 0;

            return value;
        }

        internal static void ReadListHeadInternal(ReadOnlySpan<byte> span, ref int index)
        {
            int b = ReadByte(span, ref index);

            if (b != 'l')
                throw MismatchError("'l'", b, index);

            static Exception MismatchError(string expected, int unexpected, int position)
            {
                string message = unexpected == -1 ? $"Expected {expected} but reached the end of the source buffer while reading a list head."
                        : IsPrintable(unexpected) ? $"Expected {expected} but found '{(char)unexpected}' while reading a list head."
                                                  : $"Expected {expected} but found '\\x{unexpected:X02}' while reading a list head.";
                return new InvalidBencodeException(message, unexpected == -1 ? position : position - 1);
            }
        }

        internal static void ReadListTailInternal(ReadOnlySpan<byte> span, ref int index)
        {
            int b = ReadByte(span, ref index);

            if (b != 'e')
                throw MismatchError("'e'", b, index);

            static Exception MismatchError(string expected, int unexpected, int position)
            {
                string message = unexpected == -1 ? $"Expected {expected} but reached the end of the source buffer while reading a list tail."
                        : IsPrintable(unexpected) ? $"Expected {expected} but found '{(char)unexpected}' while reading a list tail."
                                                  : $"Expected {expected} but found '\\x{unexpected:X02}' while reading a list tail.";
                return new InvalidBencodeException(message, unexpected == -1 ? position : position - 1);
            }
        }

        internal static void ReadDictionaryHeadInternal(ReadOnlySpan<byte> span, ref int index)
        {
            int b = ReadByte(span, ref index);

            if (b != 'd')
                throw MismatchError("'d'", b, index);

            static Exception MismatchError(string expected, int unexpected, int position)
            {
                string message = unexpected == -1 ? $"Expected {expected} but reached the end of the source buffer while reading a dictionary head."
                        : IsPrintable(unexpected) ? $"Expected {expected} but found '{(char)unexpected}' while reading a dictionary head."
                                                  : $"Expected {expected} but found '\\x{unexpected:X02}' while reading a dictionary head.";
                return new InvalidBencodeException(message, unexpected == -1 ? position : position - 1);
            }
        }

        internal static void ReadDictionaryTailInternal(ReadOnlySpan<byte> span, ref int index)
        {
            int b = ReadByte(span, ref index);

            if (b != 'e')
                throw MismatchError("'e'", b, index);

            static Exception MismatchError(string expected, int unexpected, int position)
            {
                string message = unexpected == -1 ? $"Expected {expected} but reached the end of the source buffer while reading a dictionary tail."
                        : IsPrintable(unexpected) ? $"Expected {expected} but found '{(char)unexpected}' while reading a dictionary tail."
                                                  : $"Expected {expected} but found '\\x{unexpected:X02}' while reading a dictionary tail.";
                return new InvalidBencodeException(message, unexpected == -1 ? position : position - 1);
            }
        }

        private static int PeekByte(ReadOnlySpan<byte> span, int index)
        {
            if (index >= span.Length)
                return -1;

            return span[index];
        }

        private static int ReadByte(ReadOnlySpan<byte> span, ref int index)
        {
            if (index >= span.Length)
                return -1;

            byte b = span[index];
            index += 1;
            return b;
        }

        private static bool IsPrintable(int b)
        {
            return b >= ' ' && b <= '~';
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
    }
}
