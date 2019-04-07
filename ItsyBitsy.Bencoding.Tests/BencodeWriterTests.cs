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
    public partial class BencodeWriterTests : BencodeWriterTestsBase
    {
        private BencodeWriter _writer = new BencodeWriter();

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes)[] CompleteValue_DataAndTokens = new[]
        {
            ("i1e", new BTT[] { BTT.Integer }),
            ("1:a", new BTT[] { BTT.String }),
            ("le", new BTT[] { BTT.ListHead, BTT.ListTail }),
            ("de", new BTT[] { BTT.DictionaryHead, BTT.DictionaryTail }),
        };

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Fact]
        public static void Constructor_UnspecifiedCapacity_InitialCapacityIsZero()
        {
            var writer = new BencodeWriter();

            Assert.Equal(0, writer.Capacity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public static void Constructor_ValidCapacity_InitialCapacityEqualToSpecified(int capacity)
        {
            var writer = new BencodeWriter(capacity);

            Assert.Equal(capacity, writer.Capacity);
        }

        [Fact]
        public static void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _ = new BencodeWriter(-1));

            Assert.Equal("capacity", ex.ParamName);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes)[] Clear_DataAndTokens = new[]
        {
            ("", new BTT[0]),
            ("i0e", new BTT[] { BTT.Integer }),
            ("1:a", new BTT[] { BTT.String }),
            ("l", new BTT[] { BTT.ListHead }),
            ("le", new BTT[] { BTT.ListHead, BTT.ListTail }),
            ("d", new BTT[] { BTT.DictionaryHead }),
            ("de", new BTT[] { BTT.DictionaryHead, BTT.DictionaryTail }),
            ("d1:a", new BTT[] { BTT.DictionaryHead, BTT.Key }),
        };

        [Theory]
        [TupleMemberData(nameof(Clear_DataAndTokens))]
        public static void Clear_AfterWrites_LengthIsZero(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            writer.Clear();

            Assert.Equal(0, writer.Length);
        }

        [Theory]
        [TupleMemberData(nameof(Clear_DataAndTokens))]
        public static void Clear_AfterWrites_CanWriteNewValue(string bencodeString, BTT[] tokenTypes)
        {
            foreach (var dataAndTokens in CompleteValue_DataAndTokens)
            {
                byte[] bencode = bencodeString.ToUtf8();
                var reader = new BencodeReader(bencode);
                var writer = new BencodeWriter();
                Copy(reader, writer, tokenTypes);

                writer.Clear();

                byte[] bencode2 = dataAndTokens.bencodeString.ToUtf8();
                var reader2 = new BencodeReader(bencode2);
                Copy(reader2, writer, dataAndTokens.tokenTypes);

                byte[] buffer = writer.Encode();
                Assert.Equal(bencode2, buffer);
            }
        }

        [Theory]
        [InlineData("d1:ai1e1:bi1ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.Key, BTT.Integer, BTT.DictionaryTail })]
        [InlineData("d1:ad1:bi1eee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.DictionaryTail, BTT.DictionaryTail })]
        public static void Clear_AfterWriteDictionary_CanWriteNewDictionaryWithSameKeys(string bencodeString, BTT[] tokenTypes)
        {
            for (int i = tokenTypes.Length; i > 0; --i)
            {
                byte[] bencode = bencodeString.ToUtf8();
                var reader = new BencodeReader(bencode);
                var writer = new BencodeWriter();
                Copy(reader, writer, tokenTypes.AsSpan(0, i));

                writer.Clear();

                reader.Position = 0;
                Copy(reader, writer, tokenTypes);

                byte[] buffer = writer.Encode();
                Assert.Equal(bencode, buffer);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void Encode_IncompleteValue_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => writer.Encode());

            Assert.Equal("The value is incomplete.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void EncodeTo_IncompleteValue_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => writer.EncodeTo(new byte[bencode.Length]));

            Assert.Equal("The value is incomplete.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void TryEncode_IncompleteValue_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => _ = writer.TryEncodeTo(new byte[bencode.Length]));

            Assert.Equal("The value is incomplete.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.InInitialState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InListValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryValueState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        [TupleMemberData(nameof(BencodeTestData.InDictionaryKeyState_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void TransferEncoded_IncompleteValue_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            var ex = Assert.Throws<InvalidOperationException>(() => _ = writer.TransferEncoded());

            Assert.Equal("The value is incomplete.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void Encode_ValidData_ReturnsExpectedData(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            byte[] buffer = writer.Encode();

            Assert.Equal(bencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void EncodeTo_ExactSizeBuffer_WritesExpectedDataAndReturnsExpectedLength(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            int length = writer.Length;
            byte[] buffer = new byte[length];
            writer.EncodeTo(buffer);

            Assert.Equal(bencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void EncodeTo_OversizedBuffer_WritesExpectedDataAndReturnsExpectedLength(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            int length = writer.Length;
            byte[] buffer = new byte[length + 1];
            writer.EncodeTo(buffer);

            Assert.Equal(bencode, buffer.AsSpan(0, length).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void EncodeTo_UndersizedBuffer_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            byte[] buffer = new byte[writer.Length - 1];
            var ex = Assert.Throws<ArgumentException>(() => writer.EncodeTo(buffer));

            Assert.StartsWith("Destination is too short.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void TryEncodeTo_ExactSizeBuffer_WritesExpectedDataAndReturnsExpectedLength(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            int length = writer.Length;
            byte[] buffer = new byte[length];
            bool result = writer.TryEncodeTo(buffer);

            Assert.True(result);
            Assert.Equal(bencode, buffer);
        }

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void TryEncodeTo_OversizedBuffer_WritesExpectedDataAndReturnsExpectedLength(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            int length = writer.Length;
            byte[] buffer = new byte[length + 1];
            bool result = writer.TryEncodeTo(buffer);

            Assert.True(result);
            Assert.Equal(bencode, buffer.AsSpan(0, length).ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void TryEncodeTo_UndersizedBuffer_ReturnsFalseAndZero(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            int length = writer.Length;
            byte[] buffer = new byte[length - 1];
            bool result = writer.TryEncodeTo(buffer);

            Assert.False(result);
        }

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void TransferEncoded_ValidData_ReturnsExpectedData(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            ReadOnlyMemory<byte> buffer = writer.TransferEncoded();

            Assert.Equal(bencode, buffer.ToArray());
        }

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void TransferEncoded_AfterWrites_LengthIsZero(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            _ = writer.TransferEncoded();

            Assert.Equal(0, writer.Length);
        }

        [Theory]
        [TupleMemberData(nameof(CompleteValue_DataAndTokens))]
        public static void TransferEncoded_AfterWrites_CanWriteNewValueWithoutClobbering(string bencodeString, BTT[] tokenTypes)
        {
            foreach (var dataAndTokens in CompleteValue_DataAndTokens)
            {
                byte[] bencode = bencodeString.ToUtf8();
                var reader = new BencodeReader(bencode);
                var writer = new BencodeWriter();
                Copy(reader, writer, tokenTypes);

                var buffer = writer.TransferEncoded();

                byte[] bencode2 = dataAndTokens.bencodeString.ToUtf8();
                var reader2 = new BencodeReader(bencode2);
                Copy(reader2, writer, dataAndTokens.tokenTypes);

                byte[] buffer2 = writer.Encode();
                Assert.Equal(bencode2, buffer2);

                Assert.Equal(bencode, buffer.ToArray());
            }
        }

        [Theory]
        [InlineData("d1:ai1e1:bi1ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.Key, BTT.Integer, BTT.DictionaryTail })]
        [InlineData("d1:ad1:bi1eee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.DictionaryTail, BTT.DictionaryTail })]
        public static void TransferEncoded_AfterWriteDictionary_CanWriteNewDictionaryWithSameKeys(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter();
            Copy(reader, writer, tokenTypes);

            _ = writer.TransferEncoded();

            reader.Position = 0;
            Copy(reader, writer, tokenTypes);

            byte[] buffer = writer.Encode();
            Assert.Equal(bencode, buffer);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        protected override IBencodeWriter Writer => _writer;

        protected override byte[] EncodedData => _writer.Encode();
    }
}
