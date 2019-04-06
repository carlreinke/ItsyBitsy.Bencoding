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
namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// Indicates the type of token that must be read next.
    /// </summary>
    public enum BencodeTokenType
    {
        /// <summary>
        /// Indicates that no more tokens can be read.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that an integer must be read next.
        /// </summary>
        Integer,

        /// <summary>
        /// Indicates that a string must be read next.
        /// </summary>
        String,

        /// <summary>
        /// Indicates that the beginning of a list must be read next.
        /// </summary>
        ListHead,

        /// <summary>
        /// Indicates that the end of a list must be read next.
        /// </summary>
        ListTail,

        /// <summary>
        /// Indicates that the beginning of a dictionary must be read next.
        /// </summary>
        DictionaryHead,

        /// <summary>
        /// Indicates that the end of a list or dictionary must be read next.
        /// </summary>
        DictionaryTail,

        /// <summary>
        /// Indicates that a key must be read next.
        /// </summary>
        Key,
    }
}
