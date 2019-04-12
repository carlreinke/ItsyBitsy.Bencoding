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
    /// Provides extensions for <see cref="IBencodeReader"/>.
    /// </summary>
    public static class IBencodeReaderExtensions
    {
        /// <summary>
        /// Reads a dictionary and returns a <see cref="BencodeDictionary{TPosition}"/>.
        /// </summary>
        /// <param name="reader">The reader to read from.</param>
        /// <param name="skipDuplicateKeys">If <see langword="true"/>, duplicate keys are skipped
        ///     rather than causing <see cref="InvalidBencodeException"/> to be thrown.</param>
        /// <returns>The dictionary.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is
        ///     <see langword="null"/>.</exception>
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
        public static BencodeDictionary<long> ReadDictionary(this IBencodeReader reader, bool skipDuplicateKeys = false)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            reader.ReadDictionaryHead();

            var dictionary = new BencodeDictionary<long>();

            long duplicateKeyPosition = -1;

            while (reader.ReadTokenType() != BencodeTokenType.DictionaryTail)
            {
                long keyPosition = reader.Position;

                ReadOnlyMemory<byte> key = reader.ReadKey();

                if (!(dictionary.TryAdd(key, reader.Position) || skipDuplicateKeys))
                    if (duplicateKeyPosition == -1)
                        duplicateKeyPosition = keyPosition;

                reader.SkipValue();
            }

            reader.ReadDictionaryTail();

            if (duplicateKeyPosition != -1)
                throw new InvalidBencodeException("The keys of the dictionary are not unique.", duplicateKeyPosition);

            return dictionary;
        }

        /// <summary>
        /// Reads a list and returns the positions of the elements.
        /// </summary>
        /// <returns>The list of positions of the list element values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="reader"/> is
        ///     <see langword="null"/>.</exception>
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
        public static List<long> ReadList(this IBencodeReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            reader.ReadListHead();

            var list = new List<long>();

            while (reader.ReadTokenType() != BencodeTokenType.ListTail)
            {
                list.Add(reader.Position);

                reader.SkipValue();
            }

            reader.ReadListTail();

            return list;
        }
    }
}
