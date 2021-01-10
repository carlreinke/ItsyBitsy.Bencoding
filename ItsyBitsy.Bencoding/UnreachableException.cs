//
// Copyright (C) 2021  Carl Reinke
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
    [Serializable]
    internal sealed class UnreachableException : Exception
    {
        private const string _defaultMessage = "The program executed an instruction that was thought to be unreachable.";

        public UnreachableException()
            : base(_defaultMessage)
        {
        }

        public UnreachableException(string? message)
            : base(message ?? _defaultMessage)
        {
        }

        public UnreachableException(string? message, Exception? inner)
            : base(message ?? _defaultMessage, inner)
        {
        }

        private UnreachableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
