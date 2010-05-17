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
using System.ComponentModel;
using System.Text;

#endregion

namespace Interlace.Utilities
{
    public delegate void PropertyEventFireDelegate(string propertyName);

    public class PropertyEventMap
    {
        readonly Dictionary<string, List<string>> _propertyNameMap;

        public PropertyEventMap()
        {
            _propertyNameMap = new Dictionary<string, List<string>>();
        }

        public void Map(string sourceProperty)
        {
            InternalMap(sourceProperty, sourceProperty, null);
        }

        public void Map(string sourceProperty, string destinationProperty)
        {
            InternalMap(sourceProperty, destinationProperty, null);
        }

        public void Map(string sourceProperty, string destinationProperty, params string[] otherDestinationProperties)
        {
            InternalMap(sourceProperty, destinationProperty, otherDestinationProperties);
        }

        void InternalMap(string sourceProperty, string destinationProperty, string[] otherDestinationProperties)
        {
            if (!_propertyNameMap.ContainsKey(sourceProperty))
            {
                _propertyNameMap[sourceProperty] = 
                    new List<string>(1 + (otherDestinationProperties != null ? otherDestinationProperties.Length : 0));
            }

            _propertyNameMap[sourceProperty].Add(destinationProperty);
            if (otherDestinationProperties != null) _propertyNameMap[sourceProperty].AddRange(otherDestinationProperties);
        }

        public void ConnectEvents(PropertyEventFireDelegate fire, INotifyPropertyChanged source)
        {
            source.PropertyChanged += new PropertyChangedEventHandler(
                delegate(object sender, PropertyChangedEventArgs e)
                {
                    List<string> destinationProperties;
                    
                    if (_propertyNameMap.TryGetValue(e.PropertyName, out destinationProperties))
                    {
                        foreach (string propertyName in destinationProperties)
                        {
                            fire(propertyName);
                        }
                    }
                });
        }
    }
}
