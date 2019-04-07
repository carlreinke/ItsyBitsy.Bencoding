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
        public void WriteList_InInitialState_GoesToFinalState()
        {
            byte[] bencode = $"le".ToUtf8();
            var writer = Writer;

            writer.WriteListHead();
            writer.WriteListTail();
            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteInteger(0));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteList_InListValueState_GoesToListValueState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            string listBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] listBencode = $"lle{listBodyBencodeString}e".ToUtf8();
            var writer = Writer;
            reader.ReadListHead();
            writer.WriteListHead();

            writer.WriteListHead();
            writer.WriteListTail();
            Copy(reader, writer, tokenTypes);

            reader.ReadListTail();
            writer.WriteListTail();

            byte[] buffer = EncodedData;
            Assert.Equal(listBencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteList_InDictionaryValueState_GoesToDictionaryKeyState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            string dictionaryBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] dictionaryBencode = $"d1:!le{dictionaryBodyBencodeString}e".ToUtf8();
            var writer = Writer;
            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();
            writer.WriteKey("!".ToUtf8());

            writer.WriteListHead();
            writer.WriteListTail();
            Copy(reader, writer, tokenTypes);

            reader.ReadDictionaryTail();
            writer.WriteDictionaryTail();

            byte[] buffer = EncodedData;
            Assert.Equal(dictionaryBencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public void ReadListHead_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = Writer;
            Copy(reader, writer, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteListHead());

            Assert.Equal("The writer is not in a state that allows a list head to be written.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public void ReadListTail_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = Writer;
            Copy(reader, writer, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteListTail());

            Assert.Equal("The writer is not in a state that allows a list tail to be written.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteList_InInitialState_WritesExpectedData(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = Writer;

            reader.ReadListHead();
            writer.WriteListHead();

            Copy(reader, writer, tokenTypes);

            reader.ReadListTail();
            writer.WriteListTail();

            byte[] buffer = EncodedData;
            Assert.Equal(bencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteList_InListValueState_WritesExpectedData(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            byte[] listBencode = $"l{bencodeString}e".ToUtf8();
            var writer = Writer;
            writer.WriteListHead();

            reader.ReadListHead();
            writer.WriteListHead();

            Copy(reader, writer, tokenTypes);

            reader.ReadListTail();
            writer.WriteListTail();

            writer.WriteListTail();

            byte[] buffer = EncodedData;
            Assert.Equal(listBencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void WriteList_InDictionaryValueState_WritesExpectedData(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            byte[] dictionaryBencode = $"d1:a{bencodeString}e".ToUtf8();
            var writer = Writer;
            writer.WriteDictionaryHead();
            writer.WriteKey("a".ToUtf8());

            reader.ReadListHead();
            writer.WriteListHead();

            Copy(reader, writer, tokenTypes);

            reader.ReadListTail();
            writer.WriteListTail();

            writer.WriteDictionaryTail();

            byte[] buffer = EncodedData;
            Assert.Equal(dictionaryBencode, buffer);
        }
    }
}
