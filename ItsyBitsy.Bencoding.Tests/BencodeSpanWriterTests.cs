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
    public static partial class BencodeSpanWriterTests
    {
        [Fact]
        public static void Length_DefaultInstance_IsZero()
        {
            BencodeSpanWriter writer = default;

            Assert.Equal(0, writer.Length);
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

        private static void Copy(ref BencodeSpanReader reader, ref BencodeSpanWriter writer, ReadOnlySpan<BTT> tokenTypes)
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
                        writer.WriteString(reader.ReadString());
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
                        writer.WriteKey(reader.ReadKey());
                        break;
                }
            }
        }
    }
}
