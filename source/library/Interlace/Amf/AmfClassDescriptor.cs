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
    public class AmfClassDescriptor : IAmfClassDescriptor
    {
        Type _type;
        AmfClassAttribute _classAttribute;
        List<AmfPropertyDescriptor> _properties;

        AmfTraits _serializationTraits = null;

        public AmfClassDescriptor(Type type)
        {
            _type = type;

            object[] classAttributes = type.GetCustomAttributes(typeof(AmfClassAttribute), true);

            if (classAttributes.Length != 1) throw new ArgumentException("The specified type is not marked with the AmfClassAttribute.", "type");

            _classAttribute = classAttributes[0] as AmfClassAttribute;

            _properties = new List<AmfPropertyDescriptor>();

            foreach (PropertyInfo property in type.GetProperties())
            {
                object[] propertyAttributes = property.GetCustomAttributes(typeof(AmfPropertyAttribute), true);

                if (propertyAttributes.Length == 0) continue;

                AmfPropertyDescriptor descriptor = new AmfPropertyDescriptor(property, propertyAttributes[0] as AmfPropertyAttribute);

                _properties.Add(descriptor);
            }
        }

        public string Alias
        {
            get { return _classAttribute.Alias; }
        }

        public object BeginDeserialization(AmfTraits traits)
        {
            return Activator.CreateInstance(_type);
        }

        public void EndDeserialization(AmfTraits traits, object deserializingTo, IDictionary<string, object> staticMembers, IDictionary<string, object> dynamicMembers)
        {
            // Set the dynamic properties:
            if (traits.Kind == AmfTraitsKind.Dynamic)
            {
                if (!(deserializingTo is AmfObject)) throw new AmfException(string.Format(
                    "The AMF decoder received a dynamic object but the registered class for the object (\"{0}\") " +
                    "does not support dynamic properties (it does not inherit from AmfObject).", Alias));

                AmfObject amfObj = deserializingTo as AmfObject;

                foreach (KeyValuePair<string, object> pair in dynamicMembers)
                {
                    amfObj.Properties[pair.Key] = pair.Value;
                }
            }

            // Check for unexpected properties:
            if (_properties.Count != staticMembers.Count)
            {
                // (If the property count is equal and there is an unexpected property, there
                // must also be a missing property that will trigger later).

                Dictionary<string, bool> receivedProperties = new Dictionary<string, bool>();

                foreach (string key in staticMembers.Keys)
                {
                    receivedProperties[key] = true;
                }

                foreach (AmfPropertyDescriptor descriptor in _properties)
                {
                    foreach (string propertyName in descriptor.GetUsedPropertyNames(this))
                    {
                        if (!receivedProperties.ContainsKey(propertyName))
                        {
                            throw new AmfException(string.Format(
                                "The class registered for the alias \"{0}\" was received but was has an unexpected " +
                                "property (\"{1}\").", Alias, propertyName));
                        }
                    }
                }
            }

            foreach (AmfPropertyDescriptor descriptor in _properties)
            {
                descriptor.DeserializeProperty(this, deserializingTo, staticMembers);
            }
        }

        public void SerializeObject(object value, out AmfTraits traits, out IDictionary<string, object> staticMembers, out IDictionary<string, object> dynamicMembers)
        {
            if (_serializationTraits == null)
            {
                List<string> memberNames = new List<string>();

                foreach (AmfPropertyDescriptor property in _properties)
                {
                    memberNames.AddRange(property.GetUsedPropertyNames(this));
                }

                _serializationTraits = new AmfTraits(_classAttribute.Alias,
                    _type.IsSubclassOf(typeof(AmfObject)) ? AmfTraitsKind.Dynamic : AmfTraitsKind.Static, memberNames.ToArray());
            }

            traits = _serializationTraits;

            if (_serializationTraits.Kind == AmfTraitsKind.Dynamic)
            {
                dynamicMembers = (value as AmfObject).Properties;
            }
            else
            {
                dynamicMembers = null;
            }

            staticMembers = new Dictionary<string, object>();

            foreach (AmfPropertyDescriptor property in _properties)
            {
                property.SerializeProperty(this, value, staticMembers);
            }
        }
    }
}
