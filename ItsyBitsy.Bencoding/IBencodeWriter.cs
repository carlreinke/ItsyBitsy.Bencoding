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
    /// Provides a writer for bencoded data.
    /// </summary>
    public interface IBencodeWriter
    {
        /// <summary>
        /// Writes an integer.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows an
        ///     integer to be written.</exception>
        void WriteInteger(long value);

        /// <summary>
        /// Writes a string.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     string to be written.</exception>
        void WriteString(ReadOnlySpan<byte> value);

        /// <summary>
        /// Writes the beginning of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     list head to be written.</exception>
        void WriteListHead();

        /// <summary>
        /// Writes the end of a list.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     list tail to be written.</exception>
        void WriteListTail();

        /// <summary>
        /// Writes the beginning of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     dictionary head to be written.</exception>
        void WriteDictionaryHead();

        /// <summary>
        /// Writes the end of a dictionary.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     dictionary tail to be written.</exception>
        void WriteDictionaryTail();

        /// <summary>
        /// Writes a key.
        /// </summary>
        /// <param name="key">The key to write.</param>
        /// <exception cref="InvalidOperationException">The writer is not in a state that allows a
        ///     key to be written.</exception>
        /// <exception cref="InvalidOperationException">The writer is validating keys and the key
        ///     being written is mis-ordered or duplicated.</exception>
        void WriteKey(ReadOnlySpan<byte> key);
    }
}
