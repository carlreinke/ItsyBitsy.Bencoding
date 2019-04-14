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
using BTT = ItsyBitsy.Bencoding.BencodeTokenType;

namespace ItsyBitsy.Bencoding.Tests
{
    public partial class BencodeReaderTests
    {
        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            Expect(reader, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.ReadStringLength());

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        [Fact]
        public static void ReadStringLength_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "1:a".ToUtf8();
            var reader = new BencodeReader(bencode);
            _ = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadInteger());

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.ReadStringLength());

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void ReadString_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            Expect(reader, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.ReadString());

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        [Fact]
        public static void ReadString_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "1:a".ToUtf8();
            var reader = new BencodeReader(bencode);
            _ = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadInteger());

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.ReadString());

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidData_ReturnsLength(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            int length = reader.ReadStringLength();

            Assert.Equal(expectedValue.Length, length);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidDataInList_ReturnsLength(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
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
            var reader = new BencodeReader(bencode);
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
            var reader = new BencodeReader(bencode);

            var value = reader.ReadString();

            Assert.Equal(expectedValue, value.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInList_ReturnsValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
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
            var reader = new BencodeReader(bencode);
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
            var reader = new BencodeReader(bencode);
            _ = reader.ReadStringLength();

            var value = reader.ReadString();

            Assert.Equal(expectedValue, value.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidData_GoesToFinalState(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            _ = reader.ReadString();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.None, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInList_GoesToValueState(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
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
            var reader = new BencodeReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            _ = reader.ReadString();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.DictionaryTail, tokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidData_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            int position = reader.Position;

            _ = reader.ReadStringLength();

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_ValidDataInList_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
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
            var reader = new BencodeReader(bencode);
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
            var reader = new BencodeReader(bencode);

            _ = reader.ReadString();

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInList_PositionIsAfterString(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            _ = reader.ReadString();

            Assert.Equal(1 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadString_ValidDataInDictionary_PositionIsAfterString(string bencodeString)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
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
            var reader = new BencodeReader(bencode);
            _ = reader.ReadStringLength();

            _ = reader.ReadString();

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_InvalidHeadData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadStringLength());
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
            var reader = new BencodeReader(bencode);

            int length = reader.ReadStringLength();

            Assert.Equal(expectedLength, length);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_MissingHeadDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadStringLength());
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
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadStringLength());
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
            var reader = new BencodeReader(bencode);
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
            var reader = new BencodeReader(bencode);

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadString());
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
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadString());
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
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadString());
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
            var reader = new BencodeReader(bencode);
            _ = reader.ReadStringLength();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadString());
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
            var reader = new BencodeReader(bencode);
            int position = reader.Position;

            _ = reader.ReadStringLength();

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_MissingBodyDataInList_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();
            int position = reader.Position;

            _ = reader.ReadStringLength();

            Assert.Equal(position, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public static void ReadStringLength_UnsupportedData_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            var ex = Assert.Throws<UnsupportedBencodeException>(() => _ = reader.ReadStringLength());
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
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<UnsupportedBencodeException>(() => _ = reader.ReadStringLength());
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
            var reader = new BencodeReader(bencode);

            var ex = Assert.Throws<UnsupportedBencodeException>(() => _ = reader.ReadString());
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
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<UnsupportedBencodeException>(() => _ = reader.ReadString());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }
    }
}
