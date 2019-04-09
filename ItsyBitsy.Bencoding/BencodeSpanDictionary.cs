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
    /// <summary>
    /// Represents a collection of key/position pairs from a dictionary in an instance of
    /// <see cref="BencodeSpanReader"/>.
    /// </summary>
    public ref struct BencodeSpanDictionary
    {
        private readonly ReadOnlySpan<byte> _span;

        private KeyRange[] _keyRanges;

        private int _count;

        internal BencodeSpanDictionary(ReadOnlySpan<byte> span)
            : this()
        {
            _span = span;
        }

        /// <summary>
        /// Gets the number of elements in the dictionary.
        /// </summary>
        public int Count => _count;

        internal int Capacity => _keyRanges?.Length ?? 0;

        /// <summary>
        /// Gets an enumerator for the elements of the dictionary.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

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

            KeyRange keyRange = _keyRanges[index];
            position = keyRange.Index + keyRange.Length;
            return true;
        }

        internal bool TryAdd(int keyIndex, int keyLength)
        {
            Debug.Assert(keyIndex >= 0);
            Debug.Assert(keyIndex <= _span.Length - keyLength);

            var keyRange = new KeyRange(keyIndex, keyLength);

            if (_count == 0)
            {
                InsertLast(keyRange);
                return true;
            }

            ReadOnlySpan<byte> key = SliceKey(keyRange);

            // Fast path for ordered keys.
            ReadOnlySpan<byte> lastKey = SliceKey(_keyRanges[_count - 1]);
            int result = BencodeStringComparer.Compare(lastKey, key);
            if (result < 0)
            {
                InsertLast(keyRange);
                return true;
            }
            if (result == 0)
                return false;

            // Search can ignore last key because it has already been compared against.
            int index = BinarySearch(key, _count - 1);
            if (index >= 0)
                return false;

            Insert(~index, keyRange);
            return true;
        }

        private ReadOnlySpan<byte> SliceKey(KeyRange keyRange) => _span.Slice(keyRange.Index, keyRange.Length);

        private int BinarySearch(ReadOnlySpan<byte> key, int count)
        {
            int left = 0;
            int right = count - 1;
            while (left <= right)
            {
                int middle = left + ((right - left) >> 1);
                ReadOnlySpan<byte> middleKey = SliceKey(_keyRanges[middle]);
                int result = BencodeStringComparer.Compare(middleKey, key);
                if (result == 0)
                    return middle;
                else if (result < 0)
                    left = middle + 1;
                else
                    right = middle - 1;
            }
            return ~left;
        }

        private void InsertLast(KeyRange keyRange)
        {
            EnsureCapacity();

            _keyRanges[_count] = keyRange;
            _count += 1;
        }

        private void Insert(int index, KeyRange keyRange)
        {
            EnsureCapacity();

            Array.Copy(_keyRanges, index, _keyRanges, index + 1, _count - index);
            _keyRanges[index] = keyRange;
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

                Array.Resize(ref _keyRanges, capacity);
            }
        }

        private struct KeyRange
        {
            public readonly int Index;

            public readonly int Length;

            public KeyRange(int index, int length)
            {
                Index = index;
                Length = length;
            }
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="BencodeSpanDictionary"/>.
        /// </summary>
        public ref struct Enumerator
        {
            private readonly BencodeSpanDictionary _dictionary;

            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(BencodeSpanDictionary dictionary)
            {
                _dictionary = dictionary;
                _index = -1;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public KeyPositionPair Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    KeyRange keyRange = _dictionary._keyRanges[_index];
                    ReadOnlySpan<byte> key = _dictionary.SliceKey(keyRange);
                    int position = keyRange.Index + keyRange.Length;
                    return new KeyPositionPair(key, position);
                }
            }

            /// <summary>
            /// Advances the enumerator to the next position.
            /// </summary>
            /// <returns><see langword="true"/> if the enumerator advanced to the next element;
            ///     <see langword="false"/> if the enumerator has reached the end of the dictionary.
            ///     </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int nextIndex = _index + 1;
                if (nextIndex < _dictionary._count)
                {
                    _index = nextIndex;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Represents an element in a <see cref="BencodeSpanDictionary"/>.
        /// </summary>
        public ref struct KeyPositionPair
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyPositionPair(ReadOnlySpan<byte> key, int position)
            {
                Key = key;
                Position = position;
            }

            /// <summary>
            /// Gets the key.
            /// </summary>
            public ReadOnlySpan<byte> Key { get; }

            /// <summary>
            /// Gets the position of the value.
            /// </summary>
            public int Position { get; }
        }
    }
}
