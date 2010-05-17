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

#endregion

namespace Interlace.Utilities
{
    public class XmlTransformer
    {
        XmlNode _styleSheet;
        XslCompiledTransform _transform;

        public XmlTransformer()
        {
            StyleSheet = null;
        }

        public XmlTransformer(XmlNode styleSheet)
        {
            StyleSheet = styleSheet;
        }

        public XmlTransformer(string styleSheet)
        {
            XmlDocument document = new XmlDocument();
            document.XmlResolver = null;

            document.LoadXml(styleSheet);

            StyleSheet = document;
        }

        public XmlNode StyleSheet
        {
            get { return _styleSheet; }
            set
            {
                if (value != null)
                {
                    _styleSheet = value;
                    _transform = new XslCompiledTransform();
                    _transform.Load(_styleSheet);
                }
                else
                {
                    _styleSheet = null;
                    _transform = null;
                }
            }
        }

        public XmlDocument Transform(XmlDocument document, XsltArgumentList argumentsOrNull)
        {
            if (_transform == null) return document;

            XmlNodeReader reader = new XmlNodeReader(document.DocumentElement);
            XmlDocument result = new XmlDocument();

            using (MemoryStream documentStream = new MemoryStream())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.CloseOutput = false;

                using (XmlWriter documentWriter = XmlWriter.Create(documentStream, settings))
                {
                    XsltArgumentList arguments = argumentsOrNull ?? new XsltArgumentList();

                    _transform.Transform(reader, arguments, documentWriter);

                    documentWriter.Flush();
                }

                documentStream.Seek(0, SeekOrigin.Begin);

                XmlDocument transformedDocument = new XmlDocument();
                transformedDocument.XmlResolver = null;
                transformedDocument.Load(documentStream);

                return transformedDocument;
            }
        }
    }
}
