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

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// A collection of keys and positions for a <see cref="BencodeReader"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="BencodeDictionary"/> does not copy the keys from the <see cref="BencodeReader"/>,
    /// so it may have unexpected behavior if the underlying memory is modified.
    /// </remarks>
    public readonly struct BencodeDictionary
    {
        private readonly KeyPositionPair[] _entries;

        private readonly int _count;

        private BencodeDictionary(KeyPositionPair[] entries, int count)
        {
            _entries = entries;
            _count = count;
        }

        /// <summary>
        /// Gets the number of elements in the dictionary.
        /// </summary>
        public int Count => _count;

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
            int index = BinarySearch(_entries, _count, key);
            if (index < 0)
            {
                position = default;
                return false;
            }

            position = _entries[index].Position;
            return true;
        }

        private static int BinarySearch(KeyPositionPair[] entries, int count, ReadOnlySpan<byte> key)
        {
            int left = 0;
            int right = count - 1;
            while (left <= right)
            {
                int middle = left + ((right - left) >> 1);
                ReadOnlyMemory<byte> middleKey = entries[middle].Key;
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

        /// <summary>
        /// Enumerates the elements of a <see cref="BencodeDictionary"/>.
        /// </summary>
        public struct Enumerator
        {
            private readonly BencodeDictionary _dictionary;

            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(BencodeDictionary dictionary)
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
                    return _dictionary._entries[_index];
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
        /// Represents an element in a <see cref="BencodeDictionary"/>.
        /// </summary>
        public readonly struct KeyPositionPair
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal KeyPositionPair(ReadOnlyMemory<byte> key, int position)
            {
                Key = key;
                Position = position;
            }

            /// <summary>
            /// Gets the key.
            /// </summary>
            public ReadOnlyMemory<byte> Key { get; }

            /// <summary>
            /// Gets the position of the value.
            /// </summary>
            public int Position { get; }
        }

        internal struct Builder
        {
            private KeyPositionPair[] _entries;

            private int _count;

            internal int Capacity => _entries?.Length ?? 0;

            internal BencodeDictionary ToDictionary() => new BencodeDictionary(_entries, _count);

            internal bool TryAdd(ReadOnlyMemory<byte> key, int position)
            {
                if (_count == 0)
                {
                    InsertLast(key, position);
                    return true;
                }

                // Fast path for ordered keys.
                ReadOnlyMemory<byte> lastKey = _entries[_count - 1].Key;
                int result = BencodeStringComparer.Compare(lastKey.Span, key.Span);
                if (result < 0)
                {
                    InsertLast(key, position);
                    return true;
                }
                if (result == 0)
                    return false;

                // Search can ignore last key because it has already been compared against.
                int index = BinarySearch(_entries, _count - 1, key.Span);
                if (index >= 0)
                    return false;

                Insert(~index, key, position);
                return true;
            }

            private void InsertLast(ReadOnlyMemory<byte> key, int position)
            {
                EnsureCapacity();

                _entries[_count] = new KeyPositionPair(key, position);
                _count += 1;
            }

            private void Insert(int index, ReadOnlyMemory<byte> key, int position)
            {
                EnsureCapacity();

                Array.Copy(_entries, index, _entries, index + 1, _count - index);
                _entries[index] = new KeyPositionPair(key, position);
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

                    Array.Resize(ref _entries, capacity);
                }
            }
        }
    }
}
