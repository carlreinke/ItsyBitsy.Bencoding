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
using ItsyBitsy.Xunit;
using System;
using Xunit;
using BTT = ItsyBitsy.Bencoding.BencodeTokenType;

namespace ItsyBitsy.Bencoding.Tests
{
    public partial class BencodeSpanWriterTests
    {
        [Fact]
        public static void WriteInteger_InInitialState_GoesToFinalState()
        {
            byte[] bencode = $"i1e".ToUtf8();
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);

            writer.WriteInteger(1);
            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteInteger(0));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InListValueState_GoesToListValueState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            string listBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] listBencode = $"li1e{listBodyBencodeString}e".ToUtf8();
            byte[] buffer = new byte[listBencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadListHead();
            writer.WriteListHead();

            writer.WriteInteger(1);
            Copy(ref reader, ref writer, tokenTypes);

            reader.ReadListTail();
            writer.WriteListTail();

            Assert.Equal(listBencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InDictionaryValueState_GoesToDictionaryKeyState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            string dictionaryBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] dictionaryBencode = $"d1:!i1e{dictionaryBodyBencodeString}e".ToUtf8();
            byte[] buffer = new byte[dictionaryBencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();
            writer.WriteKey("!".ToUtf8());

            writer.WriteInteger(1);
            Copy(ref reader, ref writer, tokenTypes);

            reader.ReadDictionaryTail();
            writer.WriteDictionaryTail();

            Assert.Equal(dictionaryBencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            Copy(ref reader, ref writer, tokenTypes);

            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteInteger(1));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        [Fact]
        public static void WriteInteger_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] buffer = Array.Empty<byte>();
            var writer = new BencodeSpanWriter(buffer);
            _ = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteString(Array.Empty<byte>()));

            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteInteger(1));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InInitialState_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = bencodeString.ToUtf8();
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);

            writer.WriteInteger(value);

            Assert.Equal(bencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InListValueState_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            writer.WriteListHead();

            writer.WriteInteger(value);

            writer.WriteListTail();

            Assert.Equal(bencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InDictionaryValueState_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = $"d1:a{bencodeString}e".ToUtf8();
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            writer.WriteDictionaryHead();
            writer.WriteKey("a".ToUtf8());

            writer.WriteInteger(value);

            writer.WriteDictionaryTail();

            Assert.Equal(bencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_OversizedBuffer_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = bencodeString.ToUtf8();
            byte[] buffer = new byte[bencode.Length + 1];
            var writer = new BencodeSpanWriter(buffer);

            writer.WriteInteger(value);

            Assert.Equal(bencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_UndersizedBuffer_ThrowsInvalidOperationException(string bencodeString, long value)
        {
            byte[] bencode = bencodeString.ToUtf8();
            for (int i = 1; i < bencode.Length; ++i)
            {
                byte[] buffer = new byte[bencode.Length - i];
                var writer = new BencodeSpanWriter(buffer);

                var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteInteger(value));

                Assert.Equal("Reached the end of the destination buffer while attempting to write.", ex.Message);
            }
        }
    }
}
