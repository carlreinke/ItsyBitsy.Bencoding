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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ItsyBitsy.Bencoding
{
    [DebuggerDisplay("Count = {Count}")]
    internal struct BitStack
    {
        private int _count;

        private ulong _bits;

        private ulong[] _bitsArray;

        public readonly int Count => _count;

        public void PushFalse()
        {
            _bits = _bits << 1;
            Push();
        }

        public void PushTrue()
        {
            _bits = (_bits << 1) | 1;
            Push();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Push()
        {
            _count += 1;
            if ((_count & 0x3F) == 0)
            {
                int index = (_count >> 6) - 1;
                if (_bitsArray == null)
                    _bitsArray = new ulong[1];
                else if (index == _bitsArray.Length)
                    Array.Resize(ref _bitsArray, _bitsArray.Length * 2);
                _bitsArray[index] = _bits;
            }
        }

        public bool Pop()
        {
            Debug.Assert(_count != 0);

            if ((_count & 0x3F) == 0)
            {
                int index = (_count >> 6) - 1;
                _bits = _bitsArray[index];
            }
            bool result = (_bits & 1uL) != 0;
            _bits >>= 1;
            _count -= 1;
            return result;
        }

        public void Clear()
        {
            _count = 0;
        }
    }
}
