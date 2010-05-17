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
using System.Xml;

#endregion

namespace Interlace.AdornedText.InDesign
{
    public class XmlBuilder
    {
        XmlDocument _document;
        XmlElement _current;
        Stack<XmlElement> _stack;

        public XmlBuilder(string rootElementName)
        {
            _document = new XmlDocument();
            _document.XmlResolver = null;

            _current = _document.CreateElement(rootElementName);
            _document.AppendChild(_current);

            _stack = new Stack<XmlElement>();
        }

        public XmlDocument Document
        { 	 
            get { return _document; }
        }

        public void WriteNewLine()
        {
            Write(Environment.NewLine);
        }

        public void WriteElement(string elementName, string text)
        {
            Push(elementName);
            Write(text);
            Pop();
        }

        public void Push(string elementName)
        {
            XmlElement newElement = _document.CreateElement(elementName);
            _current.AppendChild(newElement);

            _stack.Push(_current);
            _current = newElement;
        }

        public void SetAttribute(string name, string value)
        {
            _current.SetAttribute(name, value);
        }

        public void Write(string text)
        {
            XmlText textElement = _document.CreateTextNode(text);
            _current.AppendChild(textElement);
        }

        public void Pop()
        {
            _current = _stack.Pop();
        }
    }
}
