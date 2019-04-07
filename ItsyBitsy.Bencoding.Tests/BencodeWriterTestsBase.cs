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
    public abstract partial class BencodeWriterTestsBase
    {
        protected abstract IBencodeWriter Writer { get; }

        protected abstract byte[] EncodedData { get; }

        protected static void Copy(IBencodeReader reader, IBencodeWriter writer, ReadOnlySpan<BTT> tokenTypes)
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
