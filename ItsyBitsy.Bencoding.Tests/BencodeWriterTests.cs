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
        public static void Constructor_DestinationIsNull_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new BencodeWriter(null!));

            Assert.Equal("destination", ex.ParamName);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Fact]
        public static void BufferedLength_NewInstance_IsZero()
        {
            var buffer = new FixedLengthBufferWriter(5);
            var writer = new BencodeWriter(buffer);

            Assert.Equal(0, writer.BufferedLength);
        }

        [Fact]
        public static void BufferedLength_WithBufferWriterAfterFlush_IsZero()
        {
            var buffer = new FixedLengthBufferWriter(5);
            var writer = new BencodeWriter(buffer);

            writer.WriteInteger(1);
            int bufferedLengthBeforeFlush = writer.BufferedLength;
            writer.Flush();

            Assert.NotEqual(0, bufferedLengthBeforeFlush);
            Assert.Equal(0, writer.BufferedLength);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Fact]
        public static void CreateSpanWriter_Always_GoesToErrorState()
        {
            var buffer = new FixedLengthBufferWriter(2);
            var writer = new BencodeWriter(buffer);
            var spanWriter = writer.CreateSpanWriter();

            var ex = Assert.Throws<InvalidOperationException>(() => writer.WriteListHead());

            Assert.Equal("The writer is not in a state that allows a list head to be written.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.CreateSpan_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void CreateSpanWriter_Always_CanWritePartsOfValue(string bencodeString, BTT[] tokenTypes1, BTT[] tokenTypes2, BTT[] tokenTypes3)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);
            Copy(reader, writer, tokenTypes1);

            var spanWriter = writer.CreateSpanWriter();
            Copy(reader, ref spanWriter, tokenTypes2);
            spanWriter.Dispose();

            Copy(reader, writer, tokenTypes3);
            writer.Flush();

            Assert.Equal(bencode, buffer.WrittenSpan.ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.CompleteValue_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void Flush_CompleteValueNotFinal_DoesNotThrow(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);

            Copy(reader, writer, tokenTypes);
            writer.Flush(final: true);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.CompleteValue_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void Flush_CompleteValueFinal_DoesNotThrow(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);

            Copy(reader, writer, tokenTypes);
            writer.Flush(final: true);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.IncompleteValue_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void Flush_IncompleteValueNotFinal_DoesNotThrow(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);

            Copy(reader, writer, tokenTypes);
            writer.Flush(final: false);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.IncompleteValue_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void Flush_IncompleteValueFinal_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var buffer = new FixedLengthBufferWriter(bencode.Length);
            var writer = new BencodeWriter(buffer);

            Copy(reader, writer, tokenTypes);
            var ex = Assert.Throws<InvalidOperationException>(() => writer.Flush(final: true));

            Assert.Equal("The value is incomplete.", ex.Message);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        internal static void Copy(BencodeReader reader, BencodeWriter writer, ReadOnlySpan<BTT> tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                Assert.Equal(tokenType, reader.ReadTokenType());

                switch (tokenType)
                {
                    case BTT.None:
                        break;
                    case BTT.Integer:
                        writer.WriteInteger(reader.ReadInteger());
                        break;
                    case BTT.String:
                        writer.WriteString(reader.ReadString().Span);
                        break;
                    case BTT.ListHead:
                        reader.ReadListHead();
                        writer.WriteListHead();
                        break;
                    case BTT.ListTail:
                        reader.ReadListTail();
                        writer.WriteListTail();
                        break;
                    case BTT.DictionaryHead:
                        reader.ReadDictionaryHead();
                        writer.WriteDictionaryHead();
                        break;
                    case BTT.DictionaryTail:
                        reader.ReadDictionaryTail();
                        writer.WriteDictionaryTail();
                        break;
                    case BTT.Key:
                        writer.WriteKey(reader.ReadKey().Span);
                        break;
                }
            }
        }

        internal static void Copy(BencodeReader reader, ref BencodeSpanWriter writer, ReadOnlySpan<BTT> tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                Assert.Equal(tokenType, reader.ReadTokenType());

                switch (tokenType)
                {
                    case BTT.None:
                        break;
                    case BTT.Integer:
                        writer.WriteInteger(reader.ReadInteger());
                        break;
                    case BTT.String:
                        writer.WriteString(reader.ReadString().Span);
                        break;
                    case BTT.ListHead:
                        reader.ReadListHead();
                        writer.WriteListHead();
                        break;
                    case BTT.ListTail:
                        reader.ReadListTail();
                        writer.WriteListTail();
                        break;
                    case BTT.DictionaryHead:
                        reader.ReadDictionaryHead();
                        writer.WriteDictionaryHead();
                        break;
                    case BTT.DictionaryTail:
                        reader.ReadDictionaryTail();
                        writer.WriteDictionaryTail();
                        break;
                    case BTT.Key:
                        writer.WriteKey(reader.ReadKey().Span);
                        break;
                }
            }
        }
    }
}
