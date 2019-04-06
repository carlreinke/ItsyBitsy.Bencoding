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

namespace ItsyBitsy.Bencoding.Tests
{
    public static partial class BencodeTestData
    {
        public static readonly (string bencodeString, string expectedValueString)[] ReadString_ValidData_DataAndValue = new[]
        {
            ("0:", ""),
            ("4:test", "test"),
            ("7:testabc", "testabc"),
            ("8:test abc", "test abc"),
            ("8:test:abc", "test:abc"),
            ("36:0123456789abcdefghijklmnopqrstuvwxyz", "0123456789abcdefghijklmnopqrstuvwxyz"),
        };

        public static IEnumerable<ValueTuple<string>> ReadString_ValidData_Data => ReadString_ValidData_DataAndValue
            .Select(x => ValueTuple.Create(x.bencodeString));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadString_MissingHeadData_DataAndError = new[]
        {
            // Missing head.
            ("", "Expected '0'-'9' but reached the end of the source buffer while reading a string.", 0),

            // Missing delimiter.
            ("0", "Expected ':' but reached the end of the source buffer while reading a string.", 1),
            ("1", "Expected '0'-'9' or ':' but reached the end of the source buffer while reading a string.", 1),
            ("10", "Expected '0'-'9' or ':' but reached the end of the source buffer while reading a string.", 2),
        };

        public static IEnumerable<(string bencodeString, int expectedLength)> ReadString_MissingHeadData_DataAndLength => ReadString_MissingHeadData_DataAndError
            .Select(x => (x.bencodeString, 0));

        public static IEnumerable<(string bencodeString, int expectedLength, string errorMessage, int errorPosition)> ReadString_MissingHeadData_DataAndLengthAndError => ReadString_MissingHeadData_DataAndError
            .Select(x => (x.bencodeString, 0, x.errorMessage, x.errorPosition));

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadString_InvalidData_DataAndError = new[]
        {
            // Invalid head byte.
            ("d0:", "Expected '0'-'9' but found 'd' while reading a string.", 0),
            ("e0:", "Expected '0'-'9' but found 'e' while reading a string.", 0),
            ("i0:", "Expected '0'-'9' but found 'i' while reading a string.", 0),
            ("l0:", "Expected '0'-'9' but found 'l' while reading a string.", 0),
            (":0:", "Expected '0'-'9' but found ':' while reading a string.", 0),
            ("-0:", "Expected '0'-'9' but found '-' while reading a string.", 0),
            ("\00:", "Expected '0'-'9' but found '\\x00' while reading a string.", 0),

            // Leading zeros.
            ("00:", "Expected ':' but found '0' while reading a string.", 1),
            ("01:", "Expected ':' but found '1' while reading a string.", 1),
            ("042:", "Expected ':' but found '4' while reading a string.", 1),
            ("01234567890:", "Expected ':' but found '1' while reading a string.", 1),
            ("001:", "Expected ':' but found '0' while reading a string.", 1),

            // No digits.
            (":", "Expected '0'-'9' but found ':' while reading a string.", 0),

            // Invalid digit.
            ("1a:", "Expected '0'-'9' or ':' but found 'a' while reading a string.", 1),
            ("1-:", "Expected '0'-'9' or ':' but found '-' while reading a string.", 1),
        };

        public static IEnumerable<(string bencodeString, int expectedLength)> ReadString_InvalidData_DataAndLength => ReadString_InvalidData_DataAndError
            .Select(x => (x.bencodeString, 0));

        public static IEnumerable<(string bencodeString, int expectedLength, string errorMessage, int errorPosition)> ReadString_InvalidData_DataAndLengthAndError => ReadString_InvalidData_DataAndError
            .Select(x => (x.bencodeString, 0, x.errorMessage, x.errorPosition));

        public static readonly (string bencodeString, int expectedLength, string errorMessage, int errorPosition)[] ReadString_MissingBodyData_DataAndLengthAndError = new[]
        {
            // Missing data.
            ("1:", 1, "Expected to read 1 bytes but reached the end of the source buffer while reading a string.", 2),
            ("5:test", 5, "Expected to read 5 bytes but reached the end of the source buffer while reading a string.", 6),
            ("6:test", 6, "Expected to read 6 bytes but reached the end of the source buffer while reading a string.", 6),
            ("100:test", 100, "Expected to read 100 bytes but reached the end of the source buffer while reading a string.", 8),
            ("2147483647:test", 2147483647, "Expected to read 2147483647 bytes but reached the end of the source buffer while reading a string.", 15),
        };

        public static IEnumerable<ValueTuple<string>> ReadString_MissingBodyData_Data => ReadString_MissingBodyData_DataAndLengthAndError
            .Select(x => ValueTuple.Create(x.bencodeString));

        public static IEnumerable<(string bencodeString, int expectedLength)> ReadString_MissingBodyData_DataAndLength => ReadString_MissingBodyData_DataAndLengthAndError
            .Select(x => (x.bencodeString, x.expectedLength));

        public static IEnumerable<(string bencodeString, string errorMessage, int errorPosition)> ReadString_MissingBodyData_DataAndError => ReadString_MissingBodyData_DataAndLengthAndError
            .Select(x => (x.bencodeString, x.errorMessage, x.errorPosition));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, string errorMessage, int errorPosition, int expectedPosition)[] ReadString_UnsupportedData_DataAndErrorAndPosition = new[]
        {
            ("2147483648:", "The string length is not in the supported range from '0' to '2147483647'.", 0, 0),
            ("12345678901:", "The string length is not in the supported range from '0' to '2147483647'.", 0, 0),
            ("123456789012:", "The string length is not in the supported range from '0' to '2147483647'.", 0, 0),
        };
    }
}
