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
using System.Drawing;
using Interlace.Drawing;

#endregion

namespace Interlace.Controls
{
    public class TextConsoleRow : ConsoleRow
    {
        // Content fields:
        string _text;

        int _leftPadding = 15;
        int _rightPadding = 15;

        // Estimation fields:
        int _fullWidthForEstimation;
        int _heightForEstimation;

        // Layout fields:
        int _layoutForWidth = -1;

        int _layoutHeight;
        string[] _lines;
        int[] _lineHeights;

        public TextConsoleRow(string text)
        {
            _text = text ?? "";
        }

        public override void Initialize(DirectText directText, Font defaultFont)
        {
            Size size = directText.MeasureString(_text, defaultFont);

            _fullWidthForEstimation = size.Width;
            _heightForEstimation = size.Height;
        }

        public override int GetEstimatedHeight(int width)
        {
            if (width <= 0) return 0;

            int lines = _fullWidthForEstimation / width;

            if (_fullWidthForEstimation % width != 0) lines++;

            return lines * _heightForEstimation;
        }

        public override int GetHeight(DirectText directText, Font defaultFont, int width)
        {
            if (_layoutForWidth != width)
            {
                _layoutForWidth = width;

                List<string> lines = new List<string>();
                List<int> lineHeights = new List<int>();

                _layoutHeight = 0;

                string remaining = _text;

                int availableWidth = width - _leftPadding - _rightPadding;

                while (remaining.Length > 0)
                {
                    // Break the remaining text into a line:
                    int fittingCharacterCount = directText.MeasureFittingCharacterCount(remaining, availableWidth, defaultFont);
                    fittingCharacterCount = Math.Max(1, fittingCharacterCount);

                    string line = remaining.Substring(0, fittingCharacterCount);
                    remaining = remaining.Substring(fittingCharacterCount);

                    lines.Add(line);

                    // Add the line height:
                    Size size = directText.MeasureString(line, defaultFont);

                    lineHeights.Add(size.Height);

                    _layoutHeight += size.Height;
                }

                _lines = lines.ToArray();
                _lineHeights = lineHeights.ToArray();
            }

            return _layoutHeight;
        }

        public override void Paint(Graphics graphics, DirectText directText, Font defaultFont, int initialTop, int width)
        {
            graphics.FillRectangle(SystemBrushes.Window, new Rectangle(0, initialTop, width, _layoutHeight));

            int top = initialTop;

            for (int i = 0; i < _lines.Length; i++)
            {
                directText.DrawString(graphics, _lines[i], defaultFont, SystemColors.WindowText, _leftPadding, top);

                top += _lineHeights[i];
            }
        }
    }
}
