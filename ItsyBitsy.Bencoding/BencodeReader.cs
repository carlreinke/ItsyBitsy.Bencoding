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
using System.Collections.Generic;
using static ItsyBitsy.Bencoding.BencodeSpanReader;

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// A reader that provides access to bencoded data.
    /// </summary>
    /// <seealso cref="BencodeSpanReader"/>
    public sealed class BencodeReader
    {
        private readonly ReadOnlyMemory<byte> _memory;

        private int _index;

        private State _state;

        private BitStack _scopeStack;

        private int _stringHeadLength;

        private int _stringLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="BencodeReader"/> class using the specified
        /// encoded data.
        /// </summary>
        /// <param name="source">The memory containing encoded data.</param>
        public BencodeReader(ReadOnlyMemory<byte> source)
        {
            _memory = source;
        }

        /// <summary>
        /// Gets or sets the position of the reader in the source memory.
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
            get => _index;
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
        /// Gets a <see cref="BencodeSpanReader"/> that continues from the current state of this
        /// reader.
        /// </summary>
        /// <returns>A <see cref="BencodeSpanReader"/> that continues from the current state of this
        ///     reader.</returns>
        /// <remarks>
        /// Do not operate on this reader again until after calling
        /// <see cref="BencodeSpanReader.Dispose"/> on the <see cref="BencodeSpanReader"/>.
        /// </remarks>
        public BencodeSpanReader CreateSpanReader()
        {
            var spanReader = new BencodeSpanReader(this, _memory.Span, _index, _state, _scopeStack);

            _state = State.Error;

            return spanReader;
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
                return ReadTokenTypeInternal(_memory.Span, ref _index, _state);
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
                SkipValueInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);
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
                ReadValueToInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength, ref writer);
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
                ReadValueToInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength, writer);
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
                return ReadIntegerInternal(_memory.Span, ref _index);
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
                ReadStringLengthInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);

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
        public ReadOnlyMemory<byte> ReadString()
        {
            _state = GetStateAfterValue(_state);

            try
            {
                var span = ReadStringInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);
                return _memory.Slice(span);
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
        public List<int> ReadList()
        {
            ReadListHead();

            var list = new List<int>();

            while (ReadTokenType() != BencodeTokenType.ListTail)
            {
                list.Add(Position);

                SkipValue();
            }

            ReadListTail();

            return list;
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
                ReadListHeadInternal(_memory.Span, ref _index);
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
                ReadListTailInternal(_memory.Span, ref _index);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads a dictionary and returns a <see cref="BencodeDictionary"/>.
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
        public BencodeDictionary ReadDictionary(bool skipDuplicateKeys = false)
        {
            ReadDictionaryHead();

            var builder = new BencodeDictionary.Builder();

            int duplicateKeyPosition = -1;

            while (ReadTokenType() != BencodeTokenType.DictionaryTail)
            {
                int keyPosition = Position;

                ReadOnlyMemory<byte> key = ReadKey();

                if (!(builder.TryAdd(key, Position) || skipDuplicateKeys))
                    if (duplicateKeyPosition == -1)
                        duplicateKeyPosition = keyPosition;

                SkipValue();
            }

            ReadDictionaryTail();

            if (duplicateKeyPosition != -1)
                throw new InvalidBencodeException("The keys of the dictionary are not unique.", duplicateKeyPosition);

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
                ReadDictionaryHeadInternal(_memory.Span, ref _index);
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
                ReadDictionaryTailInternal(_memory.Span, ref _index);
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
                _ = ReadStringInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);
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
                ReadStringLengthInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);

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
        public ReadOnlyMemory<byte> ReadKey()
        {
            _state = GetStateAfterKey(_state);

            try
            {
                var span = ReadStringInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);
                return _memory.Slice(span);
            }
            catch
            {
                _state = State.Error;
                throw;
            }
        }

        internal void RestoreState(int index, State state, BitStack scopeStack)
        {
            _index = index;
            _state = state;
            _scopeStack = scopeStack;
            _stringHeadLength = 0;
            _stringLength = 0;
        }
    }
}
