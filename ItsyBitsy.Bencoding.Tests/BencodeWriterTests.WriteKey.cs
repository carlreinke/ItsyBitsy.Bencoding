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
    public partial class BencodeWriterTests
    {
        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void WriteKey_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);
            Copy(reader, writer, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteKey("a".ToUtf8()));

            Assert.Equal("The writer is not in a state that allows a key to be written.", ex.Message);
        }

        [Fact]
        public static void WriteKey_InErrorState_ThrowsInvalidOperationException()
        {
            var buffer = new FixedLengthBufferWriter(1);
            var writer = new BencodeWriter(buffer);
            writer.WriteDictionaryHead();
            _ = Assert.Throws<ArgumentException>(() => writer.WriteDictionaryTail());

            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteKey("a".ToUtf8()));

            Assert.Equal("The writer is not in a state that allows a key to be written.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteKey_InDictionaryKeyState_WritesExpectedData(string bencodeString, string key)
        {
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);
            writer.WriteDictionaryHead();

            writer.WriteKey(key.ToUtf8());

            writer.WriteInteger(1);
            writer.WriteDictionaryTail();
            writer.Flush();

            Assert.Equal(bencode, buffer.WrittenSpan.ToArray());
        }

        [Fact]
        public static void WriteKey_ShortAndLongKey_WritesExpectedData()
        {
            string key1 = "";
            string key2 = new string('a', 1000);
            byte[] bencode = $"d{key1.Length}:{key1}i1e{key2.Length}:{key2}i2ee".ToUtf8();
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);
            writer.WriteDictionaryHead();

            writer.WriteKey(key1.ToUtf8());
            writer.WriteInteger(1);

            writer.WriteKey(key2.ToUtf8());
            writer.WriteInteger(2);

            writer.WriteDictionaryTail();
            writer.Flush();

            Assert.Equal(bencode, buffer.WrittenSpan.ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteKey_OversizedBuffer_WritesExpectedData(string bencodeString, string key)
        {
            byte[] bencode = $"d{bencodeString}".ToUtf8();
            var buffer = new FixedLengthBufferWriter(bencode.Length + 1);
            var writer = new BencodeWriter(buffer);
            writer.WriteDictionaryHead();

            writer.WriteKey(key.ToUtf8());
            writer.Flush(final: false);

            Assert.Equal(bencode, buffer.WrittenSpan.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteKey_UndersizedBuffer_ThrowsArgumentException(string bencodeString, string key)
        {
            byte[] bencode = $"d{bencodeString}".ToUtf8();
            for (int i = 1; i < bencode.Length - 1; ++i)
            {
                var buffer = new FixedLengthBufferWriter(bencode.Length - i);
                var writer = new BencodeWriter(buffer);
                writer.WriteDictionaryHead();

                var ex = Assert.Throws<ArgumentException>(() => writer.WriteKey(key.ToUtf8()));

                Assert.Equal("Reached the end of the destination buffer while attempting to write.", ex.Message);
            }
        }
    }
}
