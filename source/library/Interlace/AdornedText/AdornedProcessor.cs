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

using Interlace.AdornedRendering;
using Interlace.Utilities;

#endregion

namespace Interlace.AdornedText
{
    public class AdornedProcessor
    {
        Section _topSection = null;

        public AdornedProcessor()
        {
        }

        public static Section ParseToDom(TextReader reader)
        {
            LineClassifier classifier = new LineClassifier(reader);
            Parser parser = new Parser(classifier);

            return parser.Parse();
        }

        public void Parse(TextReader reader)
        {
            if (_topSection != null)
            {
                throw new InvalidOperationException(AdornedTextStrings.AdornedProcessorAlreadyParsed);
            }

            LineClassifier classifier = new LineClassifier(reader);
            Parser parser = new Parser(classifier);

            try
            {
                _topSection = parser.Parse();
            }
            catch (AdornedTextParsingException ex)
            {
                _topSection = GenerateErrorDocument(ex, classifier);
            }
        }

        static Section GenerateErrorDocument(Exception ex, LineClassifier classifier)
        {
            ParseErrorSection topSection = new ParseErrorSection();

            Paragraph paragraph = new Paragraph(new FormattedSpan(FormattedSpanKind.Bold, new TextSpan("I can haz fixed?")));
            topSection.Blocks.Add(paragraph);

            LineClassifierContext context = classifier.GetContext();

            List<string> lines = new List<string>();

            foreach (Pair<int, string> pair in context.AllLines)
            {
                lines.Add(string.Format("{0} {1}: {2}",
                    pair.First == context.CurrentLine.First ? ">" : " ",
                    pair.First.ToString().PadLeft(4, ' '),
                    pair.Second));
            }

            VerbatimBlock linesBlock = new VerbatimBlock();
            linesBlock.Lines.AddRange(lines);

            topSection.Blocks.Add(linesBlock);

            return topSection;
        }

        public void Parse(string document)
        {
            using (StringReader reader = new StringReader(document))
            {
                Parse(reader);
            }
        }

        public void Assign(Section document)
        {
            if (_topSection != null)
            {
                throw new InvalidOperationException(AdornedTextStrings.AdornedProcessorAlreadyParsed);
            }

            _topSection = document;
        }

        public void Preprocess(IAdornedPreprocessor preprocessor)
        {
            preprocessor.Preprocess(_topSection);
        }

        public XmlDocument RenderToXml()
        {
            if (_topSection == null) throw new InvalidOperationException(AdornedTextStrings.AdornedProcessorNotParsed);

            XmlRenderer renderer = new XmlRenderer();
            return renderer.Render(_topSection);
        }

        public void ResolveReferences(IAdornedReferenceResolver resolver)
        {
            if (_topSection == null) throw new InvalidOperationException(AdornedTextStrings.AdornedProcessorNotParsed);

            // Find all inlines:
            InlineListBuildingVisitor inlineVisitor = new InlineListBuildingVisitor();
            _topSection.Visit(inlineVisitor);

            foreach (InlineBlock inline in inlineVisitor.Inlines)
            {
                Uri replacement = resolver.ResolveInline(inline.ContentType, inline.Text);

                int inlineIndex = inline.Parent.Blocks.IndexOf(inline);

                if (replacement == null)
                {
                    inline.Parent.Blocks.RemoveAt(inlineIndex);
                }
                else
                {
                    Paragraph paragraph = new Paragraph(
                        new ReferenceSpan("image", replacement.ToString(), null));

                    inline.Parent.Blocks[inlineIndex] = paragraph;
                }
            }

            // Find all references within the document:
            ReferenceListBuildingVisitor referenceVisitor = new ReferenceListBuildingVisitor();
            _topSection.Visit(referenceVisitor);

            List<ReferenceSpan> references = referenceVisitor.References;

            foreach (ReferenceSpan reference in references)
            {
                try
                {
                    Uri referenceUri = new Uri(reference.Reference);

                    if (referenceUri.Scheme == "adorned")
                    {
                        Uri newUri = resolver.ResolveReference(referenceUri);

                        if (newUri != null) reference.Reference = newUri.ToString();
                    }
                }
                catch (UriFormatException ex)
                {
                    reference.ResolutionException = ex;
                }
                catch (AdornedReferenceResolutionException ex)
                {
                    reference.ResolutionException = ex;
                }
            }
        }
    }
}
