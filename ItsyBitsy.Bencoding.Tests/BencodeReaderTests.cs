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
    public partial class BencodeReaderTests : BencodeReaderTestsBase
    {
        [Fact]
        public void CanSeek_Always_ReturnsTrue()
        {
            var reader = new BencodeReader("i1e".ToUtf8());

            bool canSeek = reader.CanSeek;

            Assert.True(canSeek);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Fact]
        public void NonInterfacePosition_NewInstance_IsZero()
        {
            var reader = new BencodeReader("i1e".ToUtf8());

            int position = reader.Position;

            Assert.Equal(0, position);
        }

        [Fact]
        public void NonInterfacePosition_NegativeValue_ThrowsArgumentOutOfRangeException()
        {
            byte[] bencode = "i1e".ToUtf8();
            var reader = new BencodeReader(bencode);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => reader.Position = -1);

            Assert.Equal("value", ex.ParamName);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.Position_DataAndPosition), MemberType = typeof(BencodeTestData))]
        public void NonInterfacePosition_PositiveValue_ReturnsValueSet(string bencodeString, int position)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            reader.Position = position;

            Assert.Equal(position, reader.Position);
        }

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.Position_DataAndPositionAndTokens), MemberType = typeof(BencodeTestData))]
        public void NonInterfacePosition_ValidValue_AffectsTokenType(string bencodeString, int position, BTT[] tokenTypes)
        {
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            reader.Position = position;

            Expect(reader, tokenTypes);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void NonInterfaceReadString_ValidData_ReturnsValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = bencodeString.ToUtf8();
            var reader = new BencodeReader(bencode);

            var value = reader.ReadString();

            Assert.Equal(expectedValue, value.ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        [Theory]
        [TupleMemberData(nameof(BencodeTestData.ReadString_ValidData_DataAndValue), MemberType = typeof(BencodeTestData))]
        public void NonInterfaceReadKey_ValidData_ReturnsValue(string bencodeString, string expectedValueString)
        {
            byte[] expectedValue = expectedValueString.ToUtf8();
            byte[] bencode = $"d{bencodeString}i1ee".ToUtf8();
            var reader = new BencodeReader(bencode);
            reader.ReadDictionaryHead();

            var value = reader.ReadKey();

            Assert.Equal(expectedValue, value.ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        protected override IBencodeReader CreateReader(byte[] bencode)
        {
            return new BencodeReader(bencode);
        }
    }
}
