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
using System.Collections.Generic;
using System.Linq;
using BTT = ItsyBitsy.Bencoding.BencodeTokenType;

namespace ItsyBitsy.Bencoding.Tests
{
    public static partial class BencodeTestData
    {
        public static readonly (string bencodeString, BTT[] tokenTypes, BTT expectedTokenType, int expectedPosition)[] ReadTokenType_InInitialState_DataAndTokensAndTokenAndPosition = new[]
        {
            ("i", new BTT[0], BTT.Integer, 0),
            ("0", new BTT[0], BTT.String, 0),
            ("l", new BTT[0], BTT.ListHead, 0),
            ("d", new BTT[0], BTT.DictionaryHead, 0),
        };

        public static IEnumerable<(string, BTT[], BTT)> ReadTokenType_InInitialState_DataAndTokensAndToken => ReadTokenType_InInitialState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedTokenType));

        public static IEnumerable<(string, BTT[], int)> ReadTokenType_InInitialState_DataAndTokensAndPosition => ReadTokenType_InInitialState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedPosition));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes, BTT expectedTokenType, int expectedPosition)[] ReadTokenType_InListValueState_DataAndTokensAndTokenAndPosition = new[]
        {
            ("li", new BTT[] { BTT.ListHead }, BTT.Integer, 1),
            ("l0", new BTT[] { BTT.ListHead }, BTT.String, 1),
            ("ll", new BTT[] { BTT.ListHead }, BTT.ListHead, 1),
            ("ld", new BTT[] { BTT.ListHead }, BTT.DictionaryHead, 1),
            ("le", new BTT[] { BTT.ListHead }, BTT.ListTail, 1),
        };

        public static IEnumerable<(string bencodeString, BTT[] tokenTypes, BTT expectedTokenType)> ReadTokenType_InListValueState_DataAndTokensAndToken => ReadTokenType_InListValueState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedTokenType));

        public static IEnumerable<(string bencodeString, BTT[] tokenTypes, int expectedPosition)> ReadTokenType_InListValueState_DataAndTokensAndPosition => ReadTokenType_InListValueState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedPosition));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes, BTT expectedTokenType, int expectedPosition)[] ReadTokenType_InDictionaryValueState_DataAndTokensAndTokenAndPosition = new[]
        {
            ("d1:ai", new BTT[] { BTT.DictionaryHead, BTT.Key }, BTT.Integer, 4),
            ("d1:a0", new BTT[] { BTT.DictionaryHead, BTT.Key }, BTT.String, 4),
            ("d1:al", new BTT[] { BTT.DictionaryHead, BTT.Key }, BTT.ListHead, 4),
            ("d1:ad", new BTT[] { BTT.DictionaryHead, BTT.Key }, BTT.DictionaryHead, 4),
        };

        public static IEnumerable<(string, BTT[], BTT)> ReadTokenType_InDictionaryValueState_DataAndTokensAndToken => ReadTokenType_InDictionaryValueState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedTokenType));

        public static IEnumerable<(string, BTT[], int)> ReadTokenType_InDictionaryValueState_DataAndTokensAndPosition => ReadTokenType_InDictionaryValueState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedPosition));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes, BTT expectedTokenType, int expectedPosition)[] ReadTokenType_InDictionaryKeyState_DataAndTokensAndTokenAndPosition = new[]
        {
            ("d0", new BTT[] { BTT.DictionaryHead }, BTT.Key, 1),
            ("de", new BTT[] { BTT.DictionaryHead }, BTT.DictionaryTail, 1),
        };

        public static IEnumerable<(string bencodeString, BTT[] tokenTypes, BTT expectedTokenType)> ReadTokenType_InDictionaryKeyState_DataAndTokensAndToken => ReadTokenType_InDictionaryKeyState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedTokenType));

        public static IEnumerable<(string bencodeString, BTT[] tokenTypes, int expectedPosition)> ReadTokenType_InDictionaryKeyState_DataAndTokensAndPosition => ReadTokenType_InDictionaryKeyState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedPosition));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes, BTT expectedTokenType, int expectedPosition)[] ReadTokenType_InAnyTerminalState_DataAndTokensAndTokenAndPosition = new[]
        {
            ("i0e", new BTT[] { BTT.Integer }, BTT.None, 3),
            ("1:a", new BTT[] { BTT.String }, BTT.None, 3),
            ("le", new BTT[] { BTT.ListHead, BTT.ListTail }, BTT.None, 2),
            ("de", new BTT[] { BTT.DictionaryHead, BTT.DictionaryTail }, BTT.None, 2),
        };

        public static IEnumerable<(string bencodeString, BTT[] tokenTypes, BTT expectedTokenType)> ReadTokenType_InAnyTerminalState_DataAndTokensAndToken => ReadTokenType_InAnyTerminalState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedTokenType));

        public static IEnumerable<(string bencodeString, BTT[] tokenTypes, int expectedPosition)> ReadTokenType_InAnyTerminalState_DataAndTokensAndPosition => ReadTokenType_InAnyTerminalState_DataAndTokensAndTokenAndPosition
            .Select(x => (x.bencodeString, x.tokenTypes, x.expectedPosition));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes, string errorMessage, int errorPosition)[] ReadTokenType_InInitialStateButMissingData_DataAndTokensAndError = new[]
        {
            ("", new BTT[0], "Expected 'd', 'i', 'l', or '0'-'9' but reached the end of the source buffer while reading the token type.", 0),
        };

        public static IEnumerable<(string bencodeString, string errorMessage, int expectedPosition)> ReadTokenType_InInitialStateButMissingData_DataAndError => ReadTokenType_InInitialStateButMissingData_DataAndTokensAndError
            .Select(x => (x.bencodeString, x.errorMessage, x.errorPosition));

        public static readonly (string bencodeString, BTT[] tokenTypes, string errorMessage, int errorPosition)[] ReadTokenType_InInitialStateButInvalidData_DataAndTokensAndError = new[]
        {
            (":", new BTT[0], "Expected 'd', 'i', 'l', or '0'-'9' but found ':' while reading the token type.", 0),
            ("-", new BTT[0], "Expected 'd', 'i', 'l', or '0'-'9' but found '-' while reading the token type.", 0),
            ("\0", new BTT[0], "Expected 'd', 'i', 'l', or '0'-'9' but found '\\x00' while reading the token type.", 0),
        };

        public static IEnumerable<(string bencodeString, string errorMessage, int expectedPosition)> ReadTokenType_InInitialStateButInvalidData_DataAndError => ReadTokenType_InInitialStateButInvalidData_DataAndTokensAndError
            .Select(x => (x.bencodeString, x.errorMessage, x.errorPosition));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes, string errorMessage, int errorPosition)[] ReadTokenType_InListValueStateButMissingData_DataAndTokensAndError = new[]
        {
            ("l", new BTT[] { BTT.ListHead }, "Expected 'd', 'e', 'i', 'l', or '0'-'9' but reached the end of the source buffer while reading the token type.", 1),
        };

        public static IEnumerable<(string bencodeString, string errorMessage, int expectedPosition)> ReadTokenType_InListValueStateButMissingData_DataAndError => ReadTokenType_InListValueStateButMissingData_DataAndTokensAndError
            .Select(x => (x.bencodeString, x.errorMessage, x.errorPosition));

        public static readonly (string bencodeString, BTT[] tokenTypes, string errorMessage, int errorPosition)[] ReadTokenType_InListValueStateButInvalidData_DataAndTokensAndError = new[]
        {
            ("l:", new BTT[] { BTT.ListHead }, $"Expected 'd', 'e', 'i', 'l', or '0'-'9' but found ':' while reading the token type.", 1),
            ("l-", new BTT[] { BTT.ListHead }, $"Expected 'd', 'e', 'i', 'l', or '0'-'9' but found '-' while reading the token type.", 1),
            ("l\0", new BTT[] { BTT.ListHead }, $"Expected 'd', 'e', 'i', 'l', or '0'-'9' but found '\\x00' while reading the token type.", 1),
        };

        public static IEnumerable<(string bencodeString, string errorMessage, int expectedPosition)> ReadTokenType_InListValueStateButInvalidData_DataAndError => ReadTokenType_InListValueStateButInvalidData_DataAndTokensAndError
            .Select(x => (x.bencodeString, x.errorMessage, x.errorPosition));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes, string errorMessage, int errorPosition)[] ReadTokenType_InDictionaryValueStateButMissingData_DataAndTokensAndError = new[]
        {
            ("d1:a", new BTT[] { BTT.DictionaryHead, BTT.Key }, "Expected 'd', 'i', 'l', or '0'-'9' but reached the end of the source buffer while reading the token type.", 4),
        };

        public static IEnumerable<(string bencodeString, string errorMessage, int expectedPosition)> ReadTokenType_InDictionaryValueStateButMissingData_DataAndError => ReadTokenType_InDictionaryValueStateButMissingData_DataAndTokensAndError
            .Select(x => (x.bencodeString, x.errorMessage, x.errorPosition));

        public static readonly (string bencodeString, BTT[] tokenTypes, string errorMessage, int errorPosition)[] ReadTokenType_InDictionaryValueStateButInvalidData_DataAndTokensAndError = new[]
        {
            ("d1:a:", new BTT[] { BTT.DictionaryHead, BTT.Key }, "Expected 'd', 'i', 'l', or '0'-'9' but found ':' while reading the token type.", 4),
            ("d1:a-", new BTT[] { BTT.DictionaryHead, BTT.Key }, "Expected 'd', 'i', 'l', or '0'-'9' but found '-' while reading the token type.", 4),
            ("d1:a\0", new BTT[] { BTT.DictionaryHead, BTT.Key }, "Expected 'd', 'i', 'l', or '0'-'9' but found '\\x00' while reading the token type.", 4),
        };

        public static IEnumerable<(string bencodeString, string errorMessage, int expectedPosition)> ReadTokenType_InDictionaryValueStateButInvalidData_DataAndError => ReadTokenType_InDictionaryValueStateButInvalidData_DataAndTokensAndError
            .Select(x => (x.bencodeString, x.errorMessage, x.errorPosition));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] tokenTypes, string errorMessage, int errorPosition)[] ReadTokenType_InDictionaryKeyStateButMissingData_DataAndTokensAndError = new[]
        {
            ("d", new BTT[] { BTT.DictionaryHead }, "Expected 'e' or '0'-'9' but reached the end of the source buffer while reading the token type.", 1),
        };

        public static IEnumerable<(string bencodeString, string errorMessage, int expectedPosition)> ReadTokenType_InDictionaryKeyStateButMissingData_DataAndError => ReadTokenType_InDictionaryKeyStateButMissingData_DataAndTokensAndError
            .Select(x => (x.bencodeString, x.errorMessage, x.errorPosition));

        public static readonly (string bencodeString, BTT[] tokenTypes, string errorMessage, int errorPosition)[] ReadTokenType_InDictionaryKeyStateWithInvalidData_DataAndTokensAndError = new[]
        {
            ("dd", new BTT[] { BTT.DictionaryHead }, $"Expected 'e' or '0'-'9' but found 'd' while reading the token type.", 1),
            ("di", new BTT[] { BTT.DictionaryHead }, $"Expected 'e' or '0'-'9' but found 'i' while reading the token type.", 1),
            ("dl", new BTT[] { BTT.DictionaryHead }, $"Expected 'e' or '0'-'9' but found 'l' while reading the token type.", 1),
            ("d:", new BTT[] { BTT.DictionaryHead }, $"Expected 'e' or '0'-'9' but found ':' while reading the token type.", 1),
            ("d-", new BTT[] { BTT.DictionaryHead }, $"Expected 'e' or '0'-'9' but found '-' while reading the token type.", 1),
            ("d\0", new BTT[] { BTT.DictionaryHead }, $"Expected 'e' or '0'-'9' but found '\\x00' while reading the token type.", 1),
        };

        public static IEnumerable<(string bencodeString, string errorMessage, int expectedPosition)> ReadTokenType_InDictionaryKeyStateButInvalidData_DataAndError => ReadTokenType_InDictionaryKeyStateWithInvalidData_DataAndTokensAndError
            .Select(x => (x.bencodeString, x.errorMessage, x.errorPosition));
    }
}
