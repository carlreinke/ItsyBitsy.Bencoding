﻿//
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
using BTT = ItsyBitsy.Bencoding.BencodeTokenType;

namespace ItsyBitsy.Bencoding.Tests
{
    public partial class BencodeSpanReaderTests
    {
        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            Expect(ref reader, tokenTypes);

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadStringLength());

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        [Fact]
        public static void ReadStringLength_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "1:a".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            _ = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadInteger());

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadStringLength());

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void ReadString_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            Expect(ref reader, tokenTypes);

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadString());

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        [Fact]
        public static void ReadString_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "1:a".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            _ = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadInteger());

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadString());

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            Expect(ref reader, tokenTypes);

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(default, out _));

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        [Fact]
        public static void TryReadString_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "1:a".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            _ = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadInteger());

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(default, out _));

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidData_ReturnsLength(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            int length = reader.ReadStringLength();

            Assert.Equal(expectedValue.Length, length);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidDataInList_ReturnsLength(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            int length = reader.ReadStringLength();

            Assert.Equal(expectedValue.Length, length);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidDataInDictionary_ReturnsLength(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            int length = reader.ReadStringLength();

            Assert.Equal(expectedValue.Length, length);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidData_ReturnsValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            var value = reader.ReadString();

            Assert.Equal(expectedValue, value.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInList_ReturnsValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var value = reader.ReadString();

            Assert.Equal(expectedValue, value.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInDictionary_ReturnsValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            var value = reader.ReadString();

            Assert.Equal(expectedValue, value.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataAfterReadStringLength_ReturnsValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            _ = reader.ReadStringLength();

            var value = reader.ReadString();

            Assert.Equal(expectedValue, value.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidData_GoesToFinalState(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            _ = reader.ReadString();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.None, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInList_GoesToValueState(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            _ = reader.ReadString();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.ListTail, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInDictionary_GoesToKeyState(string bencodeString)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            _ = reader.ReadString();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.DictionaryTail, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidData_ReturnsTrueAndValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length];

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.True(result);
            Assert.Equal(expectedValue.Length, bytesWritten);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataOversizedBuffer_ReturnsTrueAndValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length + 1];

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.True(result);
            Assert.Equal(expectedValue.Length, bytesWritten);
            Assert.Equal(expectedValue, value.AsMemory().Slice(0, bytesWritten).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataUndersizedBuffer_ReturnsFalse(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            if (expectedValue.Length == 0)
                return;

            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length - 1];

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.False(result);
            Assert.Equal(0, bytesWritten);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataInList_ReturnsTrueAndValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length];
            reader.ReadListHead();

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.True(result);
            Assert.Equal(expectedValue.Length, bytesWritten);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataInListOversizedBuffer_ReturnsTrueAndValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length + 1];
            reader.ReadListHead();

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.True(result);
            Assert.Equal(expectedValue.Length, bytesWritten);
            Assert.Equal(expectedValue, value.AsMemory().Slice(0, bytesWritten).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataInListUndersizedBuffer_ReturnsFalse(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            if (expectedValue.Length == 0)
                return;

            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length - 1];
            reader.ReadListHead();

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.False(result);
            Assert.Equal(0, bytesWritten);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataInDictionary_ReturnsTrueAndValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length];
            reader.ReadDictionaryHead();
            reader.SkipKey();

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.True(result);
            Assert.Equal(expectedValue.Length, bytesWritten);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataAfterReadStringLength_ReturnsTrueAndValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length];
            _ = reader.ReadStringLength();

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.True(result);
            Assert.Equal(expectedValue.Length, bytesWritten);
            Assert.Equal(expectedValue, value);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidData_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            int position = reader.Position;

            _ = reader.ReadStringLength();

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidDataInList_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();
            int position = reader.Position;

            _ = reader.ReadStringLength();

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidDataInDictionary_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();
            int position = reader.Position;

            _ = reader.ReadStringLength();

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidData_PositionIsAfterString(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            _ = reader.ReadString();

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInList_PositionIsAfterString(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            _ = reader.ReadString();

            Assert.Equal(1 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInDictionary_PositionIsAfterString(string bencodeString)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            _ = reader.ReadString();

            Assert.Equal(3 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataAfterReadStringLength_PositionIsAfterString(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            _ = reader.ReadStringLength();

            _ = reader.ReadString();

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidData_PositionIsAfterString(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length];

            _ = reader.TryReadString(value, out _);

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataOversizedBuffer_PositionIsAfterString(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length + 1];

            _ = reader.TryReadString(value, out _);

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataUndersizedBuffer_PositionIsUnchanged(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            if (expectedValue.Length == 0)
                return;

            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length - 1];
            int position = reader.Position;

            _ = reader.TryReadString(value, out _);

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataInList_PositionIsAfterString(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length];
            reader.ReadListHead();

            _ = reader.TryReadString(value, out _);

            Assert.Equal(1 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataInListOversizedBuffer_PositionIsAfterString(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length + 1];
            reader.ReadListHead();

            _ = reader.TryReadString(value, out _);

            Assert.Equal(1 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataInListUndersizedBuffer_PositionIsUnchanged(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            if (expectedValue.Length == 0)
                return;

            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length - 1];
            reader.ReadListHead();
            int position = reader.Position;

            _ = reader.TryReadString(value, out _);

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataInDictionary_PositionIsAfterString(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length];
            reader.ReadDictionaryHead();
            reader.SkipKey();

            _ = reader.TryReadString(value, out _);

            Assert.Equal(3 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_ValidDataAfterReadStringLength_PositionIsAfterString(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedValue.Length];
            _ = reader.ReadStringLength();

            _ = reader.TryReadString(value, out _);

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_InvalidHeadData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadStringLength());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLength), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_MissingBodyData_ReturnsLength(string bencodeString, int expectedLength)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            int length = reader.ReadStringLength();

            Assert.Equal(expectedLength, length);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_MissingHeadDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadStringLength());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_InvalidHeadDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadStringLength());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLength), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_MissingBodyDataInList_ReturnsLength(string bencodeString, int expectedLength)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            int length = reader.ReadStringLength();

            Assert.Equal(expectedLength, length);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadString_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadString());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadString_MissingDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadString());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadString_InvalidDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadString());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadString_MissingBodyDataAfterReadStringLength_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            _ = reader.ReadStringLength();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadString());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength];

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(value, out _));
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_MissingBodyDataOversizedBuffer_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength + 1];

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(value, out _));
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLength), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_MissingBodyDataUndersizedBuffer_ReturnsFalse(string bencodeString, int expectedLength)
        {
            if (expectedLength == 0 || expectedLength > 4096)
                return;

            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength - 1];

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.False(result);
            Assert.Equal(0, bytesWritten);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_MissingDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength];
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(value, out _));
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_InvalidDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength];
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(value, out _));
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_MissingBodyDataInListOversizedBuffer_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength + 1];
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(value, out _));
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLength), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_MissingBodyDataInListUndersizedBuffer_ReturnsFalse(string bencodeString, int expectedLength)
        {
            if (expectedLength == 0 || expectedLength > 4096)
                return;

            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength - 1];
            reader.ReadListHead();

            int bytesWritten;
            bool result = reader.TryReadString(value, out bytesWritten);

            Assert.False(result);
            Assert.Equal(0, bytesWritten);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_MissingBodyDataAfterReadStringLength_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength];
            _ = reader.ReadStringLength();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(value, out _));
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_MissingBodyData_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            int position = reader.Position;

            _ = reader.ReadStringLength();

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_MissingBodyDataInList_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();
            int position = reader.Position;

            _ = reader.ReadStringLength();

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLength), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_MissingBodyDataUndersizedBuffer_PositionIsUnchanged(string bencodeString, int expectedLength)
        {
            if (expectedLength == 0 || expectedLength > 4096)
                return;

            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength - 1];
            int position = reader.Position;

            _ = reader.TryReadString(value, out _);

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLength), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_MissingBodyDataInListUndersizedBuffer_PositionIsUnchanged(string bencodeString, int expectedLength)
        {
            if (expectedLength == 0 || expectedLength > 4096)
                return;

            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = new byte[expectedLength - 1];
            reader.ReadListHead();
            int position = reader.Position;

            _ = reader.TryReadString(value, out _);

            Assert.Equal(position, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_UnsupportedData_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            var ex = AssertThrows<UnsupportedBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadStringLength());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_UnsupportedDataInList_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var ex = AssertThrows<UnsupportedBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadStringLength());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public static void ReadString_UnsupportedData_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            var ex = AssertThrows<UnsupportedBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadString());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public static void ReadString_UnsupportedDataInList_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var ex = AssertThrows<UnsupportedBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadString());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_UnsupportedData_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = Array.Empty<byte>();

            var ex = AssertThrows<UnsupportedBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(value, out _));
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public static void TryReadString_UnsupportedDataInList_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] value = Array.Empty<byte>();
            reader.ReadListHead();

            var ex = AssertThrows<UnsupportedBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.TryReadString(value, out _));
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }
    }
}