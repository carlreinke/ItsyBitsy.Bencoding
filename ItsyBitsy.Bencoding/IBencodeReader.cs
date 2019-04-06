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

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// Provides a reader for bencoded data.
    /// </summary>
    public interface IBencodeReader
    {
        /// <summary>
        /// Gets a indicator of whether the reader position can be set.
        /// </summary>
        bool CanSeek { get; }

        /// <summary>
        /// Gets or sets the position of the reader in the source memory.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.
        ///     </exception>
        /// <exception cref="InvalidOperationException"><see cref="Position"/> is being set and the
        ///     reader does not support seeking.</exception>
        /// <remarks>
        /// <para>
        /// Setting the position puts the reader into a state in which it can read a value.  For
        /// example, if the position of the reader is set to the position of an element in a list,
        /// the reader will be able to read only that element and not any subsequent elements.  If
        /// the position of the reader is set to position of a key, the reader will be able to read
        /// the key as a value using <see cref="ReadString"/> rather than <see cref="ReadKey"/>.
        /// </para>
        /// <para>
        /// Consult <see cref="CanSeek"/> to determine whether <see cref="Position"/> can be set.
        /// </para>
        /// </remarks>
        long Position { get; set; }

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
        BencodeTokenType ReadTokenType();

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
        void SkipValue();

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
        void ReadValueTo(ref BencodeSpanWriter writer);

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
        void ReadValueTo(IBencodeWriter writer);

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
        long ReadInteger();

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
        int ReadStringLength();

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
        byte[] ReadString();

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
        bool TryReadString(Span<byte> destination, out int bytesWritten);

        /// <summary>
        /// Reads the beginning of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     list head to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read the beginning of a list.</exception>
        void ReadListHead();

        /// <summary>
        /// Reads the end of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     list tail to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read the end of a list.</exception>
        void ReadListTail();

        /// <summary>
        /// Reads the beginning of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     dictionary head to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read the beginning of a dictionary.</exception>
        void ReadDictionaryHead();

        /// <summary>
        /// Reads the end of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     dictionary tail to be read.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to read the end of a dictionary.</exception>
        void ReadDictionaryTail();

        /// <summary>
        /// Skips a key.
        /// </summary>
        /// <exception cref="InvalidOperationException">The reader is not in a state that allows a
        ///     key to be skipped.</exception>
        /// <exception cref="InvalidBencodeException">The reader encountered invalid data while
        ///     attempting to skip a key.</exception>
        /// <exception cref="UnsupportedBencodeException">The reader encountered a key with a length
        ///     that is not in the supported range.</exception>
        void SkipKey();

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
        void ReadKeyTo(ref BencodeSpanWriter writer);

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
        void ReadKeyTo(IBencodeWriter writer);

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
        int ReadKeyLength();

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
        byte[] ReadKey();

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
        bool TryReadKey(Span<byte> destination, out int bytesWritten);
    }
}
