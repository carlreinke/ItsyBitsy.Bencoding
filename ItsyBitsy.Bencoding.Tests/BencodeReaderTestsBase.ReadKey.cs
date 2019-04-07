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
using System.Text;
using Xunit;
using BTT = ItsyBitsy.Bencoding.BencodeTokenType;

namespace ItsyBitsy.Bencoding.Tests
{
    public abstract partial class BencodeReaderTestsBase
    {
        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public void ReadKeyLength_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = CreateReader(bencode);
            Expect(reader, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.ReadKeyLength());

            Assert.Equal("The reader is not in a state that allows a key to be read.", ex.Message);
        }

        [Fact]
        public void ReadKeyLength_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "d1:ae1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            _ = Assert.Throws<InvalidBencodeException>(() => reader.ReadDictionaryTail());

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.ReadKeyLength());

            Assert.Equal("The reader is not in a state that allows a key to be read.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public void ReadKey_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = CreateReader(bencode);
            Expect(reader, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.ReadKey());

            Assert.Equal("The reader is not in a state that allows a key to be read.", ex.Message);
        }

        [Fact]
        public void ReadKey_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "d1:ae1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            _ = Assert.Throws<InvalidBencodeException>(() => reader.ReadDictionaryTail());

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.ReadKey());

            Assert.Equal("The reader is not in a state that allows a key to be read.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = CreateReader(bencode);
            Expect(reader, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.TryReadKey(default, out _));

            Assert.Equal("The reader is not in a state that allows a key to be read.", ex.Message);
        }

        [Fact]
        public void TryReadKey_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "d1:ae1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            _ = Assert.Throws<InvalidBencodeException>(() => reader.ReadDictionaryTail());

            var ex = Assert.Throws<InvalidOperationException>(() => _ = reader.TryReadKey(default, out _));

            Assert.Equal("The reader is not in a state that allows a key to be read.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void ReadKeyLength_ValidData_ReturnsLength(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            int length = reader.ReadKeyLength();

            Assert.Equal(expectedValue.Length, length);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void ReadKey_ValidData_ReturnsValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var value = reader.ReadKey();

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void ReadKey_ValidDataAfterReadKeyLength_ReturnsValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            _ = reader.ReadKeyLength();

            var value = reader.ReadKey();

            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public void ReadKey_ValidData_GoesToValueState(string bencodeString)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            _ = reader.ReadKey();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.Integer, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_ValidData_ReturnsTrueAndValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            byte[] value = new byte[expectedValue.Length];

            int bytesWritten;
            bool result = reader.TryReadKey(value, out bytesWritten);

            Assert.True(result);
            Assert.Equal(expectedValue.Length, bytesWritten);
            Assert.Equal(expectedValue, value);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_ValidDataOversizedBuffer_ReturnsTrueAndValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            byte[] value = new byte[expectedValue.Length + 1];

            int bytesWritten;
            bool result = reader.TryReadKey(value, out bytesWritten);

            Assert.True(result);
            Assert.Equal(expectedValue.Length, bytesWritten);
            Assert.Equal(expectedValue, value.AsMemory().Slice(0, bytesWritten).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_ValidDataUndersizedBuffer_ReturnsFalse(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            if (expectedValue.Length == 0)
                return;

            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            byte[] value = new byte[expectedValue.Length - 1];

            int bytesWritten;
            bool result = reader.TryReadKey(value, out bytesWritten);

            Assert.False(result);
            Assert.Equal(0, bytesWritten);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_ValidDataAfterReadKeyLength_ReturnsTrueAndValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            byte[] value = new byte[expectedValue.Length];
            _ = reader.ReadKeyLength();

            int bytesWritten;
            bool result = reader.TryReadKey(value, out bytesWritten);

            Assert.True(result);
            Assert.Equal(expectedValue.Length, bytesWritten);
            Assert.Equal(expectedValue, value);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public void ReadKeyLength_ValidData_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            long position = reader.Position;

            _ = reader.ReadKeyLength();

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public void ReadKey_ValidData_PositionIsAfterKey(string bencodeString)
        {
            int keyBencodeLength = Encoding.UTF8.GetByteCount(bencodeString);
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            _ = reader.ReadKey();

            Assert.Equal(1 + keyBencodeLength, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public void ReadKey_ValidDataAfterReadKeyLength_PositionIsAfterKey(string bencodeString)
        {
            int keyBencodeLength = Encoding.UTF8.GetByteCount(bencodeString);
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            _ = reader.ReadKeyLength();

            _ = reader.ReadKey();

            Assert.Equal(1 + keyBencodeLength, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_ValidData_PositionIsAfterKey(string bencodeString, string expectedValueString)
        {
            int keyBencodeLength = Encoding.UTF8.GetByteCount(bencodeString);
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            byte[] value = new byte[expectedValue.Length];

            _ = reader.TryReadKey(value, out _);

            Assert.Equal(1 + keyBencodeLength, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_ValidDataOversizedBuffer_PositionIsAfterKey(string bencodeString, string expectedValueString)
        {
            int keyBencodeLength = Encoding.UTF8.GetByteCount(bencodeString);
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            byte[] value = new byte[expectedValue.Length + 1];

            _ = reader.TryReadKey(value, out _);

            Assert.Equal(1 + keyBencodeLength, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_ValidDataUndersizedBuffer_PositionIsUnchanged(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            if (expectedValue.Length == 0)
                return;

            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            byte[] value = new byte[expectedValue.Length - 1];
            long position = reader.Position;

            _ = reader.TryReadKey(value, out _);

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_ValidDataAfterReadKeyLength_PositionIsAfterKey(string bencodeString, string expectedValueString)
        {
            int keyBencodeLength = Encoding.UTF8.GetByteCount(bencodeString);
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            byte[] value = new byte[expectedValue.Length];
            _ = reader.ReadKeyLength();

            _ = reader.TryReadKey(value, out _);

            Assert.Equal(1 + keyBencodeLength, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndError), MemberType = typeof(BencodeTestData))]
        public void ReadKeyLength_MissingHeadData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadKeyLength());
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public void ReadKeyLength_InvalidHeadData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadKeyLength());
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLength), MemberType = typeof(BencodeTestData))]
        public void ReadKeyLength_MissingBodyData_ReturnsLength(string bencodeString, int expectedLength)
        {
            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            int length = reader.ReadKeyLength();

            Assert.Equal(expectedLength, length);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndError), MemberType = typeof(BencodeTestData))]
        public void ReadKey_MissingData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadKey());
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public void ReadKey_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadKey());
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndError), MemberType = typeof(BencodeTestData))]
        public void ReadKey_MissingBodyDataAfterReadStringLength_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            _ = reader.ReadKeyLength();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadKey());
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_MissingData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            byte[] value = new byte[expectedLength];
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.TryReadKey(value, out _));
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            byte[] value = new byte[expectedLength];
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.TryReadKey(value, out _));
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_MissingBodyDataOversizedBuffer_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            byte[] value = new byte[expectedLength + 1];
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.TryReadKey(value, out _));
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLength), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_MissingBodyDataUndersizedBuffer_ReturnsFalse(string bencodeString, int expectedLength)
        {
            if (expectedLength == 0 || expectedLength > 4096)
                return;

            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            byte[] value = new byte[expectedLength - 1];
            reader.ReadDictionaryHead();

            int bytesWritten;
            bool result = reader.TryReadKey(value, out bytesWritten);

            Assert.False(result);
            Assert.Equal(0, bytesWritten);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLengthAndError), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_MissingBodyDataAfterReadStringLength_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, int expectedLength, string errorMessage, int errorPosition)
        {
            if (expectedLength > 4096)
                return;

            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            byte[] value = new byte[expectedLength];
            reader.ReadDictionaryHead();
            _ = reader.ReadKeyLength();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.TryReadKey(value, out _));
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_Data), MemberType = typeof(BencodeTestData))]
        public void ReadKeyLength_MissingBodyData_PositionIsUnchanged(string bencodeString)
        {
            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            long position = reader.Position;

            _ = reader.ReadKeyLength();

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndLength), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_MissingBodyDataUndersizedBuffer_PositionIsUnchanged(string bencodeString, int expectedLength)
        {
            if (expectedLength == 0 || expectedLength > 4096)
                return;

            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            byte[] value = new byte[expectedLength - 1];
            reader.ReadDictionaryHead();
            long position = reader.Position;

            _ = reader.TryReadKey(value, out _);

            Assert.Equal(position, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public void ReadKeyLength_UnsupportedData_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<UnsupportedBencodeException>(() => _ = reader.ReadKeyLength());
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public void ReadKey_UnsupportedData_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<UnsupportedBencodeException>(() => _ = reader.ReadKey());
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public void TryReadKey_UnsupportedData_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            byte[] value = Array.Empty<byte>();
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<UnsupportedBencodeException>(() => _ = reader.TryReadKey(value, out _));
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }
    }
}
