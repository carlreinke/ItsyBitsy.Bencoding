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
using System.Text;
using Xunit;
using BTT = ItsyBitsy.Bencoding.BencodeTokenType;

namespace ItsyBitsy.Bencoding.Tests
{
    public abstract partial class BencodeReaderTestsBase
    {
        [Fact]
        public void ReadKeyToIBencodeWriter_WriterIsNull_ThrowsArgumentNullException()
        {
            byte[] bencode = "d1:ai1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<ArgumentNullException>(() => reader.ReadKeyTo(null));

            Assert.Equal("writer", ex.ParamName);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public void ReadKeyToIBencodeWriter_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = CreateReader(bencode);
            Expect(reader, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var writer = new BencodeWriter();
                reader.ReadKeyTo(writer);
            });

            Assert.Equal("The reader is not in a state that allows a key to be read.", ex.Message);
        }

        [Fact]
        public void ReadKeyToIBencodeWriter_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "d1:ae1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            _ = Assert.Throws<InvalidBencodeException>(() => reader.ReadDictionaryTail());

            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var writer = new BencodeWriter();
                reader.ReadKeyTo(writer);
            });

            Assert.Equal("The reader is not in a state that allows a key to be read.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public void ReadKeyToIBencodeWriter_Always_WritesExpectedData(string bencodeString)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            var writer = new BencodeWriter();
            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();

            reader.ReadKeyTo(writer);

            reader.ReadValueTo(writer);
            reader.ReadDictionaryTail();
            writer.WriteDictionaryTail();

            byte[] buffer = writer.Encode();
            Assert.Equal(bencode, buffer);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public void ReadKeyToIBencodeWriter_ValidData_PositionIsAfterKey(string bencodeString)
        {
            int keyBencodeLength = Encoding.UTF8.GetByteCount(bencodeString);
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            var writer = new BencodeWriter();
            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();

            reader.ReadKeyTo(writer);

            Assert.Equal(1 + keyBencodeLength, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public void ReadKeyToIBencodeWriter_ValidDataAfterReadKeyLength_PositionIsAfterKey(string bencodeString)
        {
            int keyBencodeLength = Encoding.UTF8.GetByteCount(bencodeString);
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            var writer = new BencodeWriter();
            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();
            _ = reader.ReadKeyLength();

            reader.ReadKeyTo(writer);

            Assert.Equal(1 + keyBencodeLength, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingHeadData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndError), MemberType = typeof(BencodeTestData))]
        public void ReadKeyToBencodeWriter_MissingData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<InvalidBencodeException>(() =>
            {
                var writer = new BencodeWriter();
                reader.ReadKeyTo(writer);
            });
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public void ReadKeyToBencodeWriter_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<InvalidBencodeException>(() =>
            {
                var writer = new BencodeWriter();
                reader.ReadKeyTo(writer);
            });
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_MissingBodyData_DataAndError), MemberType = typeof(BencodeTestData))]
        public void ReadKeyToBencodeWriter_MissingBodyDataAfterReadStringLength_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();
            _ = reader.ReadKeyLength();

            var ex = Assert.Throws<InvalidBencodeException>(() =>
            {
                var writer = new BencodeWriter();
                reader.ReadKeyTo(writer);
            });
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_UnsupportedData_DataAndErrorAndPosition), MemberType = typeof(BencodeTestData))]
        public void ReadKeyToIBencodeWriter_UnsupportedData_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = CreateReader(bencode);
            reader.ReadDictionaryHead();

            var ex = Assert.Throws<UnsupportedBencodeException>(() =>
            {
                var writer = new BencodeWriter();
                reader.ReadKeyTo(writer);
            });
            long position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }
    }
}
