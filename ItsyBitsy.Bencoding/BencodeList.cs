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
    /// A list of positions for a <see cref="BencodeReader"/> or <see cref="BencodeSpanReader"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public readonly struct BencodeList
    {
        private readonly int[] _entries;

        private readonly int _count;

        private BencodeList(int[] entries, int count)
        {
            _entries = entries;
            _count = count;
        }

        /// <summary>
        /// Gets the number of elements in the list.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets the position at the specified index.
        /// </summary>
        /// <param name="index">The index of the position.</param>
        /// <returns>The position at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or
        ///     greater than or equal to <see cref="Count"/>.</exception>
        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _entries[index];
            }
        }

        /// <summary>
        /// Gets an enumerator for the elements of the list.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="BencodeList"/>.
        /// </summary>
        public struct Enumerator
        {
            private readonly BencodeList _list;

            private int _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(BencodeList list)
            {
                _list = list;
                _index = -1;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public int Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return _list._entries[_index];
                }
            }

            /// <summary>
            /// Advances the enumerator to the next position.
            /// </summary>
            /// <returns><see langword="true"/> if the enumerator advanced to the next element;
            ///     <see langword="false"/> if the enumerator has reached the end of the list.
            ///     </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int nextIndex = _index + 1;
                if (nextIndex < _list._count)
                {
                    _index = nextIndex;
                    return true;
                }

                return false;
            }
        }

        internal struct Builder
        {
            private int[] _entries;

            private int _count;

            internal int Capacity => _entries?.Length ?? 0;

            internal void Add(int position)
            {
                EnsureCapacity();

                _entries[_count] = position;
                _count += 1;
            }

            internal BencodeList ToList() => new BencodeList(_entries, _count);

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
