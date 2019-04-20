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
namespace ItsyBitsy.Bencoding.Tests
{
    public static class BencodeListTestData
    {
        public static readonly (string bencodeString, int[] positions)[] DataAndPositions = new[]
        {
            ("le", new int[0]),
            ("li1ee", new[] { 1 }),
            ("li1ei1ee", new[] { 1, 4 }),
            ("li1ei11ei111ee", new[] { 1, 4, 8 }),
            ("li111ei11ei1ee", new[] { 1, 6, 10 }),
        };
    }
}
