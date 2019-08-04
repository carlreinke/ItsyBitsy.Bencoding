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
using System.Runtime.Serialization;

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// Represents a bencode error.
    /// </summary>
    [Serializable]
    public abstract class BencodeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BencodeException"/> class.
        /// </summary>
        protected BencodeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BencodeException"/> class with the
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        protected BencodeException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BencodeException"/> class with the
        /// specified error message and inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception that is the cause of this exception.</param>
        protected BencodeException(string? message, Exception? inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BencodeException"/> class with serialized
        /// data.
        /// </summary>
        /// <param name="info">The serialized object data about the exception being thrown.</param>
        /// <param name="context">The contextual information about the source or destination.
        ///     </param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="SerializationException"/>
        protected BencodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
