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
    public static class UnsupportedBencodeExceptionTests
    {
        private const string _defaultExceptionMessage = "Exception of type 'ItsyBitsy.Bencoding.UnsupportedBencodeException' was thrown.";

        [Fact]
        public static void Message_AfterDefaultConstructor_IsDefaultExceptionMessage()
        {
            var ex = new UnsupportedBencodeException();

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Fact]
        public static void Message_AfterConstructorWithNullMessage_IsDefaultExceptionMessage()
        {
            var ex = new UnsupportedBencodeException(null);

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterConstructorWithMessage_HasSpecifiedValue(string message)
        {
            var ex = new UnsupportedBencodeException(message);

            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public static void Message_AfterConstructorWithNullMessageAndPosition_IsDefaultExceptionMessage()
        {
            var ex = new UnsupportedBencodeException(null, 0);

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterConstructorWithMessageAndPosition_HasSpecifiedValue(string message)
        {
            var ex = new UnsupportedBencodeException(message, 0);

            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public static void Message_AfterConstructorWithNullMessageAndInnerException_IsDefaultExceptionMessage()
        {
            var ex = new UnsupportedBencodeException(null, null);

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterConstructorWithMessageAndInnerException_HasSpecifiedValue(string message)
        {
            var ex = new UnsupportedBencodeException(message, null);

            Assert.Equal(message, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterSerializationRoundTrip_RetainsValue(string message)
        {
            var ex = new UnsupportedBencodeException(message);

            using (var memoryStream = new MemoryStream())
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, ex);
                memoryStream.Position = 0;
                var newEx = (UnsupportedBencodeException)formatter.Deserialize(memoryStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

                Assert.Equal(newEx.Message, ex.Message);
            }
        }

        [Fact]
        public static void Position_AfterDefaultConstructor_IsNegativeOne()
        {
            var ex = new UnsupportedBencodeException();

            Assert.Equal(-1, ex.Position);
        }

        [Fact]
        public static void Position_AfterConstructorWithMessage_IsNegativeOne()
        {
            var ex = new UnsupportedBencodeException(null);

            Assert.Equal(-1, ex.Position);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public static void Position_AfterConstructorWithMessageAndPosition_HasSpecifiedValue(int position)
        {
            var ex = new UnsupportedBencodeException(null, position);

            Assert.Equal(position, ex.Position);
        }

        [Fact]
        public static void Position_AfterConstructorWithMessageAndInnerException_IsNegativeOne()
        {
            var ex = new UnsupportedBencodeException(null, null);

            Assert.Equal(-1, ex.Position);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public static void Position_AfterSerializationRoundTrip_RetainsValue(int position)
        {
            var ex = new UnsupportedBencodeException(null, position);

            using (var memoryStream = new MemoryStream())
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, ex);
                memoryStream.Position = 0;
                var newEx = (UnsupportedBencodeException)formatter.Deserialize(memoryStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

                Assert.Equal(newEx.Position, ex.Position);
            }
        }
    }
}
