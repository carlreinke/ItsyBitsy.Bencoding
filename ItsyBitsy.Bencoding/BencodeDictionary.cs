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
using System.Collections;
using System.Collections.Generic;

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// A dictionary that is optimized for ordered insertion.
    /// </summary>
    public sealed class BencodeDictionary : IEnumerable<KeyValuePair<ReadOnlyMemory<byte>, int>>
    {
        private ReadOnlyMemory<byte>[] _keys = Array.Empty<ReadOnlyMemory<byte>>();

        private int[] _positions = Array.Empty<int>();

        private int _count;

        /// <summary>
        /// Gets the number of elements in the dictionary.
        /// </summary>
        public int Count => _count;

        internal int Capacity => _keys.Length;

        /// <summary>
        /// Gets an enumerator for the elements of the dictionary.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<KeyValuePair<ReadOnlyMemory<byte>, int>> GetEnumerator()
        {
            for (int i = 0; i < _count; ++i)
                yield return new KeyValuePair<ReadOnlyMemory<byte>, int>(_keys[i], _positions[i]);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Attempts to get the position of the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="position">Returns the position if the key exists in the dictionary;
        ///     otherwise, the default value.</param>
        /// <returns><see langword="true"/> if the key exists in the dictionary; otherwise,
        ///     <see langword="false"/>.</returns>
        public bool TryGetPosition(ReadOnlySpan<byte> key, out int position)
        {
            int index = BinarySearch(key, _count);
            if (index < 0)
            {
                position = default;
                return false;
            }

            position = _positions[index];
            return true;
        }

        internal bool TryAdd(ReadOnlyMemory<byte> key, int position)
        {
            if (_count == 0)
            {
                InsertLast(key, position);
                return true;
            }

            // Fast path for ordered keys.
            ReadOnlyMemory<byte> lastKey = _keys[_count - 1];
            int result = BencodeStringComparer.Compare(lastKey.Span, key.Span);
            if (result < 0)
            {
                InsertLast(key, position);
                return true;
            }
            if (result == 0)
                return false;

            // Search can ignore last key because it has already been compared against.
            int index = BinarySearch(key.Span, _count - 1);
            if (index >= 0)
                return false;

            Insert(~index, key, position);
            return true;
        }

        private int BinarySearch(ReadOnlySpan<byte> key, int count)
        {
            int left = 0;
            int right = count - 1;
            while (left <= right)
            {
                int middle = left + ((right - left) >> 1);
                ReadOnlyMemory<byte> middleKey = _keys[middle];
                int result = BencodeStringComparer.Compare(middleKey.Span, key);
                if (result == 0)
                    return middle;
                else if (result < 0)
                    left = middle + 1;
                else
                    right = middle - 1;
            }
            return ~left;
        }

        private void InsertLast(ReadOnlyMemory<byte> key, int position)
        {
            EnsureCapacity();

            _keys[_count] = key;
            _positions[_count] = position;
            _count += 1;
        }

        private void Insert(int index, ReadOnlyMemory<byte> key, int position)
        {
            EnsureCapacity();

            Array.Copy(_keys, index, _keys, index + 1, _count - index);
            _keys[index] = key;
            Array.Copy(_positions, index, _positions, index + 1, _count - index);
            _positions[index] = position;
            _count += 1;
        }

        private void EnsureCapacity()
        {
            int capacity = Capacity;
            if (_count == capacity)
            {
                capacity = capacity == 0 ? 4 : capacity * 2;
                if (capacity < _count + 1)
                    capacity = _count + 1;

                Array.Resize(ref _keys, capacity);
                Array.Resize(ref _positions, capacity);
            }
        }
    }
}
