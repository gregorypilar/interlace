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
using System.Text.RegularExpressions;
using System.Xml;

#endregion

namespace Interlace.AdornedText
{
    class InDesignRenderer
    {
        public XmlDocument Render(Section rootSection)
        {
            XmlDocument document = new XmlDocument();
            document.XmlResolver = null;

            XmlElement rootElement = document.CreateElement("document");
            document.AppendChild(rootElement);

            RenderSection(rootSection, rootElement, 0);

            return document;
        }

        void RenderSection(Section section, XmlElement container, int parentSections)
        {
            XmlElement sectionElement = container.OwnerDocument.CreateElement(string.Format("section-{0}", parentSections + 1));
            container.AppendChild(sectionElement);

            XmlElement titleElement = container.OwnerDocument.CreateElement(string.Format("heading-{0}", parentSections + 1));

            AddText(titleElement, section.Title);

            sectionElement.AppendChild(titleElement);

            RenderBlockSequenceInto(section, sectionElement, parentSections + 1);
        }

        void RenderBlockSequenceInto(BlockSequence blockSequence, XmlElement container, int? parentSections)
        {
            foreach (Block block in blockSequence.Blocks)
            {
                RenderBlockInto(block, container, parentSections);
            }
        }

        void RenderBlockInto(Block block, XmlElement container, int? parentSections)
        {
            if (block is Paragraph) RenderParagraphInto(block as Paragraph, container);
            else if (block is Section) RenderSection(block as Section, container, parentSections.Value);
            else if (block is Listing) RenderListing(block as Listing, container);
            else if (block is VerbatimBlock) RenderVerbatimBlock(block as VerbatimBlock, container);
            else if (block is DefinitionListing) RenderDefinitionListing(block as DefinitionListing, container);
            else if (block is Table) RenderTable(block as Table, container);
        }

        void RenderParagraphInto(Paragraph paragraph, XmlElement container)
        {
            XmlElement paragraphElement = container.OwnerDocument.CreateElement("paragraph");
            container.AppendChild(paragraphElement);

            container.AppendChild(container.OwnerDocument.CreateTextNode("\n"));

            RenderSpanInto(paragraph.Span, paragraphElement);
        }

        void RenderListing(Listing listing, XmlElement container)
        {
            XmlElement listingElement = container.OwnerDocument.CreateElement("list");
            container.AppendChild(listingElement);

            foreach (ListingItem item in listing.Items)
            {
                string elementName = item.IsLast ? "list-item-last" : "list-item";

                XmlElement listingItemElement = container.OwnerDocument.CreateElement(elementName);
                listingElement.AppendChild(listingItemElement);

                RenderBlockSequenceInto(item, listingItemElement, null);
            }
        }

        void RenderDefinitionListing(DefinitionListing listing, XmlElement container)
        {
            XmlElement listingElement = container.OwnerDocument.CreateElement("definition-list");
            container.AppendChild(listingElement);

            foreach (DefinitionListItem item in listing.Items)
            {
                XmlElement listingItemElement = container.OwnerDocument.CreateElement("definition-list-item");
                listingElement.AppendChild(listingItemElement);

                listingItemElement.SetAttribute("term", item.Term);

                RenderBlockSequenceInto(item, listingItemElement, null);
            }
        }

        void RenderVerbatimBlock(VerbatimBlock block, XmlElement container)
        {
            XmlElement blockElement = container.OwnerDocument.CreateElement("verbatim-block");
            container.AppendChild(blockElement);

            XmlText blockTextNode = container.OwnerDocument.CreateTextNode(block.Text);
            blockElement.AppendChild(blockTextNode);

            container.AppendChild(container.OwnerDocument.CreateTextNode("\n"));
        }

        void RenderSpanInto(Span span, XmlElement container)
        {
            if (span is TextSpan) 
            { 
                container.AppendChild(container.OwnerDocument.CreateTextNode(StripAllWhitespace((span as TextSpan).Text)));
            } 
            else if (span is SequenceSpan) 
            {
                foreach (Span childSpan in (span as SequenceSpan).Spans)
                {
                    RenderSpanInto(childSpan, container);
                }
            }
            else if (span is FormattedSpan)
            {
                FormattedSpan formattedSpan = span as FormattedSpan;

                string containerName;

                switch (formattedSpan.Kind)
                {
                    case FormattedSpanKind.Code:
                        containerName = "span-code";
                        break;

                    case FormattedSpanKind.Bold:
                        containerName = "span-bold";
                        break;

                    case FormattedSpanKind.Italic:
                        containerName = "span-italic";
                        break;

                    case FormattedSpanKind.Underline:
                        containerName = "span-underline";
                        break;

                    default:
                        containerName = "span-unknown";
                        break;
                }

                XmlElement formattedContainer = container.OwnerDocument.CreateElement(containerName);
                container.AppendChild(formattedContainer);

                RenderSpanInto(formattedSpan.ChildSpan, formattedContainer);
            }
            else if (span is ReferenceSpan)
            {
                RenderReference(span as ReferenceSpan, container);
            }
        }

        private void RenderReference(ReferenceSpan referenceSpan, XmlElement container)
        {
            if (referenceSpan.ResolutionException == null)
            {
                XmlElement referenceContainer = container.OwnerDocument.CreateElement("reference");
                container.AppendChild(referenceContainer);

                switch (referenceSpan.Kind)
                {
                    case ReferenceSpanKind.Link:
                        referenceContainer.SetAttribute("kind", "link");
                        break;

                    case ReferenceSpanKind.Image:
                        referenceContainer.SetAttribute("kind", "image");
                        break;
                }

                referenceContainer.SetAttribute("to", referenceSpan.Reference);

                if (referenceSpan.ChildSpan != null)
                {
                    RenderSpanInto(referenceSpan.ChildSpan, referenceContainer);
                }
            }
            else
            {
                XmlElement referenceContainer = container.OwnerDocument.CreateElement("reference-exception");
                container.AppendChild(referenceContainer);

                referenceContainer.SetAttribute("to", referenceSpan.Reference);
                referenceContainer.SetAttribute("message", referenceSpan.ResolutionException.Message);

                if (referenceSpan.ResolutionException is AdornedReferenceResolutionException)
                {
                    AdornedReferenceResolutionException ex =
                        referenceSpan.ResolutionException as AdornedReferenceResolutionException;

                    if (ex.ConsoleOutput != null)
                    {
                        string consoleText = ex.ConsoleOutput;

                        XmlElement consoleOutput = container.OwnerDocument.CreateElement("console-output");
                        referenceContainer.AppendChild(consoleOutput);

                        foreach (string line in consoleText.Split('\n'))
                        {
                            XmlElement lineElement = container.OwnerDocument.CreateElement("console-output-line");
                            consoleOutput.AppendChild(lineElement);

                            lineElement.AppendChild(container.OwnerDocument.CreateTextNode(line.TrimEnd()));
                        }
                    }
                }
            }
        }

        void RenderTable(Table block, XmlElement container)
        {
            XmlElement tableElement = container.OwnerDocument.CreateElement("table");
            container.AppendChild(tableElement);

            foreach (TableRow row in block.Rows)
            {
                XmlElement tableRowElement = container.OwnerDocument.CreateElement("table-row");
                tableElement.AppendChild(tableRowElement);

                foreach (TableCell cell in row.Cells)
                {
                    XmlElement tableCellElement = container.OwnerDocument.CreateElement("table-cell");
                    tableRowElement.AppendChild(tableCellElement);

                    RenderBlockSequenceInto(cell, tableCellElement, null);
                }
            }
        }

        static Regex _whitespaceRegex = new Regex(@"\s+");

        static string StripAllWhitespace(string paragraph)
        {
            if (paragraph == null) return "";

            return _whitespaceRegex.Replace(paragraph, " ");
        }

        static void AddText(XmlElement container, string text)
        {
            container.AppendChild(container.OwnerDocument.CreateTextNode(StripAllWhitespace(text) + "\n"));
        }
    }
}
