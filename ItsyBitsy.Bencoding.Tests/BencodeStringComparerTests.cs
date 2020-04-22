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
using System;
using System.Linq;
using Xunit;

namespace ItsyBitsy.Bencoding.Tests
{
    public class BencodeStringComparerTests
    {
        public static readonly (string x, string y, int result)[] Compare_Data = new[]
        {
            ("", "", 0),
            ("", "a", -1),
            ("a", "", 1),
            ("a", "a", 0),
            ("a", "aa", -1),
            ("aa", "a", 1),
            ("aa", "aa", 0),
            ("a", "b", -1),
            ("b", "a", 1),
            ("b", "b", 0),
        };

        [Theory]
        [TupleMemberData(nameof(Compare_Data))]
        public static void CompareSpan_Always_ReturnsExpectedResult(string x, string y, int expectedResult)
        {
            Span<byte> xSpan = x.ToUtf8().AsSpan();
            Span<byte> ySpan = y.ToUtf8().AsSpan();

            int result = BencodeStringComparer.Compare(xSpan, ySpan);
            if (result < 0)
                result = -1;
            else if (result > 0)
                result = 1;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [TupleMemberData(nameof(Compare_Data))]
        public static void CompareArray_Always_ReturnsExpectedResult(string x, string y, int expectedResult)
        {
            byte[] xArray = x.ToUtf8();
            byte[] yArray = y.ToUtf8();

            int result = BencodeStringComparer.Instance.Compare(xArray, yArray);
            if (result < 0)
                result = -1;
            else if (result > 0)
                result = 1;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [TupleMemberData(nameof(Compare_Data))]
        public static void CompareMemory_Always_ReturnsExpectedResult(string x, string y, int expectedResult)
        {
            Memory<byte> xMemory = x.ToUtf8().AsMemory();
            Memory<byte> yMemory = y.ToUtf8().AsMemory();

            int result = BencodeStringComparer.Instance.Compare(xMemory, yMemory);
            if (result < 0)
                result = -1;
            else if (result > 0)
                result = 1;

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [TupleMemberData(nameof(Compare_Data))]
        public static void EqualsSpan_Always_ReturnsExpectedResult(string x, string y, int expectedCompareResult)
        {
            Span<byte> xSpan = x.ToUtf8().AsSpan();
            Span<byte> ySpan = y.ToUtf8().AsSpan();

            bool result = BencodeStringComparer.Equals(xSpan, ySpan);

            Assert.Equal(expectedCompareResult == 0, result);
        }

        [Theory]
        [TupleMemberData(nameof(Compare_Data))]
        public static void EqualsArray_Always_ReturnsExpectedResult(string x, string y, int expectedCompareResult)
        {
            byte[] xArray = x.ToUtf8();
            byte[] yArray = y.ToUtf8();

            bool result = BencodeStringComparer.Instance.Equals(xArray, yArray);

            Assert.Equal(expectedCompareResult == 0, result);
        }

        [Theory]
        [TupleMemberData(nameof(Compare_Data))]
        public static void EqualsMemory_Always_ReturnsExpectedResult(string x, string y, int expectedCompareResult)
        {
            Memory<byte> xMemory = x.ToUtf8().AsMemory();
            Memory<byte> yMemory = y.ToUtf8().AsMemory();

            bool result = BencodeStringComparer.Instance.Equals(xMemory, yMemory);

            Assert.Equal(expectedCompareResult == 0, result);
        }

        [Fact]
        public static void GetHashCodeSpan_DifferentLengths_ReturnsDistinctResults()
        {
            var random = new Random();
            byte[] data = new byte[100];
            random.NextBytes(data);

            int[] hashCodes = Enumerable.Range(0, data.Length)
                .Select(i => BencodeStringComparer.GetHashCode(data.AsSpan(0, i)))
                .ToArray();
            int[] distinctHashCodes = hashCodes
                .Distinct()
                .ToArray();

            Assert.True(distinctHashCodes.Length >= hashCodes.Length * 0.95);
        }

        [Fact]
        public static void GetHashCodeSpan_SameLengths_ReturnsDistinctResults()
        {
            var random = new Random();
            byte[] data = new byte[100];

            int[] hashCodes = Enumerable.Range(0, data.Length)
                .Select(i =>
                {
                    random.NextBytes(data);
                    return BencodeStringComparer.GetHashCode(data.AsSpan());
                })
                .ToArray();
            int[] distinctHashCodes = hashCodes
                .Distinct()
                .ToArray();

            Assert.True(distinctHashCodes.Length >= hashCodes.Length * 0.95);
        }

        [Fact]
        public static void GetHashCodeArray_DifferentLengths_ReturnsDistinctResults()
        {
            var random = new Random();
            byte[] data = new byte[100];
            random.NextBytes(data);

            int[] hashCodes = Enumerable.Range(0, data.Length)
                .Select(i => BencodeStringComparer.Instance.GetHashCode(data.AsSpan(0, i).ToArray()))
                .ToArray();
            int[] distinctHashCodes = hashCodes
                .Distinct()
                .ToArray();

            Assert.True(distinctHashCodes.Length >= hashCodes.Length * 0.95);
        }

        [Fact]
        public static void GetHashCodeArray_SameLengths_ReturnsDistinctResults()
        {
            var random = new Random();
            byte[] data = new byte[100];

            int[] hashCodes = Enumerable.Range(0, data.Length)
                .Select(i =>
                {
                    random.NextBytes(data);
                    return BencodeStringComparer.Instance.GetHashCode(data);
                })
                .ToArray();
            int[] distinctHashCodes = hashCodes
                .Distinct()
                .ToArray();

            Assert.True(distinctHashCodes.Length >= hashCodes.Length * 0.95);
        }

        [Fact]
        public static void GetHashCodeMemory_DifferentLengths_ReturnsDistinctResults()
        {
            var random = new Random();
            byte[] data = new byte[100];
            random.NextBytes(data);

            int[] hashCodes = Enumerable.Range(0, data.Length)
                .Select(i => BencodeStringComparer.Instance.GetHashCode(data.AsMemory(0, i)))
                .ToArray();
            int[] distinctHashCodes = hashCodes
                .Distinct()
                .ToArray();

            Assert.True(distinctHashCodes.Length >= hashCodes.Length * 0.95);
        }

        [Fact]
        public static void GetHashCodeMemory_SameLengths_ReturnsDistinctResults()
        {
            var random = new Random();
            byte[] data = new byte[100];

            int[] hashCodes = Enumerable.Range(0, data.Length)
                .Select(i =>
                {
                    random.NextBytes(data);
                    return BencodeStringComparer.Instance.GetHashCode(data.AsMemory());
                })
                .ToArray();
            int[] distinctHashCodes = hashCodes
                .Distinct()
                .ToArray();

            Assert.True(distinctHashCodes.Length >= hashCodes.Length * 0.95);
        }
    }
}
