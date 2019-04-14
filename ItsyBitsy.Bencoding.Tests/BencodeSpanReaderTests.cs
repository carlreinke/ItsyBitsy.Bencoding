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
    public static partial class BencodeSpanReaderTests
    {
        [Fact]
        public static void Position_DefaultInstance_IsZero()
        {
            BencodeSpanReader reader = default;

            Assert.Equal(0, reader.Position);
        }

        [Fact]
        public static void Position_NegativeValue_ThrowsArgumentOutOfRangeException()
        {
            byte[] bencode = "i1e".ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            var ex = AssertThrows<ArgumentOutOfRangeException>(ref reader, (ref BencodeSpanReader r) => r.Position = -1);

            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.Position_DataAndPosition), MemberType = typeof(BencodeTestData))]
        public static void Position_ValidValue_ReturnsValueSet(string bencodeString, int position)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            reader.Position = position;

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.Position_DataAndPositionAndTokens), MemberType = typeof(BencodeTestData))]
        public static void Position_ValidValue_AffectsTokenType(string bencodeString, int position, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            reader.Position = position;

            Expect(ref reader, tokenTypes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Fact]
        public static void Dispose_ConstructedWithSpan_GoesToDisposedState()
        {
            byte[] bencode = "le".ToUtf8();
            var reader = new BencodeSpanReader(bencode);

            reader.Dispose();

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => r.ReadListHead());

            Assert.Equal("The reader is not in a state that allows a list head to be read.", ex.Message);
        }

        [Fact]
        public static void Dispose_CreatedFromBencodeReader_GoesToDisposedState()
        {
            byte[] bencode = "le".ToUtf8();
            var parentReader = new BencodeReader(bencode);
            var reader = parentReader.CreateSpanReader();

            reader.Dispose();

            var ex = AssertThrows<InvalidOperationException>(ref reader, (ref BencodeSpanReader r) => r.ReadListHead());

            Assert.Equal("The reader is not in a state that allows a list head to be read.", ex.Message);
        }

        [Fact]
        public static void Dispose_TwiceCreatedFromBencodeReader_GoesToDisposedState()
        {
            byte[] bencode = "le".ToUtf8();
            var parentReader = new BencodeReader(bencode);
            var reader = parentReader.CreateSpanReader();

            reader.Dispose();

            parentReader.ReadListHead();

            reader.Dispose();

            parentReader.ReadListTail();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        internal static void Expect(ref BencodeSpanReader reader, ReadOnlySpan<BTT> tokenTypes)
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

        private delegate void BencodeSpanReaderAction(ref BencodeSpanReader reader);

        [System.Diagnostics.DebuggerStepThrough]
        private static T AssertThrows<T>(ref BencodeSpanReader reader, BencodeSpanReaderAction action)
            where T : Exception
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action(ref reader);
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

        private delegate void BencodeSpanReaderWriterAction(ref BencodeSpanReader reader, ref BencodeSpanWriter writer);

        [System.Diagnostics.DebuggerStepThrough]
        private static T AssertThrows<T>(ref BencodeSpanReader reader, ref BencodeSpanWriter writer, BencodeSpanReaderWriterAction action)
            where T : Exception
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action(ref reader, ref writer);
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
    }
}
