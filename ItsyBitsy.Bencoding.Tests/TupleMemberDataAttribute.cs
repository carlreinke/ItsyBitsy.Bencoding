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
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Sdk;

namespace ItsyBitsy.Bencoding.Tests
{
    /// <summary>
    /// Like <see cref="MemberDataAttribute"/> but takes <see cref="ITuple"/> instead of
    /// <see cref="object[]"/>.
    /// </summary>
    [DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class TupleMemberDataAttribute : MemberDataAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TupleMemberDataAttribute"/> class.
        /// </summary>
        /// <param name="memberName">The name of the public static member on the test class that
        ///     will provide the test data.</param>
        /// <param name="parameters">The parameters for the member if it is a method.</param>
        public TupleMemberDataAttribute(string memberName, params object[] parameters)
            : base(memberName, parameters)
        {
        }

        protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
        {
            if (item == null)
                return null;

            var tuple = item as ITuple;
            if (tuple == null)
                throw new ArgumentException($"Property {MemberName} on {MemberType ?? testMethod.DeclaringType} yielded an item that is not an ITuple.");

            var objs = new object[tuple.Length];
            for (int i = 0; i < objs.Length; ++i)
                objs[i] = tuple[i];
            return objs;
        }
    }
}
