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
    /// <summary>
    /// Writes a tree of XML elements to HTML, avoiding problems with parsing of XML by
    /// Internet Explorer.
    /// </summary>
    public class HtmlRenderer : XmlTransformer
    {
        public enum HtmlRendererEscapeBehaviour { FullEscape, IgnoreStyleElements }
        enum EscapeState { Escape, Ignore }
        
        Set<string> _noEndTags;

        EscapeState _escapeState = EscapeState.Escape;

        public HtmlRenderer()
        {
            PopulateNoTagsSet();
        }

        public HtmlRenderer(XmlDocument styleSheet)
        : base(styleSheet)
        {
            PopulateNoTagsSet();
        }

        public HtmlRenderer(string styleSheet)
        : base(styleSheet)
        {
            PopulateNoTagsSet();
        }

        void PopulateNoTagsSet()
        {
            _noEndTags = new Set<string>();

            _noEndTags.UnionUpdate("img");
            _noEndTags.UnionUpdate("br");
        }

        private readonly Regex _htmlEscapeRegex = new Regex("[<>&\"]");

        private void Write(XmlNode element, TextWriter writer, HtmlRendererEscapeBehaviour behaviour)
        {
            WriteNode(element, writer, behaviour);
        }

        public void Write(XmlDocument document, TextWriter writer, XsltArgumentList argumentsOrNull)
        {
            Write(document, writer, argumentsOrNull, HtmlRendererEscapeBehaviour.IgnoreStyleElements);
        }

        public void Write(XmlDocument document, TextWriter writer, XsltArgumentList argumentsOrNull, HtmlRendererEscapeBehaviour behaviour)
        {
            XmlDocument transformedDocument = Transform(document, argumentsOrNull);

            Write(transformedDocument.DocumentElement, writer, behaviour);
        }

        public string WriteToString(XmlDocument document, XsltArgumentList argumentsOrNull)
        {
            return WriteToString(document, argumentsOrNull, HtmlRendererEscapeBehaviour.IgnoreStyleElements);
        }

        public string WriteToString(XmlDocument document, XsltArgumentList argumentsOrNull, HtmlRendererEscapeBehaviour behaviour)
        {
            using (StringWriter stringWriter = new StringWriter())
            {
                Write(document, stringWriter, argumentsOrNull, behaviour);

                return stringWriter.GetStringBuilder().ToString();
            }
        }

        private string EscapeHtml(string text)
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
                    case '"':
                        return "&quot;";                              
                    default:
                        return match.Value;
                }
            });
        }

        private void WriteNode(XmlNode node, TextWriter writer, HtmlRendererEscapeBehaviour behaviour)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Element:
                    XmlElement element = node as XmlElement;
                    writer.Write("<");
                    writer.Write(element.Name);

                    if (element.Name.ToLower() == "style" && behaviour == HtmlRendererEscapeBehaviour.IgnoreStyleElements)
                    {
                        _escapeState = EscapeState.Ignore;
                    }

                    foreach (XmlAttribute attribute in node.Attributes)
                    {
                        writer.Write(" {0}=\"{1}\"", attribute.Name,

                            _escapeState == EscapeState.Escape ? EscapeHtml(attribute.Value) : attribute.Value

                            );
                    }

                    writer.Write(">");

                    foreach (XmlNode child in element.ChildNodes)
                    {
                        WriteNode(child, writer, behaviour);
                    }

                    if (!(element.ChildNodes.Count == 0 && _noEndTags.Contains(element.Name.ToLower())))
                    {
                        writer.Write("</");
                        writer.Write(element.Name);
                        writer.Write(">");
                    }

                    _escapeState = EscapeState.Escape;

                    break;
                case XmlNodeType.Text:
                    writer.Write(

                        _escapeState == EscapeState.Escape ? EscapeHtml((node as XmlText).Value) : (node as XmlText).Value

                        );
                    break;
            }
        }
    }
}
