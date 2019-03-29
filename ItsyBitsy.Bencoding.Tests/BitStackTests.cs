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
using Xunit;

namespace ItsyBitsy.Bencoding.Tests
{
    public static class BitStackTests
    {
        [Fact]
        public static void Count_DefaultInstance_IsZero()
        {
            BitStack bitStack = default;

            Assert.Equal(0, bitStack.Count);
        }

        [Theory]
        [InlineData(199, 0)]
        [InlineData(199, 1)]
        [InlineData(199, 198)]
        [InlineData(199, 199)]
        [InlineData(200, 0)]
        [InlineData(200, 1)]
        [InlineData(200, 199)]
        [InlineData(200, 200)]
        public static void Count_PushedAndPopped_IsPushedCountMinusPoppedCount(int pushCount, int popCount)
        {
            BitStack bitStack = default;
            PushRandomBits(ref bitStack, pushCount);
            for (int i = 0; i < popCount; ++i)
                _ = bitStack.Pop();

            Assert.Equal(pushCount - popCount, bitStack.Count);
        }

        [Fact]
        public static void PushFalse_DefaultInstance_CountIsOne()
        {
            BitStack bitStack = default;

            bitStack.PushFalse();

            Assert.Equal(1, bitStack.Count);
        }

        [Fact]
        public static void PushTrue_DefaultInstance_CountIsOne()
        {
            BitStack bitStack = default;

            bitStack.PushTrue();

            Assert.Equal(1, bitStack.Count);
        }

        [Fact]
        public static void PushFalse_AfterClear_CountIsOne()
        {
            BitStack bitStack = default;
            bitStack.PushTrue();
            bitStack.Clear();

            bitStack.PushFalse();

            Assert.Equal(1, bitStack.Count);
        }

        [Fact]
        public static void PushTrue_AfterClear_CountIsOne()
        {
            BitStack bitStack = default;
            bitStack.PushFalse();
            bitStack.Clear();

            bitStack.PushTrue();

            Assert.Equal(1, bitStack.Count);
        }

        [Fact]
        public static void Pop_PushedFalse_ReturnsFalse()
        {
            BitStack bitStack = default;
            bitStack.PushFalse();

            bool result = bitStack.Pop();

            Assert.False(result);
        }

        [Fact]
        public static void Pop_PushedTrue_ReturnsTrue()
        {
            BitStack bitStack = default;
            bitStack.PushTrue();

            bool result = bitStack.Pop();

            Assert.True(result);
        }

        [Fact]
        public static void Pop_PushedFalsePoppedPushedTrue_ReturnsTrue()
        {
            BitStack bitStack = default;
            bitStack.PushFalse();
            _ = bitStack.Pop();
            bitStack.PushTrue();

            bool result = bitStack.Pop();

            Assert.True(result);
        }

        [Fact]
        public static void Pop_PushedTruePoppedPushedFalse_ReturnsFalse()
        {
            BitStack bitStack = default;
            bitStack.PushTrue();
            _ = bitStack.Pop();
            bitStack.PushFalse();

            bool result = bitStack.Pop();

            Assert.False(result);
        }

        [Fact]
        public static void Pop_PushedFalseClearedPushedTrue_ReturnsTrue()
        {
            BitStack bitStack = default;
            bitStack.PushFalse();
            bitStack.Clear();
            bitStack.PushTrue();

            bool result = bitStack.Pop();

            Assert.True(result);
        }

        [Fact]
        public static void Pop_PushedTrueClearedPushedFalse_ReturnsFalse()
        {
            BitStack bitStack = default;
            bitStack.PushTrue();
            bitStack.Clear();
            bitStack.PushFalse();

            bool result = bitStack.Pop();

            Assert.False(result);
        }

        [Theory]
        [InlineData(199, false)]
        [InlineData(199, true)]
        [InlineData(200, false)]
        [InlineData(200, true)]
        public static void Pop_PushedAlternatingFalseAndTrue_ReturnsSeriesOfPushedValuesInReverseOrder(int count, bool invert)
        {
            BitStack bitStack = default;
            for (int i = 0; i < count; i += 1)
                if ((i & 1) == 1 == invert)
                    bitStack.PushTrue();
                else
                    bitStack.PushFalse();

            var poppedBits = new BitArray(count);
            for (int i = count - 1; i >= 0; i -= 1)
                poppedBits[i] = bitStack.Pop();

            for (int i = 0; i < count; i += 1)
                Assert.Equal((i & 1) == 1 == invert, poppedBits[i]);
        }

        [Theory]
        [InlineData(199)]
        [InlineData(200)]
        public static void Pop_PushedRandomFalseAndTrue_ReturnsSeriesOfPushedValuesInReverseOrder(int count)
        {
            BitStack bitStack = default;
            var pushedBits = PushRandomBits(ref bitStack, count);

            var poppedBits = new BitArray(count);
            for (int i = count - 1; i >= 0; i -= 1)
                poppedBits[i] = bitStack.Pop();

            for (int i = 0; i < count; i += 1)
                Assert.Equal(pushedBits[i], poppedBits[i]);
        }

        [Fact]
        public static void Clear_DefaultInstance_CountIsZero()
        {
            BitStack bitStack = default;

            bitStack.Clear();

            Assert.Equal(0, bitStack.Count);
        }

        [Theory]
        [InlineData(199, 0)]
        [InlineData(199, 1)]
        [InlineData(199, 198)]
        [InlineData(199, 199)]
        [InlineData(200, 0)]
        [InlineData(200, 1)]
        [InlineData(200, 199)]
        [InlineData(200, 200)]
        public static void Clear_PushedRandomFalseAndTrue_CountIsZero(int pushCount, int popCount)
        {
            BitStack bitStack = default;
            PushRandomBits(ref bitStack, pushCount);
            for (int i = 0; i < popCount; ++i)
                _ = bitStack.Pop();

            bitStack.Clear();

            Assert.Equal(0, bitStack.Count);
        }

        private static BitArray PushRandomBits(ref BitStack bitStack, int count)
        {
            var random = new Random(typeof(BitStack).MetadataToken);
            byte[] bytes = new byte[(count - 1) / 8 + 1];
            random.NextBytes(bytes);
            var pushedBits = new BitArray(bytes);
            for (int i = 0; i < count; i += 1)
                if (pushedBits[i])
                    bitStack.PushTrue();
                else
                    bitStack.PushFalse();
            return pushedBits;
        }
    }
}
