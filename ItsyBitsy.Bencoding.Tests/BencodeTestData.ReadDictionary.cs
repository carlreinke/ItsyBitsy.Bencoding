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
using System.Collections.Generic;
using System.Linq;
using BTT = ItsyBitsy.Bencoding.BencodeTokenType;

namespace ItsyBitsy.Bencoding.Tests
{
    public static partial class BencodeTestData
    {
        public static readonly (string bencodeString, BTT[])[] ReadDictionary_ValidData_DataAndValue = new[]
        {
            ( "de",                       new BTT[0] ),
            ( "d1:1i1ee",                 new[] { BTT.Key, BTT.Integer } ),
            ( "d1:11:ae",                 new[] { BTT.Key, BTT.String } ),
            ( "d1:1lee",                  new[] { BTT.Key, BTT.ListHead, BTT.ListTail } ),
            ( "d1:1dee",                  new[] { BTT.Key, BTT.DictionaryHead, BTT.DictionaryTail } ),
            ( "d1:1i1e1:21:a1:3le1:4dee", new[] { BTT.Key, BTT.Integer, BTT.Key, BTT.String, BTT.Key, BTT.ListHead, BTT.ListTail, BTT.Key, BTT.DictionaryHead, BTT.DictionaryTail } ),
            ( "d1:11:a1:2le1:3de1:4i1ee", new[] { BTT.Key, BTT.String, BTT.Key, BTT.ListHead, BTT.ListTail, BTT.Key, BTT.DictionaryHead, BTT.DictionaryTail, BTT.Key, BTT.Integer } ),
            ( "d1:1le1:2de1:3i1e1:41:ae", new[] { BTT.Key, BTT.ListHead, BTT.ListTail, BTT.Key, BTT.DictionaryHead, BTT.DictionaryTail, BTT.Key, BTT.Integer, BTT.Key, BTT.String } ),
            ( "d1:1de1:2i1e1:31:a1:4lee", new[] { BTT.Key, BTT.DictionaryHead, BTT.DictionaryTail, BTT.Key, BTT.Integer, BTT.Key, BTT.String, BTT.Key, BTT.ListHead, BTT.ListTail } ),
        };

        public static IEnumerable<ValueTuple<string>> ReadDictionary_ValidData_Data => ReadDictionary_ValidData_DataAndValue
            .Select(x => ValueTuple.Create(x.bencodeString));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadDictionaryHead_MissingData_DataAndError = new[]
        {
            // Missing head byte.
            ("", "Expected 'd' but reached the end of the source buffer while reading a dictionary head.", 0),
        };

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadDictionaryHead_InvalidData_DataAndError = new[]
        {
            // Invalid head byte.
            ("ede", "Expected 'd' but found 'e' while reading a dictionary head.", 0),
            ("ide", "Expected 'd' but found 'i' while reading a dictionary head.", 0),
            ("lde", "Expected 'd' but found 'l' while reading a dictionary head.", 0),
            ("1de", "Expected 'd' but found '1' while reading a dictionary head.", 0),
            (":de", "Expected 'd' but found ':' while reading a dictionary head.", 0),
            ("-de", "Expected 'd' but found '-' while reading a dictionary head.", 0),
            ("\0de", "Expected 'd' but found '\\x00' while reading a dictionary head.", 0),
        };

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadDictionaryTail_MissingData_DataAndError = new[]
        {
            // Missing tail byte.
            ("d", "Expected 'e' but reached the end of the source buffer while reading a dictionary tail.", 1),
        };

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadDictionaryTail_InvalidData_DataAndError = new[]
        {
            // Invalid tail byte.
            ("dd", "Expected 'e' but found 'd' while reading a dictionary tail.", 1),
            ("di", "Expected 'e' but found 'i' while reading a dictionary tail.", 1),
            ("dl", "Expected 'e' but found 'l' while reading a dictionary tail.", 1),
            ("d1", "Expected 'e' but found '1' while reading a dictionary tail.", 1),
            ("d:", "Expected 'e' but found ':' while reading a dictionary tail.", 1),
            ("d-", "Expected 'e' but found '-' while reading a dictionary tail.", 1),
            ("d\0", "Expected 'e' but found '\\x00' while reading a dictionary tail.", 1),
        };
    }
}
