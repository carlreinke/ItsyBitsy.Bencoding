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
    public abstract partial class BencodeWriterTestsBase
    {
        [Fact]
        public void WriteInteger_InInitialState_GoesToFinalState()
        {
            byte[] bencode = $"i1e".ToUtf8();
            var writer = Writer;

            writer.WriteInteger(1);
            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteInteger(0));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteInteger_InListValueState_GoesToListValueState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            string listBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] listBencode = $"li1e{listBodyBencodeString}e".ToUtf8();
            var writer = Writer;
            reader.ReadListHead();
            writer.WriteListHead();

            writer.WriteInteger(1);
            Copy(reader, writer, tokenTypes);

            reader.ReadListTail();
            writer.WriteListTail();

            byte[] buffer = EncodedData;
            Assert.Equal(listBencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteInteger_InDictionaryValueState_GoesToDictionaryKeyState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            string dictionaryBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] dictionaryBencode = $"d1:!i1e{dictionaryBodyBencodeString}e".ToUtf8();
            var writer = Writer;
            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();
            writer.WriteKey("!".ToUtf8());

            writer.WriteInteger(1);
            Copy(reader, writer, tokenTypes);

            reader.ReadDictionaryTail();
            writer.WriteDictionaryTail();

            byte[] buffer = EncodedData;
            Assert.Equal(dictionaryBencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public void WriteInteger_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = Writer;
            Copy(reader, writer, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteInteger(1));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteInteger_InInitialState_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var writer = Writer;

            writer.WriteInteger(value);

            byte[] buffer = EncodedData;
            Assert.Equal(bencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteInteger_InListValueState_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = $"l{bencodeString}e".ToUtf8();
            var writer = Writer;
            writer.WriteListHead();

            writer.WriteInteger(value);

            writer.WriteListTail();

            byte[] buffer = EncodedData;
            Assert.Equal(bencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadInteger_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteInteger_InDictionaryValueState_WritesExpectedData(string bencodeString, long value)
        {
            byte[] bencode = $"d1:a{bencodeString}e".ToUtf8();
            var writer = Writer;
            writer.WriteDictionaryHead();
            writer.WriteKey("a".ToUtf8());

            writer.WriteInteger(value);

            writer.WriteDictionaryTail();

            byte[] buffer = EncodedData;
            Assert.Equal(bencode, buffer);
        }
    }
}
