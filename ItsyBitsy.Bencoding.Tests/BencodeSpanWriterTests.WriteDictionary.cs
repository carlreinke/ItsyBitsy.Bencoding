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
        public static void WriteDictionary_InInitialState_GoesToFinalState()
        {
            byte[] bencode = $"de".ToUtf8();
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);

            writer.WriteDictionaryHead();
            writer.WriteDictionaryTail();
            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteInteger(0));

            Assert.Equal("The writer is not in a state that allows a value to be written.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadList_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteDictionary_InListValueState_GoesToListValueState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            string listBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] listBencode = $"lde{listBodyBencodeString}e".ToUtf8();
            byte[] buffer = new byte[listBencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadListHead();
            writer.WriteListHead();

            writer.WriteDictionaryHead();
            writer.WriteDictionaryTail();
            Copy(ref reader, ref writer, tokenTypes);

            reader.ReadListTail();
            writer.WriteListTail();

            Assert.Equal(listBencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteDictionary_InDictionaryValueState_GoesToDictionaryKeyState(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            string dictionaryBodyBencodeString = bencodeString.Substring(1, bencodeString.Length - 2);
            byte[] dictionaryBencode = $"d1:!de{dictionaryBodyBencodeString}e".ToUtf8();
            byte[] buffer = new byte[dictionaryBencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();
            writer.WriteKey("!".ToUtf8());

            writer.WriteDictionaryHead();
            writer.WriteDictionaryTail();
            Copy(ref reader, ref writer, tokenTypes);

            reader.ReadDictionaryTail();
            writer.WriteDictionaryTail();

            Assert.Equal(dictionaryBencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void WriteDictionaryHead_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            Copy(ref reader, ref writer, tokenTypes);

            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteDictionaryHead());

            Assert.Equal("The writer is not in a state that allows a dictionary head to be written.", ex.Message);
        }

        [Fact]
        public static void WriteDictionaryHead_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] buffer = Array.Empty<byte>();
            var writer = new BencodeSpanWriter(buffer);
            _ = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteInteger(1));

            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteDictionaryHead());

            Assert.Equal("The writer is not in a state that allows a dictionary head to be written.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InFinalState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void WriteDictionaryTail_InInvalidState_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            Copy(ref reader, ref writer, tokenTypes);

            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteDictionaryTail());

            Assert.Equal("The writer is not in a state that allows a dictionary tail to be written.", ex.Message);
        }

        [Fact]
        public static void WriteDictionaryTail_InErrorState_ThrowsInvalidOperationException()
        {
            byte[] buffer = new byte[1];
            var writer = new BencodeSpanWriter(buffer);
            writer.WriteDictionaryHead();
            _ = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteKey("a".ToUtf8()));

            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteDictionaryTail());

            Assert.Equal("The writer is not in a state that allows a dictionary tail to be written.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteDictionary_InInitialState_WritesExpectedData(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer);

            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();

            Copy(ref reader, ref writer, tokenTypes);

            reader.ReadDictionaryTail();
            writer.WriteDictionaryTail();

            Assert.Equal(bencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteDictionary_InListValueState_WritesExpectedData(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] listBencode = $"l{bencodeString}e".ToUtf8();
            byte[] buffer = new byte[listBencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            writer.WriteListHead();

            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();

            Copy(ref reader, ref writer, tokenTypes);

            reader.ReadDictionaryTail();
            writer.WriteDictionaryTail();

            writer.WriteListTail();

            Assert.Equal(listBencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadDictionary_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public static void WriteDictionary_InDictionaryValueState_WritesExpectedData(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] dictionaryBencode = $"d1:a{bencodeString}e".ToUtf8();
            byte[] buffer = new byte[dictionaryBencode.Length];
            var writer = new BencodeSpanWriter(buffer);
            writer.WriteDictionaryHead();
            writer.WriteKey("a".ToUtf8());

            reader.ReadDictionaryHead();
            writer.WriteDictionaryHead();

            Copy(ref reader, ref writer, tokenTypes);

            reader.ReadDictionaryTail();
            writer.WriteDictionaryTail();

            writer.WriteDictionaryTail();

            Assert.Equal(dictionaryBencode, buffer.AsSpan(0, writer.BufferedLength).ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Fact]
        public static void WriteDictionaryHead_ExactSizedBuffer_LengthIncreasesByOne()
        {
            byte[] buffer = new byte[1];
            var writer = new BencodeSpanWriter(buffer);
            int length = writer.BufferedLength;

            writer.WriteDictionaryHead();

            Assert.Equal(length + 1, writer.BufferedLength);
        }

        [Fact]
        public static void WriteDictionaryHead_OversizedBuffer_LengthIncreasesByOne()
        {
            byte[] buffer = new byte[2];
            var writer = new BencodeSpanWriter(buffer);
            int length = writer.BufferedLength;

            writer.WriteDictionaryHead();

            Assert.Equal(length + 1, writer.BufferedLength);
        }

        [Fact]
        public static void WriteDictionaryHead_UndersizedBuffer_ThrowsInvalidOperationException()
        {
            byte[] buffer = new byte[0];
            var writer = new BencodeSpanWriter(buffer);

            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteDictionaryHead());

            Assert.Equal("Reached the end of the destination buffer while attempting to write.", ex.Message);
        }

        [Fact]
        public static void WriteDictionaryTail_ExactSizedBuffer_LengthIncreasesByOne()
        {
            byte[] buffer = new byte[2];
            var writer = new BencodeSpanWriter(buffer);
            writer.WriteDictionaryHead();
            int length = writer.BufferedLength;

            writer.WriteDictionaryTail();

            Assert.Equal(length + 1, writer.BufferedLength);
        }

        [Fact]
        public static void WriteDictionaryTail_OversizedBuffer_LengthIncreasesByOne()
        {
            byte[] buffer = new byte[3];
            var writer = new BencodeSpanWriter(buffer);
            writer.WriteDictionaryHead();
            int length = writer.BufferedLength;

            writer.WriteDictionaryTail();

            Assert.Equal(length + 1, writer.BufferedLength);
        }

        [Fact]
        public static void WriteDictionaryTail_UndersizedBuffer_ThrowsInvalidOperationException()
        {
            byte[] buffer = new byte[1];
            var writer = new BencodeSpanWriter(buffer);
            writer.WriteDictionaryHead();

            var ex = AssertThrows<InvalidOperationException>(ref writer, (ref BencodeSpanWriter w) => w.WriteDictionaryTail());

            Assert.Equal("Reached the end of the destination buffer while attempting to write.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.KeyOrderValid_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void WriteKey_ValidationDisabledValidKeyOrder_DoesNotThrow(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer, skipValidation: true);

            Copy(ref reader, ref writer, tokenTypes);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.KeyOrderValid_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void WriteKey_ValidationEnabledValidKeyOrder_DoesNotThrow(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer, skipValidation: false);

            Copy(ref reader, ref writer, tokenTypes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.KeyOrderInvalid_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void WriteKey_ValidationDisabledInvalidKeyOrder_DoesNotThrow(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer, skipValidation: true);

            Copy(ref reader, ref writer, tokenTypes);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.KeyOrderInvalid_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void WriteKey_ValidationEnabledInvalidKeyOrder_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);
            byte[] buffer = new byte[bencode.Length];
            var writer = new BencodeSpanWriter(buffer, skipValidation: false);

            var ex = AssertThrows<InvalidOperationException>(ref reader, ref writer, (ref BencodeSpanReader r, ref BencodeSpanWriter w) => Copy(ref r, ref w, tokenTypes));

            Assert.Equal("Keys must be ordered and unique.", ex.Message);
        }
    }
}
