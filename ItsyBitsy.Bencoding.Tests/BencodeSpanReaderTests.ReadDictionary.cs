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
        public static void ReadDictionaryHead_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            Expect(ref reader, tokenTypes);

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryHead());

            Assert.Equal("The reader is not in a state that allows a dictionary head to be read.", ex.Message);
        }

        [Fact]
        public static void ReadDictionaryHead_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "d".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            _ = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadInteger());

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryHead());

            Assert.Equal("The reader is not in a state that allows a dictionary head to be read.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryTail_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            Expect(ref reader, tokenTypes);

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryTail());

            Assert.Equal("The reader is not in a state that allows a dictionary tail to be read.", ex.Message);
        }

        [Fact]
        public static void ReadDictionaryTail_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "de".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadDictionaryHead();
            _ = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => r.ReadKey());

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryTail());

            Assert.Equal("The reader is not in a state that allows a dictionary tail to be read.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryHeadToTail_ValidData_GoesToFinalState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            reader.ReadDictionaryHead();
            Expect(ref reader, tokenTypes);
            reader.ReadDictionaryTail();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.None, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryHeadToTail_ValidDataInList_GoesToValueState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            reader.ReadDictionaryHead();
            Expect(ref reader, tokenTypes);
            reader.ReadDictionaryTail();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.ListTail, tokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryHeadToTail_ValidDataInDictionary_GoesToKeyState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            reader.ReadDictionaryHead();
            Expect(ref reader, tokenTypes);
            reader.ReadDictionaryTail();
            var tokenType = reader.ReadTokenType();

            Assert.Equal(BTT.DictionaryTail, tokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryHeadToTail_ValidData_PositionIsAfterDictionary(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            reader.ReadDictionaryHead();
            Expect(ref reader, tokenTypes);
            reader.ReadDictionaryTail();

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryHeadToTail_ValidDataInList_PositionIsAfterDictionary(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            reader.ReadDictionaryHead();
            Expect(ref reader, tokenTypes);
            reader.ReadDictionaryTail();

            Assert.Equal(1 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryHeadToTail_ValidDataInDictionary_PositionIsAfterDictionary(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            reader.ReadDictionaryHead();
            Expect(ref reader, tokenTypes);
            reader.ReadDictionaryTail();

            Assert.Equal(3 + bencodeString.Length, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionaryHead_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionaryHead_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryHead_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryHead());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionaryHead_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryHead_MissingDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryHead());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionaryHead_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryHead_InvalidDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryHead());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionaryTail_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionaryTail_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryTail_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadDictionaryHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryTail());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionaryTail_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryTail_MissingDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();
            reader.ReadDictionaryHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryTail());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionaryTail_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadDictionaryTail_InvalidDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();
            reader.ReadDictionaryHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => r.ReadDictionaryTail());
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }
    }
}