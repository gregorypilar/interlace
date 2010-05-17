#region Using Directives and Copyright Notice

// Copyright (c) 2007-2010, Computer Consultancy Pty Ltd
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Computer Consultancy Pty Ltd nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

#endregion

namespace Interlace.Utilities
{
    public static class ReflectionHelper
    {
        public static AttributedProperty<TAttribute> 
            WrapAttributedProperty<TAttribute>(PropertyInfo property) where TAttribute : Attribute
        {
            if (property == null) return null;

            object[] attributes = property.GetCustomAttributes(typeof(TAttribute), true);

            if (attributes.Length == 0) return null;

            if (attributes.Length > 1)
            {
                throw new InvalidOperationException("An object property passed to " +
                    "ReflectionHelper.GetAttributedProperties has multiple instances of " +
                    "an attribute, which is not supported.");
            }

            TAttribute attribute = attributes[0] as TAttribute;

            return new AttributedProperty<TAttribute>(property, attribute);
        }

        public static ICollection<AttributedProperty<TAttribute>> 
            GetAttributedProperties<TAttribute>(Type type) where TAttribute : Attribute
        {
            List<AttributedProperty<TAttribute>> properties = new List<AttributedProperty<TAttribute>>();

            foreach (PropertyInfo property in type.GetProperties())
            {
                AttributedProperty<TAttribute> attributed =
                    WrapAttributedProperty<TAttribute>(property);

                if (attributed != null) properties.Add(attributed);
            }

            return properties;
        }

        public static AttributedMethod<TAttribute> 
            WrapAttributedMethod<TAttribute>(MethodInfo method) where TAttribute : Attribute
        {
            if (method == null) return null;

            object[] attributes = method.GetCustomAttributes(typeof(TAttribute), true);

            if (attributes.Length == 0) return null;

            if (attributes.Length > 1)
            {
                throw new InvalidOperationException("An object property passed to " +
                    "ReflectionHelper.GetAttributedMethods has multiple instances of " +
                    "an attribute, which is not supported.");
            }

            TAttribute attribute = attributes[0] as TAttribute;

            return new AttributedMethod<TAttribute>(method, attribute);
        }


        public static ICollection<AttributedMethod<TAttribute>> 
            GetAttributedMethods<TAttribute>(Type type) where TAttribute : Attribute
        {
            List<AttributedMethod<TAttribute>> methods = new List<AttributedMethod<TAttribute>>();

            foreach (MethodInfo method in type.GetMethods())
            {
                AttributedMethod<TAttribute> attributed =
                    WrapAttributedMethod<TAttribute>(method);

                if (attributed != null) methods.Add(attributed);
            }

            return methods;
        }
    }
}
