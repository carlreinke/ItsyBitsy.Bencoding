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
using ItsyBitsy.Xunit;
using System.Linq;
using Xunit;

namespace ItsyBitsy.Bencoding.Tests
{
    public static class BencodeDictionaryTests
    {
        [Fact]
        public static void Count_DefaultInstance_ReturnsZero()
        {
            BencodeDictionary dictionary = default;

            int count = dictionary.Count;

            Assert.Equal(0, count);
        }

        [Fact]
        public static void GetEnumerator_DefaultInstance_ReturnsEmptyEnumerator()
        {
            BencodeDictionary dictionary = default;

            int i = 0;
            foreach (var _ in dictionary)
                i += 1;
            Assert.Equal(0, i);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeDictionaryTestData.DataAndElements), MemberType = typeof(BencodeDictionaryTestData))]
        public static void Count_Always_ReturnsExpectedValue(string bencodeString, (int keyPosition, string key, int position)[] elements)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var builder = new BencodeDictionary.Builder();
            foreach (var (_, key, position) in elements)
                Assert.True(builder.TryAdd(key.ToUtf8(), position));
            var dictionary = builder.ToDictionary();

            int count = dictionary.Count;

            Assert.Equal(elements.Length, count);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeDictionaryTestData.DataAndElements), MemberType = typeof(BencodeDictionaryTestData))]
        public static void GetEnumerator_Always_ReturnsExpectedPositions(string bencodeString, (int keyPosition, string key, int position)[] elements)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var builder = new BencodeDictionary.Builder();
            foreach (var (_, key, position) in elements)
                Assert.True(builder.TryAdd(key.ToUtf8(), position));
            var dictionary = builder.ToDictionary();

            var sortedElements = elements
                .OrderBy(x => x.key.ToUtf8(), BencodeStringComparer.Instance)
                .ToArray();
            int i = 0;
            foreach (var element in dictionary)
            {
                var (_, expectedKey, expectedPosition) = sortedElements[i];
                Assert.Equal(expectedKey.ToUtf8(), element.Key.ToArray());
                Assert.Equal(expectedPosition, element.Position);
                i += 1;
            }
            Assert.Equal(elements.Length, i);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeDictionaryTestData.DataAndElements), MemberType = typeof(BencodeDictionaryTestData))]
        public static void TryGetPosition_ExistingKey_ReturnsTrueAndExpectedPosition(string bencodeString, (int keyPosition, string key, int position)[] elements)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var builder = new BencodeDictionary.Builder();
            foreach (var (_, key, position) in elements)
                Assert.True(builder.TryAdd(key.ToUtf8(), position));
            var dictionary = builder.ToDictionary();

            foreach (var (_, key, expectedPosition) in elements)
            {
                int position;
                Assert.True(dictionary.TryGetPosition(key.ToUtf8(), out position));
                Assert.Equal(expectedPosition, position);
            }
        }

        [Theory]
        [TupleMemberData(nameof(BencodeDictionaryTestData.DataAndElementsAndNonKey), MemberType = typeof(BencodeDictionaryTestData))]
        public static void TryGetPosition_NonexistingKey_ReturnsFalseAndDefaultPosition(string bencodeString, (int keyPosition, string key, int position)[] elements, string nonKey)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var builder = new BencodeDictionary.Builder();
            foreach (var (_, key, position) in elements)
                Assert.True(builder.TryAdd(key.ToUtf8(), position));
            var dictionary = builder.ToDictionary();

            foreach (var (_, key, _) in elements)
                Assert.True(dictionary.TryGetPosition(key.ToUtf8(), out _));

            int resultPosition;
            Assert.False(dictionary.TryGetPosition(nonKey.ToUtf8(), out resultPosition));
            Assert.Equal(default, resultPosition);
        }

        public static class Builder
        {
            [Fact]
            public static void Capacity_DefaultInstance_ReturnsZero()
            {
                BencodeDictionary.Builder builder = default;

                int count = builder.Capacity;

                Assert.Equal(0, count);
            }

            [Theory]
            [TupleMemberData(nameof(BencodeDictionaryTestData.DataAndElements), MemberType = typeof(BencodeDictionaryTestData))]
            public static void TryAdd_NonduplicateKey_ReturnsTrue(string bencodeString, (int keyPosition, string key, int position)[] elements)
            {
                byte[] bencode = bencodeString.ToUtf8();
                var builder = new BencodeDictionary.Builder();
                foreach (var (_, key, position) in elements)
                    Assert.True(builder.TryAdd(key.ToUtf8(), position));
            }

            [Theory]
            [TupleMemberData(nameof(BencodeDictionaryTestData.DataAndElements), MemberType = typeof(BencodeDictionaryTestData))]
            public static void TryAdd_DuplicateKey_ReturnsFalse(string bencodeString, (int keyPosition, string key, int position)[] elements)
            {
                byte[] bencode = bencodeString.ToUtf8();
                var builder = new BencodeDictionary.Builder();
                foreach (var (_, key, position) in elements)
                    Assert.True(builder.TryAdd(key.ToUtf8(), position));

                foreach (var (_, key, position) in elements)
                    Assert.False(builder.TryAdd(key.ToUtf8(), position));
            }
        }
    }
}
