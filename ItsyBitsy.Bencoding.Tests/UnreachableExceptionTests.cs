//
// Copyright (C) 2021  Carl Reinke
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
    public static class UnreachableExceptionTests
    {
        private const string _defaultExceptionMessage = "The program executed an instruction that was thought to be unreachable.";

        [Fact]
        public static void Message_AfterDefaultConstructor_IsDefaultExceptionMessage()
        {
            var ex = new UnreachableException();

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Fact]
        public static void Message_AfterConstructorWithNullMessage_IsDefaultExceptionMessage()
        {
            var ex = new UnreachableException(null);

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterConstructorWithMessage_HasSpecifiedValue(string message)
        {
            var ex = new UnreachableException(message);

            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public static void Message_AfterConstructorWithNullMessageAndInnerException_IsDefaultExceptionMessage()
        {
            var ex = new UnreachableException(null, null);

            Assert.Equal(_defaultExceptionMessage, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterConstructorWithMessageAndInnerException_HasSpecifiedValue(string message)
        {
            var ex = new UnreachableException(message, null);

            Assert.Equal(message, ex.Message);
        }

        [Theory]
        [InlineData("Test")]
        public static void Message_AfterSerializationRoundTrip_RetainsValue(string message)
        {
            var ex = new UnreachableException(message);

            using (var memoryStream = new MemoryStream())
            {
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, ex);
                memoryStream.Position = 0;
                var newEx = (UnreachableException)formatter.Deserialize(memoryStream);
#pragma warning restore SYSLIB0011 // Type or member is obsolete

                Assert.Equal(newEx.Message, ex.Message);
            }
        }
    }
}
