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
    public static class BencodeDictionaryTestData
    {
        public static readonly (string bencodeString, (int keyIndex, string key, int position)[] elements)[] DataAndElements = new[]
        {
            ("d0:i1ee", new[] { (3, "", 3) }),
            ("d2:aai1ee", new[] { (3, "aa", 5) }),
            ("d1:ai1e1:bi2e1:ci3e1:di4e1:ei5ee", new[] { (3, "a", 4), (9, "b", 10), (15, "c", 16), (21, "d", 22), (27, "e", 28) }),

            // All orders
            ("de", new (int, string, int)[0]),
            ("d1:ai1ee", new[] { (3, "a", 4) }),
            ("d1:ai1e1:bi2ee", new[] { (3, "a", 4), (9, "b", 10) }),
            ("d1:bi1e1:ai2ee", new[] { (3, "b", 4), (9, "a", 10) }),
            ("d1:ai1e1:bi2e1:ci3ee", new[] { (3, "a", 4), (9, "b", 10), (15, "c", 16) }),
            ("d1:ai1e1:ci2e1:bi3ee", new[] { (3, "a", 4), (9, "c", 10), (15, "b", 16) }),
            ("d1:bi1e1:ai2e1:ci3ee", new[] { (3, "b", 4), (9, "a", 10), (15, "c", 16) }),
            ("d1:bi1e1:ci2e1:ai3ee", new[] { (3, "b", 4), (9, "c", 10), (15, "a", 16) }),
            ("d1:ci1e1:ai2e1:bi3ee", new[] { (3, "c", 4), (9, "a", 10), (15, "b", 16) }),
            ("d1:ci1e1:bi2e1:ai3ee", new[] { (3, "c", 4), (9, "b", 10), (15, "a", 16) }),
        };

        public static readonly (string bencodeString, (int keyIndex, string key, int position)[] elements, string nonKey)[] DataAndElementsAndNonKey = new[]
        {
            ("de", new (int, string, int)[0], ""),
            ("d0:i1ee", new[] { (3, "", 3) }, "a"),
            ("d1:ai1ee", new[] { (3, "a", 4) }, ""),
            ("d1:ai1ee", new[] { (3, "a", 4) }, "aa"),
            ("d2:aai1ee", new[] { (3, "aa", 5) }, "a"),

            // All positions
            ("de", new (int, string, int)[0], "a"),
            ("d1:bi1ee", new[] { (3, "b", 4) }, "a"),
            ("d1:bi1ee", new[] { (3, "b", 4) }, "c"),
            ("d1:bi1e1:di2ee", new[] { (3, "b", 4), (9, "d", 10) }, "a"),
            ("d1:bi1e1:di2ee", new[] { (3, "b", 4), (9, "d", 10) }, "c"),
            ("d1:bi1e1:di2ee", new[] { (3, "b", 4), (9, "d", 10) }, "e"),
            ("d1:bi1e1:di2e1:fi3ee", new[] { (3, "b", 4), (9, "d", 10), (15, "f", 16) }, "a"),
            ("d1:bi1e1:di2e1:fi3ee", new[] { (3, "b", 4), (9, "d", 10), (15, "f", 16) }, "c"),
            ("d1:bi1e1:di2e1:fi3ee", new[] { (3, "b", 4), (9, "d", 10), (15, "f", 16) }, "e"),
            ("d1:bi1e1:di2e1:fi3ee", new[] { (3, "b", 4), (9, "d", 10), (15, "f", 16) }, "g"),
        };
    }
}
