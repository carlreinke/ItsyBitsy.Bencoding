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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ItsyBitsy.Bencoding
{
    internal static class MemoryExtensions
    {
        public static ReadOnlyMemory<byte> Slice(this ReadOnlyMemory<byte> memory, ReadOnlySpan<byte> span)
        {
            IntPtr start = Unsafe.ByteOffset(ref MemoryMarshal.GetReference(memory.Span), ref MemoryMarshal.GetReference(span));
            if (IntPtr.Size > sizeof(int))
                if ((long)start < int.MinValue || (long)start > int.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(span));
            return memory.Slice((int)start, span.Length);
        }
    }
}
