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
        public static IEnumerable<(string bencodeString, string errorMessage, int errorPosition)> SkipValue_MissingData_DataAndError => Enumerable.Empty<(string, string, int)>()
            .Concat(ReadInteger_MissingData_DataAndError
                .Where(x => x.bencodeString.StartsWith("i", StringComparison.Ordinal)))
            .Concat(ReadString_MissingHeadData_DataAndError
                .Where(x => x.bencodeString.Length > 0 && "0123456789".IndexOf(x.bencodeString[0]) >= 0))
            .Concat(ReadString_MissingBodyData_DataAndError
                .Where(x => x.bencodeString.Length > 0 && "0123456789".IndexOf(x.bencodeString[0]) >= 0));

        public static IEnumerable<(string bencodeString, string errorMessage, int errorPosition)> SkipValue_InvalidData_DataAndError => Enumerable.Empty<(string, string, int)>()
            .Concat(ReadInteger_InvalidData_DataAndError
                .Where(x => x.bencodeString.StartsWith("i", StringComparison.Ordinal)))
            .Concat(ReadString_InvalidData_DataAndError
                .Where(x => x.bencodeString.Length > 0 && "0123456789".IndexOf(x.bencodeString[0]) >= 0));

        public static IEnumerable<(string bencodeString, string errorMessage, int errorPosition, int expectedPosition)> SkipValue_UnsupportedData_DataAndError => Enumerable.Empty<(string, string, int, int)>()
            .Concat(ReadInteger_UnsupportedData_DataAndErrorAndPosition
                .Where(x => x.bencodeString.StartsWith("i", StringComparison.Ordinal)))
            .Concat(ReadString_UnsupportedData_DataAndErrorAndPosition
                .Where(x => x.bencodeString.Length > 0 && "0123456789".IndexOf(x.bencodeString[0]) >= 0));
    }
}
