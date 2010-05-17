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
using System.Diagnostics;
using System.Text;

#endregion

namespace Interlace.AdornedText
{
    class Parser
    {
        LineClassifier _classifier;
        SpanParser _spanParser = new SpanParser();

        public Parser(LineClassifier classifier)
        {
            _classifier = classifier;
        }

        public Section Parse()
        {
            Section section = new Section(null, 0);
            ParseSection(section);

            return section;
        }

        void ParseSection(Section section)
        {
            while (!_classifier.AtEnd)
            {
                Line line = _classifier.PeekLine();

                switch (line.Classification)
                {
                    case LineClassification.Unclassified:
                        section.Blocks.Add(ParseParagraph());
                        break;

                    case LineClassification.Blank:
                    case LineClassification.Comment:
                        // Blank lines in sections and comments are ignored:
                        _classifier.ReadLine();
                        break;

                    case LineClassification.Title:
                    case LineClassification.NumberedTitle:
                        // A lower or same level section title means this section is ending:
                        if (line.ClassificationLevel <= section.TitleLevel) return;

                        // Otherwise, it's a sub-section:
                        section.Blocks.Add(ParseSectionStart());
                        break;

                    case LineClassification.ListItem:
                    case LineClassification.NumeredListItem:
                        section.Blocks.Add(ParseList());
                        break;

                    case LineClassification.DefinitionListTerm:
                        section.Blocks.Add(ParseDefinitionList());
                        break;

                    case LineClassification.SingleVerbatimBlockLine:
                        section.Blocks.Add(ParseSingleVerbatimLine());
                        break;

                    case LineClassification.VerbatimBlockLine:
                        section.Blocks.Add(ParseVerbatimBlock());
                        break;

                    case LineClassification.TableOpen:
                        section.Blocks.Add(ParseTable());
                        break;

                    case LineClassification.InlineOpen:
                        section.Blocks.Add(ParseInline());
                        break;

                    default:
                        throw new AdornedTextParsingException(AdornedTextStrings.AdornedInternalUnexpected);
                }
            }
        }

        Paragraph ParseParagraph()
        {
            List<string> paragraphLines = new List<string>();
            return ParseParagraph(paragraphLines);
        }

        Paragraph ParseParagraph(List<string> continueFrom)
        {
            while (true)
            {
                Line line = _classifier.PeekLine();

                switch (line.Classification)
                {
                    case LineClassification.Unclassified:
                        continueFrom.Add(line.Text);

                        _classifier.ReadLine();
                        break;

                    case LineClassification.Comment:
                        _classifier.ReadLine();
                        break;

                    default:
                        return new Paragraph(_spanParser.Parse(continueFrom));
                }
            }
        }

        Section ParseSectionStart()
        {
            Line titleLine = _classifier.ReadLine();

            Debug.Assert(titleLine.Classification == LineClassification.Title ||
                         titleLine.Classification == LineClassification.NumberedTitle);

            Section section = new Section(titleLine.Text, titleLine.ClassificationLevel);

            ParseSection(section);

            return section;
        }

        Listing ParseList()
        {
            Listing listing = new Listing();

            Line firstLine = _classifier.PeekLine();

            Debug.Assert(firstLine.Classification == LineClassification.ListItem ||
                         firstLine.Classification == LineClassification.NumeredListItem);

            int indentLevel = firstLine.ClassificationLevel;

            while (!_classifier.AtEnd)
            {
                ListingItem item = ParseListLine();
                listing.Items.Add(item);

                Line line = _classifier.PeekLine();

                // See what the item parser ended on:
                switch (line.Classification)
                {
                    case LineClassification.Comment:
                        _classifier.ReadLine();
                        break;

                    case LineClassification.TableRowSeparator:
                    case LineClassification.TableCellSeparator:
                    case LineClassification.TableClose:
                    case LineClassification.EndOfStream:
                    case LineClassification.Blank:
                        // (A blank line here means a double blank was seen; all lists end)

                        return listing;

                    case LineClassification.ListItem:
                    case LineClassification.NumeredListItem:
                        Debug.Assert(line.ClassificationLevel <= indentLevel);

                        // If the list de-dents, this list is finished:
                        if (line.ClassificationLevel < indentLevel) return listing;

                        // Otherwise, continue parsing lists:
                        break;
                        
                    case LineClassification.Unclassified:
                    case LineClassification.VerbatimBlockLine:
                    case LineClassification.SingleVerbatimBlockLine:
                    case LineClassification.Title:
                    case LineClassification.NumberedTitle:
                    case LineClassification.DefinitionListTerm:
                    default:
                        throw new AdornedTextParsingException(AdornedTextStrings.UnexpectedLineClassificationInList);
                }
            }

            return listing;
        }

        ListingItem ParseListLine()
        {
            ListingItem listingItem = new ListingItem();

            // Parse the first paragraph, which must be present (and includes the list line itself):
            Line firstLine = _classifier.ReadLine();

            Debug.Assert(firstLine.Classification == LineClassification.ListItem ||
                         firstLine.Classification == LineClassification.NumeredListItem);

            List<string> paragraphLines = new List<string>();
            paragraphLines.Add(firstLine.Text);

            listingItem.Blocks.Add(ParseParagraph(paragraphLines));

            // Now parse the rest:
            while (!_classifier.AtEnd)
            {
                Line line = _classifier.PeekLine();

                switch (line.Classification)
                {
                    case LineClassification.Unclassified:
                        listingItem.Blocks.Add(ParseParagraph());
                        break;

                    case LineClassification.Comment:
                        _classifier.ReadLine();
                        break;

                    case LineClassification.Blank:
                        _classifier.ReadLine();

                        // Ignore single lines; but not double lines:
                        Line followingLine = _classifier.PeekLine();

                        if (followingLine.Classification == LineClassification.Blank)
                        {
                            return listingItem;
                        }
                        break;

                    case LineClassification.TableRowSeparator:
                    case LineClassification.TableCellSeparator:
                    case LineClassification.TableClose:
                        return listingItem;

                    case LineClassification.ListItem:
                    case LineClassification.NumeredListItem:
                        // Parse a nested list item; same level or lower ones mean we're ending this line:
                        if (line.ClassificationLevel > firstLine.ClassificationLevel)
                        {
                            listingItem.Blocks.Add(ParseList());

                            Line terminatingLine = _classifier.PeekLine();

                            if (terminatingLine.Classification == LineClassification.Blank) return listingItem;
                        }
                        else
                        {
                            return listingItem;
                        }

                        break;

                    case LineClassification.SingleVerbatimBlockLine:
                        listingItem.Blocks.Add(ParseSingleVerbatimLine());
                        break;

                    case LineClassification.VerbatimBlockLine:
                        listingItem.Blocks.Add(ParseVerbatimBlock());
                        break;

                    case LineClassification.TableOpen:
                        listingItem.Blocks.Add(ParseTable());
                        break;

                    case LineClassification.Title:
                        throw new AdornedTextParsingException(AdornedTextStrings.TitleInListIsInvalid);

                    case LineClassification.NumberedTitle:
                        throw new AdornedTextParsingException(AdornedTextStrings.TitleInListIsInvalid);

                    case LineClassification.DefinitionListTerm:
                        throw new AdornedTextParsingException(AdornedTextStrings.DefinitionListInListIsInvalid);

                    default:
                        throw new AdornedTextParsingException(AdornedTextStrings.AdornedInternalUnexpected);
                }
            }

            return listingItem;
        }

        DefinitionListing ParseDefinitionList()
        {
            DefinitionListing listing = new DefinitionListing();

            while (!_classifier.AtEnd)
            {
                // Read the term line:
                Line definitionLine = _classifier.PeekLine();

                if (definitionLine.Classification != LineClassification.DefinitionListTerm)
                {
                    Debug.Assert(listing.Items.Count != 0);

                    return listing;
                }

                DefinitionListItem listingItem = new DefinitionListItem(definitionLine.Text);

                _classifier.ReadLine();

                ParseDefinitionListingItemContents(listingItem);

                listing.Items.Add(listingItem);
            }

            return listing;
        }

        void ParseDefinitionListingItemContents(DefinitionListItem listingItem)
        {
            while (!_classifier.AtEnd)
            {
                Line line = _classifier.PeekLine();

                switch (line.Classification)
                {
                    case LineClassification.Unclassified:
                        listingItem.Blocks.Add(ParseParagraph());
                        break;

                    case LineClassification.Comment:
                        _classifier.ReadLine();
                        break;

                    case LineClassification.Blank:
                        _classifier.ReadLine();

                        // Ignore single lines; but not double lines:
                        Line followingLine = _classifier.PeekLine();

                        if (followingLine.Classification == LineClassification.Blank) return;

                        break;

                    case LineClassification.ListItem:
                    case LineClassification.NumeredListItem:
                        throw new AdornedTextParsingException(AdornedTextStrings.ListInDefinitionListIsInvalid);

                    case LineClassification.SingleVerbatimBlockLine:
                        listingItem.Blocks.Add(ParseSingleVerbatimLine());
                        break;

                    case LineClassification.VerbatimBlockLine:
                        listingItem.Blocks.Add(ParseVerbatimBlock());
                        break;

                    case LineClassification.TableOpen:
                        listingItem.Blocks.Add(ParseTable());
                        break;

                    case LineClassification.Title:
                        throw new AdornedTextParsingException(AdornedTextStrings.TitleInListIsInvalid);

                    case LineClassification.NumberedTitle:
                        throw new AdornedTextParsingException(AdornedTextStrings.TitleInListIsInvalid);

                    case LineClassification.DefinitionListTerm:
                        return;

                    default:
                        throw new AdornedTextParsingException(AdornedTextStrings.AdornedInternalUnexpected);
                }
            }
        }

        VerbatimBlock ParseSingleVerbatimLine()
        {
            Line verbatimLine = _classifier.ReadLine();

            Debug.Assert(verbatimLine.Classification == LineClassification.SingleVerbatimBlockLine);

            VerbatimBlock block = new VerbatimBlock();
            block.Lines.Add(verbatimLine.Text);

            return block;
        }

        VerbatimBlock ParseVerbatimBlock()
        {
            VerbatimBlock block = new VerbatimBlock();

            while (true)
            {
                Line line = _classifier.PeekLine();

                if (line.Classification != LineClassification.VerbatimBlockLine)
                {
                    Debug.Assert(block.Lines.Count != 0);

                    return block;
                }

                _classifier.ReadLine();

                block.Lines.Add(line.Text);
            }
        }

        Table ParseTable()
        {
            Line firstLine = _classifier.ReadLine();

            Debug.Assert(firstLine.Classification == LineClassification.TableOpen);

            TableBuilder builder = new TableBuilder();
            Table table = builder.Table;

            while (!_classifier.AtEnd)
            {
                Line line = _classifier.PeekLine();

                switch (line.Classification)
                {
                    case LineClassification.TableCellSeparator:
                        _classifier.ReadLine();
                        builder.CellSeparated();
                        break;

                    case LineClassification.TableRowSeparator:
                        _classifier.ReadLine();
                        builder.RowSeparated();
                        break;

                    case LineClassification.Unclassified:
                        builder.CurrentCell.Blocks.Add(ParseParagraph());
                        break;

                    case LineClassification.Blank:
                    case LineClassification.Comment:
                        // Blank lines in sections and comments are ignored:
                        _classifier.ReadLine();
                        break;

                    case LineClassification.ListItem:
                    case LineClassification.NumeredListItem:
                        builder.CurrentCell.Blocks.Add(ParseList());
                        break;

                    case LineClassification.DefinitionListTerm:
                        builder.CurrentCell.Blocks.Add(ParseDefinitionList());
                        break;

                    case LineClassification.SingleVerbatimBlockLine:
                        builder.CurrentCell.Blocks.Add(ParseSingleVerbatimLine());
                        break;

                    case LineClassification.VerbatimBlockLine:
                        builder.CurrentCell.Blocks.Add(ParseVerbatimBlock());
                        break;

                    case LineClassification.Title:
                    case LineClassification.NumberedTitle:
                    case LineClassification.TableOpen:
                        throw new AdornedTextParsingException(AdornedTextStrings.TitlesAndTablesInTableAreInvalid);

                    case LineClassification.TableClose:
                        _classifier.ReadLine();
                        return table;

                    default:
                        throw new AdornedTextParsingException(AdornedTextStrings.AdornedInternalUnexpected);
                }
            }

            // A table closing tag is guaranteed by the line classifier:
            throw new AdornedTextParsingException(AdornedTextStrings.AdornedInternalUnexpected);
        }

        InlineBlock ParseInline()
        {
            Line firstLine = _classifier.ReadLine();

            Debug.Assert(firstLine.Classification == LineClassification.InlineOpen);

            InlineBlock block = new InlineBlock(firstLine.Text);

            while (true)
            {
                Line line = _classifier.PeekLine();

                if (line.Classification != LineClassification.InlineLine)
                {
                    return block;
                }

                _classifier.ReadLine();

                block.Lines.Add(line.Text);
            }
        }
    }
}

