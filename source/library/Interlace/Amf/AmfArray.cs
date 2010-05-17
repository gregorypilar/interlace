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
    public class AmfArray
    {
        List<object> _denseElements;
        Dictionary<string, object> _associativeElements;

        public AmfArray()
        {
            _denseElements = new List<object>();
            _associativeElements = new Dictionary<string, object>();
        }

        public AmfArray(IEnumerable<object> denseElements, IDictionary<string, object> associativeElements)
        {
            _denseElements = new List<object>(denseElements);
            _associativeElements = new Dictionary<string, object>(associativeElements);
        }

        public AmfArray(IEnumerable<object> denseElements)
        {
            _denseElements = new List<object>(denseElements);
            _associativeElements = new Dictionary<string, object>();
        }

        public static AmfArray Dense(params object[] elements)
        {
            return new AmfArray(elements);
        }

        public object this[string indexString]
        {
            get 
            {
                int index;

                if (int.TryParse(indexString, out index))
                {
                    if (0 <= index && index < _denseElements.Count)
                    {
                        return _denseElements[index];
                    }
                }

                return _associativeElements[indexString];
            }

            set
            {
                int index;

                if (int.TryParse(indexString, out index))
                {
                    if (0 <= index && index < _denseElements.Count)
                    {
                        _denseElements[index] = value;
                    }
                }

                _associativeElements[indexString] = value;
            }
        }

        public object this[int index]
        {
            get 
            {
                if (0 <= index && index < _denseElements.Count)
                {
                    return _denseElements[index];
                }

                return _associativeElements[index.ToString()];
            }
            set
            {
                if (0 <= index && index < _denseElements.Count)
                {
                    _denseElements[index] = value;
                }
                else if (index == _denseElements.Count)
                {
                    _denseElements.Add(value);
                }
                else
                {
                    _associativeElements[index.ToString()] = value;
                }
            }
        }

        public IList<object> DenseElements
        { 	 
           get { return _denseElements; }
        }

        public IDictionary<string, object> AssociativeElements
        { 	 
           get { return _associativeElements; }
        }

        public bool IsEmpty
        {
            get { return _associativeElements.Count == 0 && _denseElements.Count == 0; }
        }
    }
}
