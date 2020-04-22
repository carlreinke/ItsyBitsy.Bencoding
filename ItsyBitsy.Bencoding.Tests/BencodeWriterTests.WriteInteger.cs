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
    public partial class BencodeWriterTests
    {
        [Fact]
        public static void WriteInteger_InInitialState_GoesToFinalState()
        {
            byte[] bencode = $"i1e".ToUtf8();
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);

            writer.WriteInteger(1);
            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteInteger(0));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InListValueState_GoesToListValueState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            string listBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] listBencode = $"li1e{listBodyBencodeString}e".ToUtf8();
            var buffer = new FixedLengthBufferWriter(listBencode.Length);
            var writer = new BencodeWriter(buffer);
            reader.ReadListHead();
            writer.WriteListHead();

            writer.WriteInteger(1);
            Copy(reader, writer, tokenTypes);

            reader.ReadListTail();
            writer.WriteListTail();
            writer.Flush();

            Assert.Equal(listBencode, buffer.WrittenSpan.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InDictionaryValueState_GoesToDictionaryKeyState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            string dictionaryBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] dictionaryBencode = $"d1:!i1e{dictionaryBodyBencodeString}e".ToUtf8();
            var buffer = new FixedLengthBufferWriter(dictionaryBencode.Length);
            var writer = new BencodeWriter(buffer);
            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();
            writer.WriteKey("!".ToUtf8());

            writer.WriteInteger(1);
            Copy(reader, writer, tokenTypes);

            reader.ReadDictionaryTail();
            writer.WriteDictionaryTail();
            writer.Flush();

            Assert.Equal(dictionaryBencode, buffer.WrittenSpan.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);
            Copy(reader, writer, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteInteger(1));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        [Fact]
        public static void WriteInteger_InErrorState_ThrowsInvalidOperationException()
        {
            var buffer = new FixedLengthBufferWriter(0);
            var writer = new BencodeWriter(buffer);
            _ = Assert.Throws<InvalidOperationException>(() => writer.WriteString(Array.Empty<byte>()));

            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteInteger(1));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InInitialState_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);

            writer.WriteInteger(value);
            writer.Flush();

            Assert.Equal(bencode, buffer.WrittenSpan.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InListValueState_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);
            writer.WriteListHead();

            writer.WriteInteger(value);

            writer.WriteListTail();
            writer.Flush();

            Assert.Equal(bencode, buffer.WrittenSpan.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_InDictionaryValueState_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = $"d1:a{bencodeString}e".ToUtf8();
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);
            writer.WriteDictionaryHead();
            writer.WriteKey("a".ToUtf8());

            writer.WriteInteger(value);

            writer.WriteDictionaryTail();
            writer.Flush();

            Assert.Equal(bencode, buffer.WrittenSpan.ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_OversizedBuffer_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var buffer = new FixedLengthBufferWriter(bencode.Length + 1);
            var writer = new BencodeWriter(buffer);

            writer.WriteInteger(value);
            writer.Flush();

            Assert.Equal(bencode, buffer.WrittenSpan.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteInteger_UndersizedBuffer_ThrowsInvalidOperationException(string bencodeString, long value)
        {
            byte[] bencode = bencodeString.ToUtf8();
            for (int i = 1; i < bencode.Length; ++i)
            {
                var buffer = new FixedLengthBufferWriter(bencode.Length - i);
                var writer = new BencodeWriter(buffer);

                var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteInteger(value));

                Assert.Equal("Reached the end of the destination buffer while attempting to write.", ex.Message);
            }
        }
    }
}
