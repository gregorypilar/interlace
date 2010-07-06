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

using Interlace.Collections;

#endregion

namespace Interlace.Pinch.Dom
{
    public class NamespaceName
    {
        CactusStack<string> _name;

        protected NamespaceName(CactusStack<string> name)
        {
            _name = name;
        }

        public NamespaceName(string identifier)
        {
            _name = new CactusStack<string>(identifier);
        }

        public static NamespaceName Parse(string dottedName)
        {
            CactusStack<string> top = null;

            foreach (string part in dottedName.Split('.'))
            {
                if (top == null) 
                {
                    top = new CactusStack<string>(part);
                }
                else
                {
                    top = top.Push(part);
                }
            }

            return new NamespaceName(top);
        }

        public NamespaceName(NamespaceName name, string identifier)
        {
            _name = name._name.Push(identifier);
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            NamespaceName rhs = obj as NamespaceName;

            if (rhs == null) return false;

            return _name.Equals(rhs._name);
        }

        public override string ToString()
        {
            List<string> reversed = new List<string>(_name.Values);

            return string.Join(".", reversed.ToArray());
        }

        public bool IsQualified
        {
            get { return _name.Count > 1; }
        }

        public string UnqualifiedName
        {
            get { return _name.Value; }
        }

        public NamespaceName ContainingName
        {
            get 
            { 
                return new NamespaceName(_name.Parent); 
            }
        }
    }
}
