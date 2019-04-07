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
    public abstract partial class BencodeReaderTestsBase
    {
        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InInitialState_DataAndTokensAndToken), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InListValueState_DataAndTokensAndToken), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryValueState_DataAndTokensAndToken), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryKeyState_DataAndTokensAndToken), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InAnyTerminalState_DataAndTokensAndToken), MemberType = typeof(BencodeTestData))]
        public void ReadTokenType_InAnyTerminalState_ReturnsExpectedResult(string bencodeString, BTT[] tokenTypes, BTT expectedTokenType)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = CreateReader(bencode);
            Expect(reader, tokenTypes);

            var tokenType = reader.ReadTokenType();

            Assert.Equal(expectedTokenType, tokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InInitialState_DataAndTokensAndPosition), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InListValueState_DataAndTokensAndPosition), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryValueState_DataAndTokensAndPosition), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryKeyState_DataAndTokensAndPosition), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InAnyTerminalState_DataAndTokensAndPosition), MemberType = typeof(BencodeTestData))]
        public void ReadTokenType_InAnyTerminalState_PositionIsUnchanged(string bencodeString, BTT[] tokenTypes, int expectedPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = CreateReader(bencode);
            Expect(reader, tokenTypes);
            long position = reader.Position;

            _ = reader.ReadTokenType();

            Assert.Equal(expectedPosition, position);
            Assert.Equal(expectedPosition, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InInitialStateButMissingData_DataAndTokensAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InInitialStateButInvalidData_DataAndTokensAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InListValueStateButMissingData_DataAndTokensAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InListValueStateButInvalidData_DataAndTokensAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryValueStateButMissingData_DataAndTokensAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryValueStateButInvalidData_DataAndTokensAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryKeyStateButMissingData_DataAndTokensAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryKeyStateWithInvalidData_DataAndTokensAndError), MemberType = typeof(BencodeTestData))]
        public void ReadTokenType_InDictionaryKeyStateButNotFound_ThrowsInvalidBencodeException(string bencodeString, BTT[] tokenTypes, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = CreateReader(bencode);
            Expect(reader, tokenTypes);

            var ex = Assert.Throws<InvalidBencodeException>(() => _ = reader.ReadTokenType());
            long position = reader.Position;

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
        }
    }
}
