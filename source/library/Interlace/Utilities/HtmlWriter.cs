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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;

using Interlace.Collections;

#endregion

namespace Interlace.Utilities
{
    public class HtmlWriter
    {
        Set<string> _noEndTags;

        public HtmlWriter()
        {
            PopulateNoTagsSet();
        }

        void PopulateNoTagsSet()
        {
            _noEndTags = new Set<string>();

            _noEndTags.UnionUpdate("img");
            _noEndTags.UnionUpdate("br");
        }

        readonly Regex _htmlEscapeRegex = new Regex("[<>&]");

        public void Write(XmlNode node, TextWriter writer)
        {
            WriteNode(node, writer);
        }

        public void Write(XmlDocument document, TextWriter writer)
        {
            WriteNode(document.DocumentElement, writer);
        }

        public void Write(XmlNodeList nodes, TextWriter writer)
        {
            foreach (XmlNode node in nodes)
            {
                WriteNode(node, writer);
            }
        }

        public string WriteToString(XmlNode node)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                Write(node, stringWriter);

                return stringWriter.GetStringBuilder().ToString();
            }
        }

        public string WriteToString(XmlDocument document)
        {
            return WriteToString(document.DocumentElement);
        }

        public string WriteToString(XmlNodeList nodes)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                Write(nodes, stringWriter);

                return stringWriter.GetStringBuilder().ToString();
            }
        }

        string EscapeHtml(string text)
        {
            return _htmlEscapeRegex.Replace(text, (MatchEvaluator)delegate(Match match)
            {
                switch (match.Value[0])
                {
                    case '&':
                        return "&amp;";
                    case '<':
                        return "&lt;";
                    case '>':
                        return "&gt;";
                    default:
                        return match.Value;
                }
            });
        }

        void WriteNode(XmlNode node, TextWriter writer)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    XmlElement element = node as XmlElement;
                    writer.Write("<");
                    writer.Write(element.LocalName);

                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        if (attribute.Prefix == "xmlns") continue;

                        writer.Write(" {0}=\"{1}\"", attribute.Name, attribute.Value);
                    }

                    writer.Write(">");

                    foreach (XmlNode child in element.ChildNodes)
                    {
                        WriteNode(child, writer);
                    }

                    if (!(element.ChildNodes.Count == 0 && _noEndTags.Contains(element.Name.ToLower())))
                    {
                        writer.Write("</");
                        writer.Write(element.LocalName);
                        writer.Write(">");
                    }

                    break;

                case XmlNodeType.Text:
                    writer.Write(EscapeHtml((node as XmlText).Value));
                    break;
            }
        }
    }
}
