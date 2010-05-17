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
using System.Text;

#endregion

namespace Interlace.Amf
{
    public class AmfRegistry
    {
        Dictionary<string, IAmfClassDescriptor> _aliases;
        Dictionary<Type, IAmfClassDescriptor> _types;

        public AmfRegistry()
        {
            _aliases = new Dictionary<string, IAmfClassDescriptor>();
            _types = new Dictionary<Type, IAmfClassDescriptor>();

            AmfAnonymousClassDescriptor anonymousDescriptor = new AmfAnonymousClassDescriptor();

            _aliases[""] = anonymousDescriptor;
            _types[typeof(AmfObject)] = anonymousDescriptor;
        }

        public void RegisterClassAlias(Type type)
        {
            AmfClassDescriptor classDescriptor = new AmfClassDescriptor(type);
            _aliases[classDescriptor.Alias] = classDescriptor;
            _types[type] = classDescriptor;
        }

        public IAmfClassDescriptor GetByAlias(string name)
        {
            if (!_aliases.ContainsKey(name)) throw new AmfException(string.Format(
                "No class has been registered for the class alias \"{0}\".", name));

            return _aliases[name];
        }

        internal IAmfClassDescriptor GetByType(Type valueType)
        {
            if (!_types.ContainsKey(valueType)) throw new AmfException(string.Format(
                "No class has been registered for the type \"{0}\".", valueType.Name));

            return _types[valueType];
        }
    }
}
