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
    /// A collection of keys and positions for a <see cref="BencodeSpanReader"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="BencodeSpanDictionary"/> does not copy the keys from the
    /// <see cref="BencodeSpanReader"/>, so it may have unexpected behavior if the underlying memory
    /// is modified.
    /// </remarks>
    public readonly ref struct BencodeSpanDictionary
    {
        private readonly ReadOnlySpan<byte> _span;

        private readonly KeyRange[] _keyRanges;

        private readonly int _count;

        private BencodeSpanDictionary(ReadOnlySpan<byte> span, KeyRange[] keyRanges, int count)
        {
            _span = span;
            _keyRanges = keyRanges;
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
            int index = BinarySearch(_span, _keyRanges, _count, key);
            if (index < 0)
            {
                position = default;
                return false;
            }

            KeyRange keyRange = _keyRanges[index];
            position = keyRange.Index + keyRange.Length;
            return true;
        }

        private static ReadOnlySpan<byte> SliceKey(ReadOnlySpan<byte> span, KeyRange keyRange) => span.Slice(keyRange.Index, keyRange.Length);

        private static int BinarySearch(ReadOnlySpan<byte> span, KeyRange[] keyRanges, int count, ReadOnlySpan<byte> key)
        {
            int left = 0;
            int right = count - 1;
            while (left <= right)
            {
                int middle = left + ((right - left) >> 1);
                ReadOnlySpan<byte> middleKey = SliceKey(span, keyRanges[middle]);
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
                    ReadOnlySpan<byte> key = SliceKey(_dictionary._span, keyRange);
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

        internal ref struct Builder
        {
            private readonly ReadOnlySpan<byte> _span;

            private KeyRange[] _keyRanges;

            private int _count;

            internal Builder(ReadOnlySpan<byte> span)
            {
                _span = span;
                _keyRanges = null;
                _count = 0;
            }

            internal int Capacity => _keyRanges?.Length ?? 0;

            internal BencodeSpanDictionary ToDictionary() => new BencodeSpanDictionary(_span, _keyRanges, _count);

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

                ReadOnlySpan<byte> key = SliceKey(_span, keyRange);

                // Fast path for ordered keys.
                ReadOnlySpan<byte> lastKey = SliceKey(_span, _keyRanges[_count - 1]);
                int result = BencodeStringComparer.Compare(lastKey, key);
                if (result < 0)
                {
                    InsertLast(keyRange);
                    return true;
                }
                if (result == 0)
                    return false;

                // Search can ignore last key because it has already been compared against.
                int index = BinarySearch(_span, _keyRanges, _count - 1, key);
                if (index >= 0)
                    return false;

                Insert(~index, keyRange);
                return true;
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
        }
    }
}
