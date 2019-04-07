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
        [Fact]
        public static void WriteDictionaryHead_Always_LengthIncreasesByOne()
        {
            var writer = new BencodeWriter();
            int length = writer.Length;

            writer.WriteDictionaryHead();

            Assert.Equal(length + 1, writer.Length);
        }

        [Fact]
        public static void WriteDictionaryTail_Always_LengthIncreasesByOne()
        {
            var writer = new BencodeWriter();
            writer.WriteDictionaryHead();
            int length = writer.Length;

            writer.WriteDictionaryTail();

            Assert.Equal(length + 1, writer.Length);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes)[] KeyOrderValid_DataAndTokens = new[]
        {
            ("d1:ai1e2:aai2ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.Key, BTT.Integer, BTT.DictionaryTail }),
            ("d1:ai1e1:bi2ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.Key, BTT.Integer, BTT.DictionaryTail }),
            ("d1:bd1:ai1eee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.DictionaryTail, BTT.DictionaryTail }),
            ("d1:ad1:ci1ee1:bi2ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.DictionaryTail, BTT.Key, BTT.Integer, BTT.DictionaryTail }),
        };

        [Theory]
        [TupleMemberData(nameof(KeyOrderValid_DataAndTokens))]
        public static void WriteKey_ValidationDisabledValidKeyOrder_DoesNotThrow(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter(skipValidation: true);

            Copy(reader, writer, tokenTypes);
        }

        [Theory]
        [TupleMemberData(nameof(KeyOrderValid_DataAndTokens))]
        public static void WriteKey_ValidationEnabledValidKeyOrder_DoesNotThrow(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter(skipValidation: false);

            Copy(reader, writer, tokenTypes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes)[] KeyOrderInvalid_DataAndTokens = new[]
        {
            ("d1:ai1e1:ai2ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.Key, BTT.Integer, BTT.DictionaryTail }),
            ("d2:aai1e1:ai2ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.Key, BTT.Integer, BTT.DictionaryTail }),
            ("d1:bi1e1:ai2ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.Key, BTT.Integer, BTT.DictionaryTail }),
            ("d1:cd1:ai1ee1:bi2ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.DictionaryTail, BTT.Key, BTT.Integer, BTT.DictionaryTail }),
        };

        [Theory]
        [TupleMemberData(nameof(KeyOrderInvalid_DataAndTokens))]
        public static void WriteKey_ValidationDisabledInvalidKeyOrder_DoesNotThrow(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter(skipValidation: true);

            Copy(reader, writer, tokenTypes);
        }

        [Theory]
        [TupleMemberData(nameof(KeyOrderInvalid_DataAndTokens))]
        public static void WriteKey_ValidationEnabledInvalidKeyOrder_ThrowsInvalidOperationException(string bencodeString, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);
            var writer = new BencodeWriter(skipValidation: false);

            var ex = Assert.Throws<InvalidOperationException>(() => Copy(reader, writer, tokenTypes));

            Assert.Equal("Keys must be ordered and unique.", ex.Message);
        }
    }
}
