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
using System.Buffers;

namespace ItsyBitsy.Bencoding.Tests
{
    internal sealed class FixedLengthBufferWriter : IBufferWriter<byte>
    {
        private readonly byte[] _array;

        private int _index;

        public int WrittenLength => _index;

        public Span<byte> WrittenSpan => _array.AsSpan(0, _index);

        public FixedLengthBufferWriter(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            _array = new byte[length];
        }

        public void Advance(int count)
        {
            if (count < 0 || count > _array.Length - _index)
                throw new ArgumentOutOfRangeException(nameof(count));

            _index += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (sizeHint < 0)
                throw new ArgumentOutOfRangeException(nameof(sizeHint));
            if (sizeHint < 1)
                sizeHint = 1;

            int length = Math.Min(_array.Length - _index, sizeHint);
            return _array.AsMemory(_index, length);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (sizeHint < 0)
                throw new ArgumentOutOfRangeException(nameof(sizeHint));
            if (sizeHint < 1)
                sizeHint = 1;

            int length = Math.Min(_array.Length - _index, sizeHint);
            return _array.AsSpan(_index, length);
        }
    }
}
