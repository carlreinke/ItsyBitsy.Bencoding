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
        public static void NonInterfaceReadDictionary_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            Expect(reader, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => reader.ReadDictionary());

            Assert.Equal("The reader is not in a state that allows a dictionary head to be read.", ex.Message);
        }

        [Fact]
        public static void NonInterfaceReadDictionary_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "d".ToUtf8();
            var reader = new BencodeReader(bencode);
            _ = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadInteger());

            var ex = Assert.Throws<InvalidOperationException>(() => reader.ReadDictionary());

            Assert.Equal("The reader is not in a state that allows a dictionary head to be read.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_ValidData_GoesToFinalState(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            _ = reader.ReadDictionary();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.None, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_ValidDataInList_GoesToValueState(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            _ = reader.ReadDictionary();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.ListTail, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_ValidDataInDictionary_GoesToKeyState(string bencodeString)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            _ = reader.ReadDictionary();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.DictionaryTail, tokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_ValidData_PositionIsAfterDictionary(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            _ = reader.ReadDictionary();

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_ValidDataInList_PositionIsAfterDictionary(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            _ = reader.ReadDictionary();

            Assert.Equal(1 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_ValidDataInDictionary_PositionIsAfterDictionary(string bencodeString)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            _ = reader.ReadDictionary();

            Assert.Equal(3 + bencodeString.Length, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadDictionary());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_MissingDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadDictionary());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_InvalidDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadListHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadDictionary());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_NonduplicateKeys_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_NonduplicateKeys_DoesNotThrow(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            _ = reader.ReadDictionary();
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_DuplicateKeys_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_DuplicateKeys_ThrowsInvalidBencodeExceptionAndGoesToNextState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadDictionary(skipDuplicateKeys: false));
            int position = reader.Position;
            var tokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(bencode.Length, position);
            Assert.Equal(BTT.None, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_DuplicateKeys_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_DuplicateKeysInList_ThrowsInvalidBencodeExceptionAndGoesToNextState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            byte[] listBencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeReader(listBencode);
            reader.ReadListHead();

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadDictionary(skipDuplicateKeys: false));
            int position = reader.Position;
            var tokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + bencode.Length, position);
            Assert.Equal(BTT.ListTail, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_DuplicateKeys_Data), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_DuplicateKeysSkipDuplicateKeysEnabled_DoesNotThrow(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            _ = reader.ReadDictionary(skipDuplicateKeys: true);
        }

        [Fact]
        public static void NonInterfaceReadDictionary_DuplicateKeySkipDuplicateKeysEnabled_KeepsFirstKey()
        {
            byte[] bencode = "d1:ai1e1:ai2ee".ToUtf8();
            var reader = new BencodeReader(bencode);
            var dictionary = reader.ReadDictionary(skipDuplicateKeys: true);

            int position;
            Assert.True(dictionary.TryGetPosition("a".ToUtf8(), out position));
            Assert.Equal(4, position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_DataAndElements), MemberType = typeof(BencodeTestData))]
        public static void NonInterfaceReadDictionary_Always_ReturnsDictionaryWithExpectedCountAndElements(string bencodeString, (string key, int position)[] expectedElements)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var dictionary = reader.ReadDictionary();

            Assert.Equal(expectedElements.Length, dictionary.Count);
            foreach (var (key, expectedPosition) in expectedElements)
            {
                int position;
                Assert.True(dictionary.TryGetPosition(key.ToUtf8(), out position));
                Assert.Equal(expectedPosition, position);
            }
        }
    }
}
