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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace ItsyBitsy.Bencoding.Tests
{
    public static class InvalidBencodeExceptionTests
    {
        private const string _defaultExceptionMessage = "Exception of type 'ItsyBitsy.Bencoding.InvalidBencodeException' was thrown.";

        [Fact]
        public static void Message_AfterDefaultConstructor_IsDefaultExceptionMessage()
        {
            var ex = new InvalidBencodeException();

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Fact]
        public static void Message_AfterConstructorWithNullMessage_IsDefaultExceptionMessage()
        {
            var ex = new InvalidBencodeException(null);

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterConstructorWithMessage_HasSpecifiedValue(string message)
        {
            var ex = new InvalidBencodeException(message);

            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public static void Message_AfterConstructorWithNullMessageAndPosition_IsDefaultExceptionMessage()
        {
            var ex = new InvalidBencodeException(null, 0);

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterConstructorWithMessageAndPosition_HasSpecifiedValue(string message)
        {
            var ex = new InvalidBencodeException(message, 0);

            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public static void Message_AfterConstructorWithNullMessageAndInnerException_IsDefaultExceptionMessage()
        {
            var ex = new InvalidBencodeException(null, null);

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterConstructorWithMessageAndInnerException_HasSpecifiedValue(string message)
        {
            var ex = new InvalidBencodeException(message, null);

            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public static void Position_AfterDefaultConstructor_IsNegativeOne()
        {
            var ex = new InvalidBencodeException();

            Assert.Equal(-1, ex.Position);
        }

        [Fact]
        public static void Position_AfterConstructorWithMessage_IsNegativeOne()
        {
            var ex = new InvalidBencodeException(null);

            Assert.Equal(-1, ex.Position);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public static void Position_AfterConstructorWithMessageAndPosition_HasSpecifiedValue(long position)
        {
            var ex = new InvalidBencodeException(null, position);

            Assert.Equal(position, ex.Position);
        }

        [Fact]
        public static void Position_AfterConstructorWithMessageAndInnerException_IsNegativeOne()
        {
            var ex = new InvalidBencodeException(null, null);

            Assert.Equal(-1, ex.Position);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public static void Position_AfterSerializationRoundTrip_RetainsValue(long position)
        {
            var ex = new InvalidBencodeException(null, position);

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, ex);
                memoryStream.Position = 0;
                var newEx = (InvalidBencodeException)formatter.Deserialize(memoryStream);

                Assert.Equal(newEx.Position, ex.Position);
            }
        }
    }
}
