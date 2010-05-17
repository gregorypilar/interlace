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

namespace Interlace.Amf
{
    public class AmfPropertyDescriptor
    {
        readonly PropertyInfo _property;
        readonly AmfPropertyAttribute _attribute;
        readonly string _serializedName;

        public AmfPropertyDescriptor(PropertyInfo property, AmfPropertyAttribute attribute)
        {
            _property = property;
            _attribute = attribute;

            if (attribute.SerializedName == null)
            {
                _serializedName = _property.Name;
            }
            else
            {
                _serializedName = attribute.SerializedName;
            }
        }

        internal string[] GetUsedPropertyNames(AmfClassDescriptor descriptor)
        {
            return new string[] { _serializedName };
        }

        internal void DeserializeProperty(AmfClassDescriptor classDescriptor, object obj, IDictionary<string, object> staticMembers)
        {
            object value;
            
            if (staticMembers.TryGetValue(_serializedName, out value))
            {
                _property.SetValue(obj, value, null);
            }
            else
            {
                throw new AmfException(string.Format(
                    "The class registered for the alias \"{0}\" was received but was missing an expected " +
                    "property (\"{1}\").", classDescriptor.Alias, _serializedName));
            }
        }

        internal void SerializeProperty(AmfClassDescriptor classDescriptor, object obj, IDictionary<string, object> staticMembers)
        {
            staticMembers[_serializedName] = _property.GetValue(obj, null);
        }
    }
}
