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

namespace Interlace.Utilities
{
    /// <summary>
    /// Keeps a list of objects, giving each object a unique label by reference; used for debugging.
    /// </summary>
    public class ReferenceLabeller
    {
        List<object> _objects = new List<object>();

        readonly string characterSet = "ABCDEFGHIJKLMNOPQRSTUVWXY";
        readonly string indexedFormat = "Z-{0}";

        public string Label(object obj)
        {
            int index = GetIndex(obj);

            if (index < characterSet.Length)
            {
                return characterSet.Substring(index, 1);
            }
            else 
            {
                return string.Format(indexedFormat, index - characterSet.Length + 1);
            }
        }

        int GetIndex(object obj)
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                if (object.ReferenceEquals(_objects[i], obj))
                {
                    return i;
                }
            }

            int index = _objects.Count;

            _objects.Add(obj);

            return index;
        }
    }
}
