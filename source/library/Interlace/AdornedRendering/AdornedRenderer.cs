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
using System.Xml;
using System.Xml.Xsl;

using Interlace.AdornedText;
using Interlace.Utilities;

#endregion

namespace Interlace.AdornedRendering
{
    public class AdornedRenderer
    {
        HtmlRenderer _renderer;
        IAdornedPreprocessor _preprocessor = new NullAdornedPreprocessor();

        public AdornedRenderer()
        {
            using (Stream styleStream = new MemoryStream(AdornedStyles.HtmlStyle))
            {
                XmlDocument document = new XmlDocument();
                document.XmlResolver = null;

                document.Load(styleStream);

                _renderer = new HtmlRenderer(document);
            }
        }

        public IAdornedPreprocessor Preprocessor
        {
           get { return _preprocessor; }
           set { _preprocessor = value; }
        }

        public void RenderDocument(TextReader documentReader, IAdornedReferenceResolver resolver, TextWriter outputTo)
        {
            AdornedProcessor processor = new AdornedProcessor();
            processor.Parse(documentReader);

            RenderProcessor(processor, resolver, outputTo, "document");
        }

        public void RenderDocument(Section parsedDocument, IAdornedReferenceResolver resolver, TextWriter outputTo)
        {
            AdornedProcessor processor = new AdornedProcessor();
            processor.Assign(parsedDocument);

            RenderProcessor(processor, resolver, outputTo, "document");
        }

        public void RenderFragment(TextReader documentReader, IAdornedReferenceResolver resolver, TextWriter outputTo)
        {
            AdornedProcessor processor = new AdornedProcessor();
            processor.Parse(documentReader);

            RenderProcessor(processor, resolver, outputTo, "fragment");
        }

        public void RenderFragment(Section parsedDocument, IAdornedReferenceResolver resolver, TextWriter outputTo)
        {
            AdornedProcessor processor = new AdornedProcessor();
            processor.Assign(parsedDocument);

            RenderProcessor(processor, resolver, outputTo, "fragment");
        }

        public void RenderProcessor(AdornedProcessor processor, IAdornedReferenceResolver resolver, TextWriter outputTo, string resultKind)
        {
            processor.Preprocess(_preprocessor);
            processor.ResolveReferences(resolver);
            XmlDocument document = processor.RenderToXml();

            XsltArgumentList arguments = new XsltArgumentList();
            arguments.AddParam("result-kind", "", resultKind);

            _renderer.Write(document, outputTo, arguments);
        }

        public string RenderFragmentStyles()
        {
            XmlDocument document = new XmlDocument();
            document.XmlResolver = null;

            document.AppendChild(document.CreateElement("fragment-styles"));

            XsltArgumentList arguments = new XsltArgumentList();
            arguments.AddParam("result-kind", "", "fragment");

            XmlDocument resultDocument = _renderer.Transform(document, arguments);

            return resultDocument.DocumentElement.InnerText;
        }

        public void RenderFragmentStyles(TextWriter writer)
        {
            writer.Write(RenderFragmentStyles());
        }
    }
}
