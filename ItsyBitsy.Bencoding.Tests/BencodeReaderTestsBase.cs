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
    public abstract partial class BencodeReaderTestsBase
    {
        [Fact]
        public void Position_NewInstance_IsZero()
        {
            var reader = CreateReader("i1e".ToUtf8());

            if (!reader.CanSeek)
                return;

            Assert.Equal(0, reader.Position);
        }

        [Fact]
        public void Position_NegativeValue_ThrowsArgumentOutOfRangeException()
        {
            byte[] bencode = "i1e".ToUtf8();
            var reader = CreateReader(bencode);

            if (!reader.CanSeek)
                return;

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => reader.Position = -1);

            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.Position_DataAndPosition), MemberType = typeof(BencodeTestData))]
        public void Position_PositiveValue_ReturnsValueSet(string bencodeString, int position)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = CreateReader(bencode);

            if (!reader.CanSeek)
                return;

            reader.Position = position;

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.Position_DataAndPositionAndTokens), MemberType = typeof(BencodeTestData))]
        public void Position_ValidValue_AffectsTokenType(string bencodeString, int position, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = CreateReader(bencode);

            if (!reader.CanSeek)
                return;

            reader.Position = position;

            Expect(reader, tokenTypes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        protected abstract IBencodeReader CreateReader(byte[] bencode);

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

        protected static void Expect(IBencodeReader reader, ReadOnlySpan<BTT> tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                Assert.Equal(tokenType, reader.ReadTokenType());

                switch (tokenType)
                {
                    case BTT.None:
                        break;
                    case BTT.Integer:
                        reader.ReadInteger();
                        break;
                    case BTT.String:
                        reader.ReadString();
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
                        reader.ReadKey();
                        break;
                }
            }
        }
    }
}
