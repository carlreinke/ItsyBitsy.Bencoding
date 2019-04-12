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

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// A reader that provides access to bencoded data.
    /// </summary>
    public sealed class BencodeReader : IBencodeReader
    {
        private readonly ReadOnlyMemory<byte> _memory;

        private int _index;

        private BencodeSpanReader.State _state;

        private int _stringHeadLength;

        private int _stringLength;

        private BitStack _scopeStack;

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
        /// Gets a indicator of whether the reader position can be set.
        /// </summary>
        /// <returns><see langword="true"/>.</returns>
        public bool CanSeek => true;

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
                _state = BencodeSpanReader.State.Initial;
                _stringHeadLength = 0;
                _stringLength = 0;
                _scopeStack.Clear();
            }
        }

        long IBencodeReader.Position
        {
            get => Position;
            set
            {
                if (value < 0 || value > int.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                Position = (int)value;
            }
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
                return BencodeSpanReader.ReadTokenTypeInternal(_memory.Span, ref _index, _state);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
            _state = BencodeSpanReader.GetStateAfterValue(_state);

            try
            {
                BencodeSpanReader.SkipValueInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
            _state = BencodeSpanReader.GetStateAfterValue(_state);

            try
            {
                BencodeSpanReader.ReadValueToInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength, ref writer);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
        public void ReadValueTo(IBencodeWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            _state = BencodeSpanReader.GetStateAfterValue(_state);

            try
            {
                BencodeSpanReader.ReadValueToInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength, writer);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
            _state = BencodeSpanReader.GetStateAfterValue(_state);

            try
            {
                return BencodeSpanReader.ReadIntegerInternal(_memory.Span, ref _index);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
            _ = BencodeSpanReader.GetStateAfterValue(_state);

            try
            {
                BencodeSpanReader.ReadStringLengthInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);

                return _stringLength;
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
            return _memory.Slice(ReadStringSpan());
        }

        /// <summary>
        /// Attempts to read a string into a buffer.
        /// </summary>
        /// <param name="destination">The buffer that the string will be written into.</param>
        /// <param name="bytesWritten">Returns the length of the string that was written into the
        ///     buffer or 0 if the length of the string is greater than the length of the buffer.
        ///     </param>
        /// <returns><see langword="false"/> if the length of the string is greater than the length
        ///     of the buffer; otherwise, <see langword="true"/>.</returns>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     string to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a string.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a string with a
        ///     length that is not in the supported range.</exception>
        public bool TryReadString(Span<byte> destination, out int bytesWritten)
        {
            int length = ReadStringLength();
            if (length > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }

            ReadOnlySpan<byte> value = ReadStringSpan();
            value.CopyTo(destination);
            bytesWritten = value.Length;
            return true;
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
            _state = BencodeSpanReader.EnterListScope(_state, ref _scopeStack);

            try
            {
                BencodeSpanReader.ReadListHeadInternal(_memory.Span, ref _index);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
            _state = BencodeSpanReader.ExitListScope(_state, ref _scopeStack);

            try
            {
                BencodeSpanReader.ReadListTailInternal(_memory.Span, ref _index);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads a dictionary and returns a <see cref="BencodeDictionary{TPosition}"/>.
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
        public BencodeDictionary<int> ReadDictionary(bool skipDuplicateKeys = false)
        {
            ReadDictionaryHead();

            var dictionary = new BencodeDictionary<int>();

            int duplicateKeyPosition = -1;

            while (ReadTokenType() != BencodeTokenType.DictionaryTail)
            {
                int keyPosition = Position;

                ReadOnlyMemory<byte> key = ReadKey();

                if (!(dictionary.TryAdd(key, Position) || skipDuplicateKeys))
                    if (duplicateKeyPosition == -1)
                        duplicateKeyPosition = keyPosition;

                SkipValue();
            }

            ReadDictionaryTail();

            if (duplicateKeyPosition != -1)
                throw new InvalidBencodeException("The keys of the dictionary are not unique.", duplicateKeyPosition);

            return dictionary;
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
            _state = BencodeSpanReader.EnterDictionaryScope(_state, ref _scopeStack);

            try
            {
                BencodeSpanReader.ReadDictionaryHeadInternal(_memory.Span, ref _index);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
            _state = BencodeSpanReader.ExitDictionaryScope(_state, ref _scopeStack);

            try
            {
                BencodeSpanReader.ReadDictionaryTailInternal(_memory.Span, ref _index);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
            _state = BencodeSpanReader.GetStateAfterKey(_state);

            try
            {
                _ = BencodeSpanReader.ReadStringInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
                throw;
            }
        }

        /// <summary>
        /// Reads a key and writes it to a bencode writer.
        /// </summary>
        /// <param name="writer">The bencode writer that the key read from the reader is written to.
        ///     </param>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     key to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a key.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     key to be written.</exception>
        public void ReadKeyTo(ref BencodeSpanWriter writer)
        {
            writer.WriteKey(ReadKeySpan());
        }

        /// <summary>
        /// Reads a key and writes it to a bencode writer.
        /// </summary>
        /// <param name="writer">The bencode writer that the key read from the reader is written to.
        ///     </param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is
        ///     <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     key to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a key.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     key to be written.</exception>
        public void ReadKeyTo(IBencodeWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.WriteKey(ReadKeySpan());
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
            _ = BencodeSpanReader.GetStateAfterKey(_state);

            try
            {
                BencodeSpanReader.ReadStringLengthInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);

                return _stringLength;
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
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
            return _memory.Slice(ReadKeySpan());
        }

        /// <summary>
        /// Attempts to read a key into a buffer.
        /// </summary>
        /// <param name="destination">The buffer that the key will be written into.</param>
        /// <param name="bytesWritten">Returns the length of the key that was written into the
        ///     buffer or 0 if the length of the key is greater than the length of the buffer.
        ///     </param>
        /// <returns><see langword="false"/> if the length of the key is greater than the length of
        ///     the buffer; otherwise, <see langword="true"/>.</returns>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     key to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read a key.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        public bool TryReadKey(Span<byte> destination, out int bytesWritten)
        {
            int length = ReadKeyLength();
            if (length > destination.Length)
            {
                bytesWritten = 0;
                return false;
            }

            ReadOnlySpan<byte> key = ReadKeySpan();
            key.CopyTo(destination);
            bytesWritten = key.Length;
            return true;
        }

        byte[] IBencodeReader.ReadString()
        {
            return ReadStringSpan().ToArray();
        }

        byte[] IBencodeReader.ReadKey()
        {
            return ReadKeySpan().ToArray();
        }

        private ReadOnlySpan<byte> ReadStringSpan()
        {
            _state = BencodeSpanReader.GetStateAfterValue(_state);

            try
            {
                return BencodeSpanReader.ReadStringInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
                throw;
            }
        }

        private ReadOnlySpan<byte> ReadKeySpan()
        {
            _state = BencodeSpanReader.GetStateAfterKey(_state);

            try
            {
                return BencodeSpanReader.ReadStringInternal(_memory.Span, ref _index, ref _stringHeadLength, ref _stringLength);
            }
            catch
            {
                _state = BencodeSpanReader.State.Error;
                throw;
            }
        }
    }
}
