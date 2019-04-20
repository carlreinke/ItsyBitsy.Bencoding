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
using Xunit;

namespace ItsyBitsy.Bencoding.Tests
{
    public class BencodeListTests
    {
        [Fact]
        public static void Count_DefaultInstance_ReturnsZero()
        {
            BencodeList list = default;

            int count = list.Count;

            Assert.Equal(0, count);
        }

        [Fact]
        public static void GetEnumerator_DefaultInstance_ReturnsEmptyEnumerator()
        {
            BencodeList list = default;

            int i = 0;
            foreach (var _ in list)
                i += 1;
            Assert.Equal(0, i);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeListTestData.DataAndPositions), MemberType = typeof(BencodeListTestData))]
        public static void Count_Always_ReturnsExpectedValue(string bencodeString, int[] positions)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var builder = new BencodeList.Builder();
            foreach (var position in positions)
                builder.Add(position);
            var list = builder.ToList();

            int count = list.Count;

            Assert.Equal(positions.Length, count);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeListTestData.DataAndPositions), MemberType = typeof(BencodeListTestData))]
        public static void GetEnumerator_Always_ReturnsExpectedPositions(string bencodeString, int[] positions)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var builder = new BencodeList.Builder();
            foreach (int position in positions)
                builder.Add(position);
            var list = builder.ToList();

            int i = 0;
            foreach (int position in list)
            {
                int expectedPosition = positions[i];
                Assert.Equal(expectedPosition, position);
                i += 1;
            }
            Assert.Equal(positions.Length, i);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeListTestData.DataAndPositions), MemberType = typeof(BencodeListTestData))]
        public static void Indexer_InRange_ReturnsExpectedPosition(string bencodeString, int[] positions)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var builder = new BencodeList.Builder();
            foreach (int position in positions)
                builder.Add(position);
            var list = builder.ToList();

            for (int i = 0; i < positions.Length; ++i)
            {
                int expectedPosition = positions[i];
                int position = list[i];
                Assert.Equal(expectedPosition, position);
            }
        }

        [Theory]
        [TupleMemberData(nameof(BencodeListTestData.DataAndPositions), MemberType = typeof(BencodeListTestData))]
        public static void Indexer_OutOfRange_ThrowsArgumentOutOfRangeException(string bencodeString, int[] positions)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var builder = new BencodeList.Builder();
            foreach (int position in positions)
                builder.Add(position);
            var list = builder.ToList();

            for (int i = 0; i < positions.Length; ++i)
                _ = list[i];

            var ex1 = Assert.Throws<ArgumentOutOfRangeException>(() => list[-1]);
            var ex2 = Assert.Throws<ArgumentOutOfRangeException>(() => list[positions.Length]);

            Assert.Equal("index", ex1.ParamName);
            Assert.Equal("index", ex2.ParamName);
        }

        public static class Builder
        {
            [Fact]
            public static void Capacity_DefaultInstance_ReturnsZero()
            {
                BencodeList.Builder builder = default;

                int count = builder.Capacity;

                Assert.Equal(0, count);
            }
        }
    }
}
