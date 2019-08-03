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
    /// Represents an error caused by parsing invalid bencode.
    /// </summary>
    [Serializable]
    public class InvalidBencodeException : BencodeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBencodeException"/> class.
        /// </summary>
        public InvalidBencodeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBencodeException"/> class with the
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public InvalidBencodeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBencodeException"/> class with the
        /// specified error message and inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner exception that is the cause of this exception.</param>
        public InvalidBencodeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBencodeException"/> class with the
        /// specified error message and position in the stream where the error occurred.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="position">The position in the stream where the error occurred.</param>
        public InvalidBencodeException(string message, long position)
            : base(message)
        {
            Position = position;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidBencodeException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The serialized object data about the exception being thrown.</param>
        /// <param name="context">The contextual information about the source or destination.
        ///     </param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="SerializationException"/>
        protected InvalidBencodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Position = info.GetInt64(nameof(Position));
        }

        /// <summary>
        /// The position in the stream where the error occurred.
        /// </summary>
        [field: NonSerialized]
        public long Position { get; private set; } = -1;

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object
        ///     data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual
        ///     information about the source or destination.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is
        ///     <see langword="null"/>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(Position), Position);
        }
    }
}
