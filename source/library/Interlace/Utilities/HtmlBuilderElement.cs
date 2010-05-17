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
using System.Xml;

#endregion

namespace Interlace.Utilities
{
    public class HtmlBuilderElement
    {
        string _name;
        string _text;

        Dictionary<string, string> _values;
        List<HtmlBuilderElement> _elements;

        public HtmlBuilderElement(string name)
        {
            _name = name;
            _values = new Dictionary<string, string>();
            _elements = new List<HtmlBuilderElement>();
        }

        public HtmlBuilderElement AddElement(string name)
        {
            HtmlBuilderElement element = new HtmlBuilderElement(name);
            _elements.Add(element);

            return element;
        }

        public void Add(string name, object value)
        {
            if (value == null) return;

            string stringValue = string.Format("{0}", value);

            if (string.IsNullOrEmpty(stringValue)) return;

            _values[name] = stringValue;
        }

        public void AddProperties(object obj, params string[] propertyNames)
        {
            Type objectType = obj.GetType();

            foreach (string propertyName in propertyNames)
            {
                PropertyInfo property = objectType.GetProperty(propertyName);

                if (property == null)
                {
                    throw new ArgumentException(string.Format(
                        "The property \"{0}\" is not available on the object of type \"{1}\".",
                        propertyName, objectType.Name));
                }

                object value = property.GetValue(obj, null);

                Add(propertyName, string.Format("{0}", value));
            }
        }

        internal XmlElement Render(XmlDocument document)
        {
            XmlElement element = document.CreateElement(_name);

            foreach (KeyValuePair<string, string> pair in _values)
            {
                XmlElement valueElement = document.CreateElement(pair.Key);
                valueElement.InnerText = pair.Value;

                element.AppendChild(valueElement);
            }

            foreach (HtmlBuilderElement builder in _elements)
            {
                XmlElement subElement = builder.Render(document);

                element.AppendChild(subElement);
            }

            if (!string.IsNullOrEmpty(_text))
            {
                element.InnerText = Text;
            }

            return element;
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }
    }
}
