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
        public static readonly (string bencodeString, BTT[] tokenTypes)[] CompleteValue_DataAndTokens = new[]
        {
            ("i1e", new BTT[] { BTT.Integer }),
            ("1:a", new BTT[] { BTT.String }),
            ("le", new BTT[] { BTT.ListHead, BTT.ListTail }),
            ("de", new BTT[] { BTT.DictionaryHead, BTT.DictionaryTail }),
        };

        public static readonly (string bencodeString, BTT[] tokenTypes)[] IncompleteValue_DataAndTokens = new[]
        {
            ("", new BTT[0]),
            ("l", new BTT[] { BTT.ListHead }),
            ("lle", new BTT[] { BTT.ListHead, BTT.ListHead, BTT.ListTail }),
            ("d", new BTT[] { BTT.DictionaryHead }),
            ("d1:a", new BTT[] { BTT.DictionaryHead, BTT.Key }),
            ("d1:ade", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.DictionaryHead, BTT.DictionaryTail }),
        };

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[])[] InInitialState_DataAndTokens = new[]
        {
            ("", new BTT[0]),
        };

        public static readonly (string bencodeString, BTT[])[] InListValueState_DataAndTokens = new[]
        {
            ("l", new BTT[] { BTT.ListHead }),
        };

        public static readonly (string bencodeString, BTT[])[] InDictionaryValueState_DataAndTokens = new[]
        {
            ("d1:a", new BTT[] { BTT.DictionaryHead, BTT.Key }),
        };

        public static readonly (string bencodeString, BTT[])[] InDictionaryKeyState_DataAndTokens = new[]
        {
            ("d", new BTT[] { BTT.DictionaryHead }),
        };

        public static readonly (string bencodeString, BTT[])[] InFinalState_DataAndTokens = new[]
        {
            ("i1e", new BTT[] { BTT.Integer }),
        };

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, int position, BTT[] tokenTypes)[] Position_DataAndPositionAndTokens = new[]
        {
            ("li0e1:aledee", 0, new BTT[] { BTT.ListHead, BTT.Integer, BTT.String, BTT.ListHead, BTT.ListTail, BTT.DictionaryHead, BTT.DictionaryTail, BTT.ListTail, BTT.None }),
            ("li0e1:aledee", 1, new BTT[] { BTT.Integer, BTT.None }),
            ("li0e1:aledee", 4, new BTT[] { BTT.String, BTT.None }),
            ("li0e1:aledee", 7, new BTT[] { BTT.ListHead, BTT.ListTail, BTT.None }),
            ("li0e1:aledee", 9, new BTT[] { BTT.DictionaryHead, BTT.DictionaryTail, BTT.None }),
            ("d1:ai0ee", 0, new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.DictionaryTail, BTT.None }),
            ("d1:ai0ee", 1, new BTT[] { BTT.String }),
            ("d1:ai0ee", 4, new BTT[] { BTT.Integer }),
        };

        public static readonly IEnumerable<(string bencodeString, int position)> Position_DataAndPosition = Position_DataAndPositionAndTokens
            .Select(x => (x.bencodeString, x.position));

        ////////////////////////////////////////////////////////////////////////////////////////////

        public static readonly (string bencodeString, BTT[] nodeTypes1, BTT[] nodeTypes2, BTT[] nodeTypes3)[] CreateSpan_DataAndTokens = new[]
        {
            ("le", new BTT[0], new BTT[] { BTT.ListHead }, new BTT[] { BTT.ListTail }),
            ("le", new BTT[0], new BTT[] { BTT.ListHead, BTT.ListTail }, new BTT[0]),
            ("le", new BTT[] { BTT.ListHead }, new BTT[0], new BTT[] { BTT.ListTail }),
            ("le", new BTT[] { BTT.ListHead }, new BTT[] { BTT.ListTail }, new BTT[0]),
            ("de", new BTT[0], new BTT[] { BTT.DictionaryHead }, new BTT[] { BTT.DictionaryTail }),
            ("de", new BTT[0], new BTT[] { BTT.DictionaryHead, BTT.DictionaryTail }, new BTT[0]),
            ("de", new BTT[] { BTT.DictionaryHead }, new BTT[0], new BTT[] { BTT.DictionaryTail }),
            ("de", new BTT[] { BTT.DictionaryHead }, new BTT[] { BTT.DictionaryTail }, new BTT[0]),
            ("d1:ai1ee", new BTT[0], new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer, BTT.DictionaryTail }, new BTT[0]),
            ("d1:ai1ee", new BTT[0] { }, new BTT[] { BTT.DictionaryHead }, new BTT[] { BTT.Key, BTT.Integer, BTT.DictionaryTail }),
            ("d1:ai1ee", new BTT[] { BTT.DictionaryHead }, new BTT[] { BTT.Key }, new BTT[] { BTT.Integer, BTT.DictionaryTail }),
            ("d1:ai1ee", new BTT[] { BTT.DictionaryHead }, new BTT[] { BTT.Key, BTT.Integer }, new BTT[] { BTT.DictionaryTail }),
            ("d1:ai1ee", new BTT[] { BTT.DictionaryHead, BTT.Key }, new BTT[] { BTT.Integer }, new BTT[] { BTT.DictionaryTail }),
            ("d1:ai1ee", new BTT[] { BTT.DictionaryHead, BTT.Key, BTT.Integer }, new BTT[] { BTT.DictionaryTail }, new BTT[0]),
        };
    }
}
