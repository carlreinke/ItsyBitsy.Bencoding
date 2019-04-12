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
    public partial class BencodeReaderTests : BencodeReaderTestsBase
    {
        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            Expect(reader, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => reader.ReadList());

            Assert.Equal("The reader is not in a state that allows a list head to be read.", ex.Message);
        }

        [Fact]
        public static void NonInterfaceReadList_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "d".ToUtf8();
            var reader = new BencodeReader(bencode);
            _ = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadInteger());

            var ex = Assert.Throws<InvalidOperationException>(() => reader.ReadList());

            Assert.Equal("The reader is not in a state that allows a list head to be read.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_ValidData_GoesToFinalState(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            _ = reader.ReadList();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.None, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_ValidDataInList_GoesToValueState(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            _ = reader.ReadList();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.ListTail, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_ValidDataInDictionary_GoesToKeyState(string bencodeString)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            _ = reader.ReadList();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.DictionaryTail, tokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_ValidData_PositionIsAfterList(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            _ = reader.ReadList();

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_ValidDataInList_PositionIsAfterList(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            _ = reader.ReadList();

            Assert.Equal(1 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_ValidDataInDictionary_PositionIsAfterList(string bencodeString)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            _ = reader.ReadList();

            Assert.Equal(3 + bencodeString.Length, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadList_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            var ex = Assert.Throws<InvalidBencodeException>(() => reader.ReadList());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_MissingDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => reader.ReadList());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadList_InvalidDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => reader.ReadList());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }
    }
}
