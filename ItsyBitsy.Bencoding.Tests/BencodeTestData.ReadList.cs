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
        public static readonly (string bencodeString, BTT[])[] ReadList_ValidData_DataAndValue = new[]
        {
            ( "le",           new BTT[0] ),
            ( "li1ee",        new[] { BTT.Integer } ),
            ( "l1:ae",        new[] { BTT.String } ),
            ( "llee",         new[] { BTT.ListHead, BTT.ListTail } ),
            ( "ldee",         new[] { BTT.DictionaryHead, BTT.DictionaryTail } ),
            ( "li1e1:aledee", new[] { BTT.Integer, BTT.String, BTT.ListHead, BTT.ListTail, BTT.DictionaryHead, BTT.DictionaryTail } ),
            ( "l1:aledei1ee", new[] { BTT.String, BTT.ListHead, BTT.ListTail, BTT.DictionaryHead, BTT.DictionaryTail, BTT.Integer } ),
            ( "lledei1e1:ae", new[] { BTT.ListHead, BTT.ListTail, BTT.DictionaryHead, BTT.DictionaryTail, BTT.Integer, BTT.String } ),
            ( "ldei1e1:alee", new[] { BTT.DictionaryHead, BTT.DictionaryTail, BTT.Integer, BTT.String, BTT.ListHead, BTT.ListTail } ),
        };

        public static IEnumerable<ValueTuple<string>> ReadList_ValidData_Data => ReadList_ValidData_DataAndValue
            .Select(x => ValueTuple.Create(x.bencodeString));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadList_MissingData_DataAndError = new[]
        {
            // Missing head byte.
            ("", "Expected 'l' but reached the end of the source buffer while reading a list head.", 0),

            // Missing value byte.
            ("l", "Expected 'd', 'e', 'i', 'l', or '0'-'9' but reached the end of the source buffer while reading the token type.", 1),
        };

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadList_InvalidData_DataAndError = new[]
        {
            // Invalid head byte.
            ("d", "Expected 'l' but found 'd' while reading a list head.", 0),
            ("e", "Expected 'l' but found 'e' while reading a list head.", 0),
            ("i", "Expected 'l' but found 'i' while reading a list head.", 0),
            ("1", "Expected 'l' but found '1' while reading a list head.", 0),
            (":", "Expected 'l' but found ':' while reading a list head.", 0),
            ("-", "Expected 'l' but found '-' while reading a list head.", 0),
            ("\0", "Expected 'l' but found '\\x00' while reading a list head.", 0),

            // Invalid value byte.
            ("l:", "Expected 'd', 'e', 'i', 'l', or '0'-'9' but found ':' while reading the token type.", 1),
            ("l-", "Expected 'd', 'e', 'i', 'l', or '0'-'9' but found '-' while reading the token type.", 1),
            ("l\0", "Expected 'd', 'e', 'i', 'l', or '0'-'9' but found '\\x00' while reading the token type.", 1),
        };

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadListHead_MissingData_DataAndError = new[]
        {
            // Missing head byte.
            ("", "Expected 'l' but reached the end of the source buffer while reading a list head.", 0),
        };

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadListHead_InvalidData_DataAndError = new[]
        {
            // Invalid head byte.
            ("dle", "Expected 'l' but found 'd' while reading a list head.", 0),
            ("ele", "Expected 'l' but found 'e' while reading a list head.", 0),
            ("ile", "Expected 'l' but found 'i' while reading a list head.", 0),
            ("1le", "Expected 'l' but found '1' while reading a list head.", 0),
            (":le", "Expected 'l' but found ':' while reading a list head.", 0),
            ("-le", "Expected 'l' but found '-' while reading a list head.", 0),
            ("\0le", "Expected 'l' but found '\\x00' while reading a list head.", 0),
        };

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadListTail_MissingData_DataAndError = new[]
        {
            // Missing tail byte.
            ("l", "Expected 'e' but reached the end of the source buffer while reading a list tail.", 1),
        };

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadListTail_InvalidData_DataAndError = new[]
        {
            // Invalid tail byte.
            ("ld", "Expected 'e' but found 'd' while reading a list tail.", 1),
            ("li", "Expected 'e' but found 'i' while reading a list tail.", 1),
            ("ll", "Expected 'e' but found 'l' while reading a list tail.", 1),
            ("l1", "Expected 'e' but found '1' while reading a list tail.", 1),
            ("l:", "Expected 'e' but found ':' while reading a list tail.", 1),
            ("l-", "Expected 'e' but found '-' while reading a list tail.", 1),
            ("l\0", "Expected 'e' but found '\\x00' while reading a list tail.", 1),
        };
    }
}
