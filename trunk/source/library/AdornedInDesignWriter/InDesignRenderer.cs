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
    class InDesignRenderer : XmlBuilder
    {
        int _sectionDepth = 0;

        public InDesignRenderer()
        : base("document")
        {
        }

        public void Render(Section rootSection)
        {
            RenderSection(rootSection);
        }

        void RenderSection(Section section)
        {
            int titleDepth = _sectionDepth;

            _sectionDepth++;

            Push("section");

            if (section.Title != null)
            {
                WriteElement(string.Format("title-{0}", titleDepth), section.Title);
                WriteNewLine();
            }

            RenderBlockSequence(section);

            Pop();

            _sectionDepth--;
        }

        void RenderBlockSequence(BlockSequence blockSequence)
        {
            foreach (Block block in blockSequence.Blocks)
            {
                RenderBlock(block);
            }
        }

        void RenderBlock(Block block)
        {
            if (block is Paragraph) RenderParagraph(block as Paragraph);
            else if (block is Section) RenderSection(block as Section);
            else if (block is Listing) RenderListing(block as Listing);
            else if (block is VerbatimBlock) RenderVerbatimBlock(block as VerbatimBlock);
            else if (block is DefinitionListing) RenderDefinitionListing(block as DefinitionListing);
            else if (block is Table) RenderTable(block as Table);
            else if (block is ErrorBlock) RenderErrorBlock(block as ErrorBlock);
        }

        void RenderParagraph(Paragraph paragraph)
        {
            Push("paragraph");

            RenderSpan(paragraph.Span);

            Pop();

            Write(Environment.NewLine);
        }

        void RenderListing(Listing listing)
        {
            Push("listing");

            foreach (ListingItem item in listing.Items)
            {
                Push("list-item");

                RenderBlockSequence(item);

                Pop();
            }

            Pop();
        }

        void RenderDefinitionListing(DefinitionListing listing)
        {
            Push("definition-list");

            foreach (DefinitionListItem item in listing.Items)
            {
                Push("definition-list-item");

                WriteElement("term", item.Term);
                WriteNewLine();

                RenderBlockSequence(item);

                Pop();
            }

            Pop();
        }

        void RenderVerbatimBlock(VerbatimBlock block)
        {
            Push("verbatim-block-prelude");
            Pop();
            WriteNewLine();

            Push("verbatim-block");

            Write(block.Text);

            Pop();

            WriteNewLine();

            Push("verbatim-block-postlude");
            Pop();
            WriteNewLine();
        }

        void RenderSpan(Span span)
        {
            if (span is TextSpan) 
            {
                Write((span as TextSpan).Text);
            } 
            else if (span is SequenceSpan) 
            {
                foreach (Span childSpan in (span as SequenceSpan).Spans)
                {
                    RenderSpan(childSpan);
                }
            }
            else if (span is FormattedSpan)
            {
                FormattedSpan formattedSpan = span as FormattedSpan;

                string spanElementName;

                switch (formattedSpan.Kind)
                {
                    case FormattedSpanKind.Code:
                        spanElementName = "span-code";
                        break;

                    case FormattedSpanKind.Bold:
                        spanElementName = "span-bold";
                        break;

                    case FormattedSpanKind.Italic:
                        spanElementName = "span-italic";
                        break;

                    case FormattedSpanKind.Underline:
                        spanElementName = "span-underline";
                        break;

                    default:
                        spanElementName = "span-unknown";
                        break;
                }

                Push(spanElementName);

                RenderSpan(formattedSpan.ChildSpan);

                Pop();
            }
            else if (span is ReferenceSpan)
            {
                RenderReference(span as ReferenceSpan);
            }
        }

        private void RenderReference(ReferenceSpan referenceSpan)
        {
            if (referenceSpan.ResolutionException == null)
            {
                Push("reference");

                switch (referenceSpan.KindTag)
                {
                    case "link":
                        SetAttribute("kind", "link");
                        break;

                    case "image":
                        SetAttribute("kind", "image");
                        break;
                }

                SetAttribute("to", referenceSpan.Reference);

                if (referenceSpan.ChildSpan != null)
                {
                    RenderSpan(referenceSpan.ChildSpan);
                }

                Pop();
            }
            else
            {
                Push("reference-exception");

                SetAttribute("to", referenceSpan.Reference);
                SetAttribute("message", referenceSpan.ResolutionException.Message);

                if (referenceSpan.ResolutionException is AdornedReferenceResolutionException)
                {
                    AdornedReferenceResolutionException ex =
                        referenceSpan.ResolutionException as AdornedReferenceResolutionException;

                    if (ex.ConsoleOutput != null)
                    {
                        string consoleText = ex.ConsoleOutput;

                        Push("console-output");

                        foreach (string line in consoleText.Split('\n'))
                        {
                            Push("console-output-line");

                            Write(line.TrimEnd());

                            Pop();

                            WriteNewLine();
                        }

                        Pop();
                    }
                }

                Pop();
            }
        }

        void RenderTable(Table block)
        {
            Push("table");

            foreach (TableRow row in block.Rows)
            {
                Push("table-row");

                foreach (TableCell cell in row.Cells)
                {
                    Push("table-cell");

                    RenderBlockSequence(cell);

                    Pop();
                }

                Pop();
            }

            Pop();
        }

        void RenderErrorBlock(ErrorBlock errorBlock)
        {
            Push("error-block");
            Push("message");

            foreach (string line in errorBlock.Message.Split('\n'))
            {
                Push("message-line");
                Write(line.TrimEnd());
                Pop();
            }

            Pop();
            Pop();
        }
    }
}
