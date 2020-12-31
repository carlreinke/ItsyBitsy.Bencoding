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
using System.Collections.Generic;

namespace ItsyBitsy.Bencoding
{
    /// <summary>
    /// Provides comparisons of bencode strings.
    /// </summary>
    public sealed class BencodeStringComparer : IComparer<byte[]>, IComparer<ReadOnlyMemory<byte>>, IEqualityComparer<byte[]>, IEqualityComparer<ReadOnlyMemory<byte>>
    {
        private const int _maxBytesToHash = 32;

        /// <summary>
        /// Gets a shared instance of the comparer.
        /// </summary>
        public static readonly BencodeStringComparer Instance = new BencodeStringComparer();

        /// <summary>
        /// Compares two instances and returns a value indicating their relative values.
        /// </summary>
        /// <param name="x">The first instance to compare.</param>
        /// <param name="y">The second instance to compare.</param>
        /// <returns>A number indicating the relative values of <paramref name="x"/> and
        ///     <paramref name="y"/>.  A negative number indicates that <paramref name="x"/> is less
        ///     than <paramref name="y"/>.  A positive number indicates that <paramref name="x"/> is
        ///     greater than <paramref name="y"/>.  Zero indicates that <paramref name="x"/> is
        ///     equal to <paramref name="y"/>.</returns>
        public static int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
        {
            int minLength = x.Length;
            if (minLength > y.Length)
                minLength = y.Length;

            for (int i = 0; i < minLength; ++i)
            {
                int result = x[i].CompareTo(y[i]);
                if (result != 0)
                    return result;
            }

            return x.Length.CompareTo(y.Length);
        }

        /// <summary>
        /// Returns a value indicating whether two instance are equal.
        /// </summary>
        /// <param name="x">The first instance to compare.</param>
        /// <param name="y">The second instance to compare.</param>
        /// <returns><see langword="true"/> if the two instances are equal; otherwise,
        ///     <see langword="false"/>.</returns>
        public static bool Equals(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
        {
            return x.SequenceEqual(y);
        }

        /// <summary>
        /// Returns the hash code for an instance.
        /// </summary>
        /// <param name="span">The instance to generate a hash code for.</param>
        /// <returns>The hash code.</returns>
        public static int GetHashCode(ReadOnlySpan<byte> span)
        {
            int bytesToHash = Math.Min(span.Length, _maxBytesToHash);

            long hashCode = span.Length.GetHashCode();

            for (int i = 0; i < bytesToHash; i++)
                hashCode = unchecked(37 * hashCode + span[i]);

            return (int)hashCode;
        }

        /// <summary>
        /// Compares two instances and returns a value indicating their relative values.
        /// </summary>
        /// <param name="x">The first instance to compare.</param>
        /// <param name="y">The second instance to compare.</param>
        /// <returns>A number indicating the relative values of <paramref name="x"/> and
        ///     <paramref name="y"/>.  A negative number indicates that <paramref name="x"/> is less
        ///     than <paramref name="y"/>.  A positive number indicates that <paramref name="x"/> is
        ///     greater than <paramref name="y"/>.  Zero indicates that <paramref name="x"/> is
        ///     equal to <paramref name="y"/>.</returns>
        public int Compare(byte[]? x, byte[]? y) => Compare(x.AsSpan(), y.AsSpan());

        /// <summary>
        /// Compares two instances and returns a value indicating their relative values.
        /// </summary>
        /// <param name="x">The first instance to compare.</param>
        /// <param name="y">The second instance to compare.</param>
        /// <returns>A number indicating the relative values of <paramref name="x"/> and
        ///     <paramref name="y"/>.  A negative number indicates that <paramref name="x"/> is less
        ///     than <paramref name="y"/>.  A positive number indicates that <paramref name="x"/> is
        ///     greater than <paramref name="y"/>.  Zero indicates that <paramref name="x"/> is
        ///     equal to <paramref name="y"/>.</returns>
        public int Compare(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => Compare(x.Span, y.Span);

        /// <summary>
        /// Returns a value indicating whether two instance are equal.
        /// </summary>
        /// <param name="x">The first instance to compare.</param>
        /// <param name="y">The second instance to compare.</param>
        /// <returns><see langword="true"/> if the two instances are equal; otherwise,
        ///     <see langword="false"/>.</returns>
        public bool Equals(byte[]? x, byte[]? y) => Equals(x.AsSpan(), y.AsSpan());

        /// <summary>
        /// Returns a value indicating whether two instance are equal.
        /// </summary>
        /// <param name="x">The first instance to compare.</param>
        /// <param name="y">The second instance to compare.</param>
        /// <returns><see langword="true"/> if the two instances are equal; otherwise,
        ///     <see langword="false"/>.</returns>
        public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => Equals(x.Span, y.Span);

        /// <summary>
        /// Returns the hash code for an instance.
        /// </summary>
        /// <param name="obj">The instance to generate a hash code for.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(byte[]? obj) => GetHashCode(obj.AsSpan());

        /// <summary>
        /// Returns the hash code for an instance.
        /// </summary>
        /// <param name="obj">The instance to generate a hash code for.</param>
        /// <returns>The hash code.</returns>
        public int GetHashCode(ReadOnlyMemory<byte> obj) => GetHashCode(obj.Span);
    }
}
