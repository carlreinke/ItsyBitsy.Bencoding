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
    public partial class BencodeReaderTests
    {
        [Fact]
        public static void Position_NewInstance_IsZero()
        {
            var reader = new BencodeReader("i1e".ToUtf8());

            Assert.Equal(0, reader.Position);
        }

        [Fact]
        public static void Position_NegativeValue_ThrowsArgumentOutOfRangeException()
        {
            byte[] bencode = "i1e".ToUtf8();
            var reader = new BencodeReader(bencode);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => reader.Position = -1);

            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.Position_DataAndPosition), MemberType = typeof(BencodeTestData))]
        public static void Position_PositiveValue_ReturnsValueSet(string bencodeString, int position)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            reader.Position = position;

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.Position_DataAndPositionAndTokens), MemberType = typeof(BencodeTestData))]
        public static void Position_ValidValue_AffectsTokenType(string bencodeString, int position, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            reader.Position = position;

            Expect(reader, tokenTypes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Fact]
        public static void CreateSpanWriter_Always_GoesToErrorState()
        {
            byte[] bencode = "le".ToUtf8();
            var reader = new BencodeReader(bencode);
            var spanReader = reader.CreateSpanReader();

            var ex = Assert.Throws<InvalidOperationException>(() => reader.ReadListHead());

            Assert.Equal("The reader is not in a state that allows a list head to be read.", ex.Message);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.CreateSpan_DataAndTokens), MemberType = typeof(BencodeTestData))]
        public static void CreateSpanReader_Always_CanReadPartsOfValue(string bencodeString, BTT[] tokenTypes1, BTT[] tokenTypes2, BTT[] tokenTypes3)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            Expect(reader, tokenTypes1);

            var spanReader = reader.CreateSpanReader();
            BencodeSpanReaderTests.Expect(ref spanReader, tokenTypes2);
            spanReader.Dispose();

            Expect(reader, tokenTypes3);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        private delegate void BencodeSpanWriterAction(ref BencodeSpanWriter writer);

        [System.Diagnostics.DebuggerStepThrough]
        private static T AssertThrows<T>(ref BencodeSpanWriter writer, BencodeSpanWriterAction action)
            where T : Exception
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action(ref writer);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(T))
                    return (T)ex;

                throw new Xunit.Sdk.ThrowsException(typeof(T), ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            throw new Xunit.Sdk.ThrowsException(typeof(T));
        }

        protected static void Expect(BencodeReader reader, ReadOnlySpan<BTT> tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                Assert.Equal(tokenType, reader.ReadTokenType());

                switch (tokenType)
                {
                    case BTT.None:
                        break;
                    case BTT.Integer:
                        _ = reader.ReadInteger();
                        break;
                    case BTT.String:
                        _ = reader.ReadString();
                        break;
                    case BTT.ListHead:
                        reader.ReadListHead();
                        break;
                    case BTT.ListTail:
                        reader.ReadListTail();
                        break;
                    case BTT.DictionaryHead:
                        reader.ReadDictionaryHead();
                        break;
                    case BTT.DictionaryTail:
                        reader.ReadDictionaryTail();
                        break;
                    case BTT.Key:
                        _ = reader.ReadKey();
                        break;
                }
            }
        }
    }
}
