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
        public static readonly (string bencodeString, long expectedValue)[] ReadInteger_ValidData_DataAndValue = new[]
        {
            ("i0e", 0),
            ("i1e", 1),
            ("i42e", 42),
            ("i100e", 100),
            ("i1234567890e", 1234567890),
            ("i9223372036854775807e", long.MaxValue),
            ("i-1e", -1),
            ("i-42e", -42),
            ("i-100e", -100),
            ("i-1234567890e", -1234567890),
            ("i-9223372036854775808e", long.MinValue),
        };

        public static IEnumerable<ValueTuple<string>> ReadInteger_ValidData_Data => ReadInteger_ValidData_DataAndValue
            .Select(x => ValueTuple.Create(x.bencodeString));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadInteger_MissingData_DataAndError = new[]
        {
            // Missing head byte.
            ("", "Expected 'i' but reached the end of the source buffer while reading an integer.", 0),

            // Missing digit.
            ("i", "Expected '-' or '0'-'9' but reached the end of the source buffer while reading an integer.", 1),

            // Missing tail byte.
            ("i1", "Expected 'e' or '0'-'9' but reached the end of the source buffer while reading an integer.", 2),
            ("i1234567890", "Expected 'e' or '0'-'9' but reached the end of the source buffer while reading an integer.", 11),
            ("i9223372036854775807", "Expected 'e' or '0'-'9' but reached the end of the source buffer while reading an integer.", 20),
        };

        public static readonly (string bencodeString, string errorMessage, int errorPosition)[] ReadInteger_InvalidData_DataAndError = new[]
        {
            // Invalid head byte.
            ("di1e", "Expected 'i' but found 'd' while reading an integer.", 0),
            ("ei1e", "Expected 'i' but found 'e' while reading an integer.", 0),
            ("li1e", "Expected 'i' but found 'l' while reading an integer.", 0),
            ("1i1e", "Expected 'i' but found '1' while reading an integer.", 0),
            (":i1e", "Expected 'i' but found ':' while reading an integer.", 0),
            ("-i1e", "Expected 'i' but found '-' while reading an integer.", 0),
            ("\0i1e", "Expected 'i' but found '\\x00' while reading an integer.", 0),

            // Leading zeros.
            ("i00e", "Expected 'e' but found '0' while reading an integer.", 2),
            ("i01e", "Expected 'e' but found '1' while reading an integer.", 2),
            ("i042e", "Expected 'e' but found '4' while reading an integer.", 2),
            ("i01234567890e", "Expected 'e' but found '1' while reading an integer.", 2),
            ("i001e", "Expected 'e' but found '0' while reading an integer.", 2),
            ("i-00e", "Expected '1'-'9' but found '0' while reading an integer.", 2),
            ("i-01e", "Expected '1'-'9' but found '0' while reading an integer.", 2),
            ("i-042e", "Expected '1'-'9' but found '0' while reading an integer.", 2),
            ("i-01234567890e", "Expected '1'-'9' but found '0' while reading an integer.", 2),
            ("i-001e", "Expected '1'-'9' but found '0' while reading an integer.", 2),

            // Negative zero.
            ("i-0e", "Expected '1'-'9' but found '0' while reading an integer.", 2),

            // Multiple negative signs.
            ("i--1e", "Expected '1'-'9' but found '-' while reading an integer.", 2),
            ("i--1234567890e", "Expected '1'-'9' but found '-' while reading an integer.", 2),

            // No digits.
            ("ie", "Expected '0'-'9' but found 'e' while reading an integer.", 1),
            ("i-e", "Expected '1'-'9' but found 'e' while reading an integer.", 2),

            // Invalid digit.
            ("iae", "Expected '0'-'9' but found 'a' while reading an integer.", 1),
            ("i1ae", "Expected 'e' or '0'-'9' but found 'a' while reading an integer.", 2),
            ("i1.2e", "Expected 'e' or '0'-'9' but found '.' while reading an integer.", 2),
            ("i1.e", "Expected 'e' or '0'-'9' but found '.' while reading an integer.", 2),
            ("i.2e", "Expected '0'-'9' but found '.' while reading an integer.", 1),
            ("i1-e", "Expected 'e' or '0'-'9' but found '-' while reading an integer.", 2),
            ("i-ae", "Expected '1'-'9' but found 'a' while reading an integer.", 2),
            ("i-1ae", "Expected 'e' or '0'-'9' but found 'a' while reading an integer.", 3),
            ("i-1.2e", "Expected 'e' or '0'-'9' but found '.' while reading an integer.", 3),
            ("i-1.e", "Expected 'e' or '0'-'9' but found '.' while reading an integer.", 3),
            ("i-.2e", "Expected '1'-'9' but found '.' while reading an integer.", 2),
            ("i-1-e", "Expected 'e' or '0'-'9' but found '-' while reading an integer.", 3),
        };

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, string errorMessage, int errorPosition, int expectedPosition)[] ReadInteger_UnsupportedData_DataAndErrorAndPosition = new[]
        {
            ("i9223372036854775808e", "The integer is not in the supported range from '-9223372036854775808' to '9223372036854775807'.", 0, 21),
            ("i-9223372036854775809e", "The integer is not in the supported range from '-9223372036854775808' to '9223372036854775807'.", 0, 22),
            ("i12345678901234567890e", "The integer is not in the supported range from '-9223372036854775808' to '9223372036854775807'.", 0, 22),
            ("i123456789012345678901e", "The integer is not in the supported range from '-9223372036854775808' to '9223372036854775807'.", 0, 23),
            ("i-12345678901234567890e", "The integer is not in the supported range from '-9223372036854775808' to '9223372036854775807'.", 0, 23),
            ("i-123456789012345678901e", "The integer is not in the supported range from '-9223372036854775808' to '9223372036854775807'.", 0, 24),
        };
    }
}
