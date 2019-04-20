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
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nested type avoids name conflict with comparable system type.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeDictionary.Enumerator")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nested type avoids name conflict with comparable system type.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeDictionary.KeyPositionPair")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nested type avoids name conflict with comparable system type.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeSpanDictionary.Enumerator")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Nested type avoids name conflict with comparable system type.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeSpanDictionary.KeyPositionPair")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity", Justification = "The code is not complex.", Scope = "member", Target = "~M:ItsyBitsy.Bencoding.BencodeSpanReader.ReadTokenTypeInternal(System.ReadOnlySpan{System.Byte},System.Int32@,ItsyBitsy.Bencoding.BencodeSpanReader.State)~ItsyBitsy.Bencoding.BencodeTokenType")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "The type represents a bencode dictionary.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeDictionary")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "The type represents a bencode dictionary.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeDictionary")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "The type represents a bencode dictionary.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeSpanDictionary")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "The enumeration values represent types.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeTokenType")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "A dictionary should never be compared for equality.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeDictionary")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "A dictionary should never be compared for equality.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeSpanDictionary")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "A span reader should never be compared for equality.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeSpanReader")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "A span writer should never be compared for equality.", Scope = "type", Target = "~T:ItsyBitsy.Bencoding.BencodeSpanWriter")]
