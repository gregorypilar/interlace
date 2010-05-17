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

using Interlace.Utilities;

#endregion

namespace Interlace.AdornedText
{
    class SpanParser
    {
        /// <summary>
        /// Either matches unformatted text (group 1) followed by a formatting
        /// token (group 2) followed by the inner formatted text (group 3), or
        /// unformatted text (group 4) followed by the contents of a link block (group 5), or
        /// it matches a block of unformatted text (group 6). Also, the pattern never
        /// fails to match a non-zero length string, and will always fail to match a 
        /// zero length string.
        /// </summary>
        static Regex _parserExpression = new Regex(@"\G(?:([^[]*?)(``|\*\*|//|__|" + "\"\"" + @")(.*?)\2|(.*?)\[(.*?)\]|(.+))");

        /// <summary>
        /// Matches a sequence of whitespace, usually to replace non-space and multiple character
        /// whitespace spans with a single space.
        /// </summary>
        static Regex _whitespaceExpression = new Regex(@"\s+");

        /// <summary>
        /// Matches the contents of a reference; the reference kind is in group 1, the
        /// reference in group 2, and any extra text in group 3.
        /// </summary>
        static Regex _referenceExpression = new Regex(@"^([a-z]+)\s+(\S+)\s*(.*)$");

        static Dictionary<string, FormattedSpanKind> _tokenToSpanKind;

        static SpanParser()
        {
            _tokenToSpanKind = new Dictionary<string, FormattedSpanKind>();
            _tokenToSpanKind["``"] = FormattedSpanKind.Code;
            _tokenToSpanKind["**"] = FormattedSpanKind.Bold;
            _tokenToSpanKind["//"] = FormattedSpanKind.Italic;
            _tokenToSpanKind["__"] = FormattedSpanKind.Underline;
        }

        public SpanParser()
        {
        }

        public string NormaliseParagraph(string paragraph)
        {
            return _whitespaceExpression.Replace(paragraph, " ").Trim();
        }

        public Span Parse(List<string> paragraphLines)
        {
            return Parse(string.Join(" ", paragraphLines.ToArray()));
        }

        public Span Parse(string paragraph)
        {
            string normalisedParagraph = NormaliseParagraph(paragraph);

            List<Span> matchedSpans = new List<Span>();

            Match match = _parserExpression.Match(normalisedParagraph);

            while (match.Success)
            {
                if (match.Groups[6].Success)
                {
                    // No formatting tokens found:
                    matchedSpans.Add(new TextSpan(match.Groups[6].Value));
                }
                else
                {
                    // Formatting tokens found, with possible text before them:
                    if (match.Groups[1].Success)
                    {
                        matchedSpans.Add(new TextSpan(match.Groups[1].Value));
                    }

                    if (match.Groups[4].Success)
                    {
                        matchedSpans.Add(new TextSpan(match.Groups[4].Value));
                    }

                    if (match.Groups[2].Success)
                    {
                        // A paired marker was found, for example **bold** and friends:
                        string formatToken = match.Groups[2].Value;
                        string formatText = match.Groups[3].Value;

                        if (formatToken == "\"\"")
                        {
                            // Raw text:
                            matchedSpans.Add(new TextSpan(formatText));
                        }
                        else
                        {
                            Span childSpan = Parse(formatText);

                            matchedSpans.Add(new FormattedSpan(_tokenToSpanKind[formatToken], childSpan));
                        }
                    }
                    else
                    {
                        // A reference marker was found:
                        string referenceContents = match.Groups[5].Value;

                        Match referenceMatch = _referenceExpression.Match(referenceContents);

                        if (!referenceMatch.Success)
                        {
                            throw new AdornedTextParsingException(AdornedTextStrings.InvalidReferenceContents);
                        }

                        string referenceKindTag = referenceMatch.Groups[1].Value;
                        string referenceString = referenceMatch.Groups[2].Value;
                        string referenceText = referenceMatch.Groups[3].Value;

                        matchedSpans.Add(new ReferenceSpan(referenceKindTag, referenceString, Parse(referenceText)));
                    }
                }

                match = match.NextMatch();
            }

            if (matchedSpans.Count == 0)
            {
                return new TextSpan("");
            }
            else if (matchedSpans.Count == 1)
            {
                return matchedSpans[0];
            }
            else
            {
                SequenceSpan sequenceSpan = new SequenceSpan();
                Functional.ApplyAdd(matchedSpans, sequenceSpan.Spans);

                return sequenceSpan;
            }
        }
    }
}
