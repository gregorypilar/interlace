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

#endregion

namespace Interlace.AdornedText
{
    class LineClassifier
    {
        static Regex _blankRegex = new Regex(@"^\s*$");
        static Regex _titleRegex = new Regex(@"^\s*(=+)\s+([^=]+)\s+\1\s*$");
        static Regex _numberedTitleRegex = new Regex(@"^\s*(\++)\s+([^=]+)\s+\1\s*$");
        static Regex _commentRegex = new Regex(@"^%(.*)$");
        static Regex _listItemRegex = new Regex(@"^(\s*)-\s+(.*)$");
        static Regex _numberedListItemRegex = new Regex(@"^(\s*)\+\s+(.*)$");
        static Regex _definitionListTermRegex = new Regex(@"^:\s+(.*)$");
        static Regex _verbatimBlockLineRegex = new Regex(@"^```(.*)```\s*$");
        static Regex _verbatimBlockMarkerRegex = new Regex(@"^```\s*$");

        static Regex _tableOpenMarkerRegex = new Regex(@"^\[\[\[\s*$");
        static Regex _tableCloseMarkerRegex = new Regex(@"^\]\]\]\s*$");

        static Regex _inlineOpenMarkerRegex = new Regex(@"^([a-z]+){{{\s*$");
        static Regex _inlineCloseMarkerRegex = new Regex(@"^}}}\s*$");

        // Matches either:
        //
        // An end-of-table marker (group 1).
        // A line starting with a marker (group 2) followed by the remainder (group 3).
        // A line starting with other text (group 4) up to some marker and trailing text (group 5).
        // A line with no markers (group 6).
        static Regex _tableMarkerRegex = new Regex(@"^(?:(\]\]\]\s*)|(\|\||\|-)(.*)|(.+?)((?:\|\||\|-).*)|(.*))$");

        TextReader _reader;

        bool _hasPeeked;
        Line _peeked;

        bool _readingVerbatimLines;
        bool _readingInlineLines;

        bool _readingTable;
        string _partiallyReadTableLine;

        int _lineNumber;

        bool _invalidated;

        int _contextLineCount;
        Queue<string> _previousLines;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineClassifier"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public LineClassifier(TextReader reader)
        {
            _reader = reader;
            _lineNumber = 0;

            _invalidated = false;
            _contextLineCount = 5;
            _previousLines = new Queue<string>(_contextLineCount + 1);

            _hasPeeked = false;

            _readingVerbatimLines = false;
            _readingTable = false;
            _partiallyReadTableLine = null;
        }

        public bool AtEnd
        {
            get 
            {
                if (_invalidated) throw new InvalidOperationException();

                return PeekLine().Classification == LineClassification.EndOfStream; 
            }
        }

        string ReadLineFromReader()
        {
            string line = _reader.ReadLine();

            if (line != null)
            {
                _lineNumber++;

                _previousLines.Enqueue(line);

                while (_previousLines.Count > _contextLineCount + 1) _previousLines.Dequeue();
            }

            return line;
        }

        /// <summary>
        /// Peeks at the next line from the reader.
        /// </summary>
        /// <returns>The line if the end of the file has not been reached, otherwise null.</returns>
        public Line PeekLine()
        {
            if (_invalidated) throw new InvalidOperationException();

            if (_hasPeeked) 
            {
                return _peeked;
            }
            else
            {
                _peeked = ReadLine();

                // Must be set after calling ReadLine:
                _hasPeeked = true;

                return _peeked;
            }
        }

        /// <summary>
        /// Reads and classifies from the next line from the reader.
        /// </summary>
        /// <returns>The line if the end of the file has not been reached, otherwise null.</returns>
        public Line ReadLine()
        {
            if (_invalidated) throw new InvalidOperationException();

            if (_hasPeeked) 
            {
                _hasPeeked = false;

                return _peeked;
            }

            Line line;

            do
            {
                if (!_readingTable)
                {
                    string lineText = ReadLineFromReader();

                    if (lineText != null)
                    {
                        line = ClassifyLine(lineText);
                    }
                    else
                    {
                        line = new Line(string.Empty, LineClassification.EndOfStream, 0);
                    }
                }
                else
                {
                    if (_partiallyReadTableLine == null)
                    {
                        _partiallyReadTableLine = ReadLineFromReader();
                        _lineNumber++;

                        if (_partiallyReadTableLine == null)
                        {
                            throw new AdornedTextParsingException(AdornedTextStrings.EndOfStreamInTable);
                        }
                    }

                    line = ClassifyTableLine(ref _partiallyReadTableLine);
                }
            } while (line.Classification == LineClassification.Discarded);

            return line;
        }

        Line ClassifyLine(string line)
        {
            // First check for toggling of verbatim lines:
            Match verbatimBlockMarkerMatch = _verbatimBlockMarkerRegex.Match(line);

            if (verbatimBlockMarkerMatch.Success)
            {
                _readingVerbatimLines = !_readingVerbatimLines;

                return new Line(string.Empty, LineClassification.Discarded, 0);
            }

            // Then, if we're in verbatim lines, return one:
            if (_readingVerbatimLines)
            {
                return new Line(line, LineClassification.VerbatimBlockLine, 0);
            }

            // Handle inlines:
            if (_readingInlineLines)
            {
                Match inlineCloseMarkerMatch = _inlineCloseMarkerRegex.Match(line);

                if (inlineCloseMarkerMatch.Success)
                {
                    _readingInlineLines = false;

                    return new Line(string.Empty, LineClassification.Discarded, 0);
                }

                return new Line(line, LineClassification.InlineLine, 0);
            }

            Match inlineOpenMarkerMatch = _inlineOpenMarkerRegex.Match(line);

            if (inlineOpenMarkerMatch.Success)
            {
                _readingInlineLines = true;

                return new Line(inlineOpenMarkerMatch.Groups[1].Value, LineClassification.InlineOpen, 0);
            }

            // Otherwise, check for the other classifications:
            Match blankMatch = _blankRegex.Match(line);

            if (blankMatch.Success) return new Line(string.Empty, LineClassification.Blank, 0);

            Match titleMatch = _titleRegex.Match(line);

            if (titleMatch.Success) return new Line(titleMatch.Groups[2].Value, LineClassification.Title, 
                titleMatch.Groups[1].Length);

            Match numberedTitleMatch = _numberedTitleRegex.Match(line);

            if (numberedTitleMatch.Success) return new Line(titleMatch.Groups[2].Value, LineClassification.NumberedTitle, 
                titleMatch.Groups[1].Length);

            Match commentMatch = _commentRegex.Match(line);

            if (commentMatch.Success) return new Line(commentMatch.Groups[1].Value, 
                LineClassification.Comment, 0);

            Match listItemMatch = _listItemRegex.Match(line);

            if (listItemMatch.Success) return new Line(listItemMatch.Groups[2].Value, 
                LineClassification.ListItem, listItemMatch.Groups[1].Length);

            Match numberedListItemMatch = _numberedListItemRegex.Match(line);

            if (numberedListItemMatch.Success) return new Line(numberedListItemMatch.Groups[2].Value, 
                LineClassification.NumeredListItem, numberedListItemMatch.Groups[1].Length);

            Match definitionListTermMatch = _definitionListTermRegex.Match(line);

            if (definitionListTermMatch.Success) return new Line(definitionListTermMatch.Groups[1].Value, 
                LineClassification.DefinitionListTerm, 0);

            Match verbatimBlockLineMatch = _verbatimBlockLineRegex.Match(line);

            if (verbatimBlockLineMatch.Success) return new Line(verbatimBlockLineMatch.Groups[1].Value, 
                LineClassification.SingleVerbatimBlockLine, 0);

            Match tableOpenMarkerMatch = _tableOpenMarkerRegex.Match(line);

            if (tableOpenMarkerMatch.Success) 
            {
                _readingTable = true;

                return new Line(string.Empty, LineClassification.TableOpen, 0);
            }

            return new Line(line, LineClassification.Unclassified, 0);
        }

        Line ClassifyTableLine(ref string line)
        {
            Match tableMarkerMatch = _tableMarkerRegex.Match(line);

            if (tableMarkerMatch.Groups[2].Success)
            {
                // We're at a marker, with trailing text:
                string marker = tableMarkerMatch.Groups[2].Value;

                line = tableMarkerMatch.Groups[3].Value;
                if (line.Trim() == string.Empty) line = null;

                if (marker == "||")
                {
                    return new Line(string.Empty, LineClassification.TableCellSeparator, 0);
                }
                else 
                {
                    return new Line(string.Empty, LineClassification.TableRowSeparator, 0);
                }
            }
            else if (tableMarkerMatch.Groups[4].Success)
            {
                // We're at text with some marker in the future:
                line = tableMarkerMatch.Groups[5].Value;
                if (line.Trim() == string.Empty) line = null;

                string lineText = tableMarkerMatch.Groups[4].Value;

                return ClassifyLine(lineText);
            }
            else if (tableMarkerMatch.Groups[6].Success)
            {
                line = null;

                return ClassifyLine(tableMarkerMatch.Groups[6].Value);
            }
            else if (tableMarkerMatch.Groups[1].Success)
            {
                // End of table marker:
                line = null;

                _readingTable = false;

                return new Line(string.Empty, LineClassification.TableClose, 0);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public int LineNumber
        { 	 
            get { return _lineNumber; }
        }

        public LineClassifierContext GetContext()
        {
            if (_invalidated) throw new InvalidOperationException();

            _invalidated = true;

            string[] previousLines = new string[Math.Max(0, _previousLines.Count - 1)];
            string currentLine = "";

            for (int i = 0; i < previousLines.Length; i++)
            {
                previousLines[i] = _previousLines.Dequeue();
            }

            if (_previousLines.Count > 0) currentLine = _previousLines.Dequeue();

            List<string> followingLines = new List<string>();

            for (int i = 0; i < _contextLineCount; i++)
            {
                string line = _reader.ReadLine();

                if (line == null) break;

                followingLines.Add(line);
            }

            return new LineClassifierContext(previousLines, currentLine, _lineNumber, followingLines.ToArray());
        }
    }
}
