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
using ItsyBitsy.Xunit;
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
        public static void ReadValueToBencodeSpanWriter_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            Expect(ref reader, tokenTypes);

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) =>
            {
                var writer = new BencodeSpanWriter();
                r.ReadValueTo(ref writer);
            });

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        [Fact]
        public static void ReadValueToBencodeSpanWriter_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] bencode = "i1e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            _ = AssertThrows<InvalidBencodeException>(ref reader, (ref BencodeSpanReader r) => _ = r.ReadString());

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) =>
            {
                var writer = new BencodeSpanWriter();
                r.ReadValueTo(ref writer);
            });

            Assert.Equal("The reader is not in a state that allows a value to be read.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_InInitialState_WritesExpectedData(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);

            reader.ReadValueTo(ref writer);

            Assert.Equal(bencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_InListValueState_WritesExpectedData(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            byte[] listBencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(listBencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadListHead();

            reader.ReadValueTo(ref writer);

            Assert.Equal(bencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_InDictionaryValueState_WritesExpectedData(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            byte[] listBencode = $"d1:a{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(listBencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadDictionaryHead();
            reader.SkipKey();

            reader.ReadValueTo(ref writer);

            Assert.Equal(bencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_ValidData_PositionIsAfterValue(string bencodeString)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);

            reader.ReadValueTo(ref writer);

            Assert.Equal(bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_ValidDataInList_PositionIsAfterValue(string bencodeString)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadListHead();
            writer.WriteListHead();

            reader.ReadValueTo(ref writer);

            Assert.Equal(1 + bencodeString.Length, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_Data), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_Data), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_ValidDataInDictionary_PositionIsAfterValue(string bencodeString)
        {
            byte[] bencode = $"d0:{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();
            writer.WriteKey(reader.ReadKey());

            reader.ReadValueTo(ref writer);

            Assert.Equal(3 + bencodeString.Length, reader.Position);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.SkipValue_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InInitialStateButMissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InListValueStateButMissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryValueStateButMissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryKeyStateButMissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.SkipValue_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InInitialStateButInvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InListValueStateButInvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryValueStateButInvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryKeyStateButInvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_InvalidData_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);

            var ex = AssertThrows<InvalidBencodeException>(ref reader, ref writer, (ref BencodeSpanReader r, ref BencodeSpanWriter w) =>
            {
                r.ReadValueTo(ref w);
            });
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.SkipValue_MissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InInitialStateButMissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InListValueStateButMissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryValueStateButMissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryKeyStateButMissingData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_MissingDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, ref writer, (ref BencodeSpanReader r, ref BencodeSpanWriter w) =>
            {
                r.ReadValueTo(ref w);
            });
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.SkipValue_InvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InInitialStateButInvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InListValueStateButInvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryValueStateButInvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.ReadTokenType_InDictionaryKeyStateButInvalidData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_InvalidDataInList_ThrowsInvalidBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadListHead();

            var ex = AssertThrows<InvalidBencodeException>(ref reader, ref writer, (ref BencodeSpanReader r, ref BencodeSpanWriter w) =>
            {
                r.ReadValueTo(ref w);
            });
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(Math.Min(ex.Position + 1, bencode.Length), position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.SkipValue_UnsupportedData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_UnsupportedData_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            var ex = AssertThrows<UnsupportedBencodeException>(ref reader, (ref BencodeSpanReader r) =>
            {
                var writer = new BencodeSpanWriter();
                r.ReadValueTo(ref writer);
            });
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(errorPosition, ex.Position);
            Assert.Equal(expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.SkipValue_UnsupportedData_DataAndError), MemberType = typeof(BencodeTestData))]
        public static void ReadValueToBencodeSpanWriter_UnsupportedDataInList_ThrowsUnsupportedBencodeExceptionAndGoesToErrorState(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            reader.ReadListHead();

            var ex = AssertThrows<UnsupportedBencodeException>(ref reader, (ref BencodeSpanReader r) =>
            {
                var writer = new BencodeSpanWriter();
                r.ReadValueTo(ref writer);
            });
            int position = reader.Position;
            var errorTokenType = reader.ReadTokenType();

            Assert.Equal(errorMessage, ex.Message);
            Assert.Equal(1 + errorPosition, ex.Position);
            Assert.Equal(1 + expectedPosition, position);
            Assert.Equal(BTT.None, errorTokenType);
        }
    }
}
