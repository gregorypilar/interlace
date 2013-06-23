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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Interlace.Drawing;

#endregion

namespace Interlace.Controls
{
    public partial class ConsoleControl : Control
    {
        LinkedList<ConsoleRow> _rows = new LinkedList<ConsoleRow>();
        LinkedListNode<ConsoleRow> _offset = null;

        int _estimatedForWidth = -1;

        VScrollBar _scrollBar;
        int _scrollBarWidth = SystemInformation.VerticalScrollBarWidth;

        Font _defaultFont;

        int? _maximumRows;

        int _nextOrdinal = 0;

        bool _scrollToBottom = false;

        public ConsoleControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            _scrollBar = new VScrollBar();
            _scrollBar.Width = SystemInformation.VerticalScrollBarWidth;
            _scrollBar.Dock = DockStyle.Right;
            _scrollBar.Scroll += new ScrollEventHandler(_scrollBar_Scroll);

            _scrollBar.Parent = this;

            _defaultFont = CreateAvailableFont(10.0f, FontStyle.Regular, "Consolas", "Lucida Console", "Courier New");
        }

        void NonDesignerDispose()
        {
            if (_defaultFont != null)
            {
                _defaultFont.Dispose();
                _defaultFont = null;
            }
        }

        public Font CreateAvailableFont(float emSize, FontStyle fontStyle, params string[] familyNames)
        {
            Font font = null;

            foreach (string familyName in familyNames)
            {
                font = new Font(familyName, emSize, fontStyle);

                if (font.Name != familyName)
                {
                    font.Dispose();
                }
                else
                {
                    return font;
                }
            }

            return new Font(Font, Font.Style);
        }

        public int? MaximumRows
        {
            get { return _maximumRows; }
            set 
            { 
                _maximumRows = value;

                ApplyMaximumRows();
            }
        }

        void ApplyMaximumRows()
        {
            if (!_maximumRows.HasValue) return;

            int rowsToRemove = _rows.Count - _maximumRows.Value;

            int heightOfRemoved = 0;

            if (rowsToRemove > 0)
            {
                for (int i = 0; i < rowsToRemove; i++)
                {
                    if (_offset == _rows.First) _offset = _rows.First.Next;

                    _rows.RemoveFirst();

                    heightOfRemoved = _rows.First.Value.CumulativeEstimatedHeight;
                }
            }

            foreach (ConsoleRow row in _rows)
            {
                row.CumulativeEstimatedHeight -= heightOfRemoved;
            }
        }

        public void Append(ConsoleRow row)
        {
            row.Initialized = false;
            row.Ordinal = _nextOrdinal++;

            if (_rows.Last != null) row.CumulativeEstimatedHeight = _rows.Last.Value.CumulativeEstimatedHeight;

            _rows.AddLast(row);

            ApplyMaximumRows();

            if (_offset == null) _offset = _rows.First;

            _scrollToBottom = true;

            Invalidate();
        }

        public void AppendText(string text)
        {
            string[] lines = text.Split(new char[] { '\r', '\n' });

            foreach (string line in lines)
            {
                if (line.Trim().Length == 0) continue;

                Append(new TextConsoleRow(line));
            }

            Invalidate();
        }

        public void AppendDivider()
        {
            Append(new DividerConsoleRow());

            Invalidate();
        }

        public void Clear()
        {
            _rows.Clear();

            _offset = null;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (DirectText directText = new DirectText(e.Graphics))
            {
                int width = Width - _scrollBarWidth;

                InitializeNewRows(directText, width);
                EnsureEstimated(directText, width);

                if (_scrollToBottom)
                {
                    _offset = GetLastOffset();

                    _scrollToBottom = false;
                }

                UpdateScrollBar();

                int top = 0;

                LinkedListNode<ConsoleRow> current = _offset;

                while (current != null && top < Height)
                {
                    int height = current.Value.GetHeight(directText, _defaultFont, width);

                    current.Value.Paint(e.Graphics, directText, _defaultFont, top, width);

                    top += height;

                    current = current.Next;
                }

                if (0 <= top && top < Height)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Window, new Rectangle(0, top, width, Height - top));
                }
            }
        }

        void InitializeNewRows(DirectText directText, int width)
        {
            if (_rows.Count == 0) return;

            LinkedListNode<ConsoleRow> current = _rows.Last;

            // The following method assumes at least one row needs initialisation:
            if (current.Value.Initialized) return;

            while (!current.Value.Initialized)
            {
                if (current.Previous == null) break;

                current = current.Previous;
            }

            // The current is now at the top of the uninitialised rows:
            int height = 0;

            if (current.Previous != null) height = current.Previous.Value.CumulativeEstimatedHeight;

            while (current != null)
            {
                current.Value.Initialize(directText, _defaultFont);
                current.Value.Initialized = true;

                height += current.Value.GetEstimatedHeight(width);

                current.Value.CumulativeEstimatedHeight = height;

                current = current.Next;
            }
        }

        void EnsureEstimated(DirectText directText, int width)
        {
            if (_estimatedForWidth != width)
            {
                _estimatedForWidth = width;

                int height = 0;

                foreach (ConsoleRow row in _rows)
                {
                    height += row.GetEstimatedHeight(_estimatedForWidth);
                    row.CumulativeEstimatedHeight = height;
                }
            }
        }

        LinkedListNode<ConsoleRow> GetLastOffset()
        {
            LinkedListNode<ConsoleRow> lastOffset = _rows.Last;

            if (lastOffset == null) return null;

            int estimatedBottom = lastOffset.Value.CumulativeEstimatedHeight;

            while (lastOffset != null)
            {
                int heightIncludingLastOffset;

                if (lastOffset.Previous != null)
                {
                    heightIncludingLastOffset = estimatedBottom - lastOffset.Previous.Value.CumulativeEstimatedHeight;
                }
                else
                {
                    heightIncludingLastOffset = estimatedBottom;
                }

                if (heightIncludingLastOffset > Height)
                {
                    if (lastOffset.Next == null)
                    {
                        return _rows.Last;
                    }
                    else
                    {
                        return lastOffset.Next;
                    }
                }

                lastOffset = lastOffset.Previous;
            }

            return _rows.First;
        }

        void UpdateScrollBar()
        {
            int estimatedHeight = 0;

            if (_rows.Last != null)
            {
                estimatedHeight = _rows.Last.Value.CumulativeEstimatedHeight;
            }

            _scrollBar.Minimum = 0;
            _scrollBar.Maximum = estimatedHeight;
            _scrollBar.LargeChange = Height;
            _scrollBar.SmallChange = 1;

            if (_offset != null && _offset.Previous != null)
            {
                _scrollBar.Value = Math.Max(0, _offset.Previous.Value.CumulativeEstimatedHeight);
            }
            else
            {
                _scrollBar.Value = 0;
            }
        }

        void _scrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            if (_offset == null) return;

            int fakeDelta = e.NewValue - e.OldValue;

            int largeHeight = (Height * 2) / 3;

            switch (e.Type)
            {
                case ScrollEventType.SmallDecrement:
                    if (_offset.Previous != null) _offset = _offset.Previous;
                    break;

                case ScrollEventType.SmallIncrement:
                    if (_offset.Next != null) _offset = _offset.Next;
                    break;

                case ScrollEventType.LargeDecrement:
                    int initialBottom = _offset.Value.CumulativeEstimatedHeight;

                    while (_offset.Previous != null && initialBottom - _offset.Value.CumulativeEstimatedHeight < largeHeight)
                    {
                        _offset = _offset.Previous;
                    }

                    break;

                case ScrollEventType.LargeIncrement:
                    int initialTop = _offset.Value.CumulativeEstimatedHeight;

                    while (_offset.Next != null && _offset.Value.CumulativeEstimatedHeight - initialTop < largeHeight)
                    {
                        _offset = _offset.Next;
                    }

                    break;

                case ScrollEventType.First:
                    _offset = _rows.First;
                    break;

                case ScrollEventType.Last:
                    _offset = GetLastOffset();
                    break;

                case ScrollEventType.ThumbPosition:
                case ScrollEventType.ThumbTrack:
                    _offset = _rows.First;

                    while (e.NewValue > _offset.Value.CumulativeEstimatedHeight && _offset.Next != null)
                    {
                        _offset = _offset.Next;
                    }

                    break;

                case ScrollEventType.EndScroll:
                    break;
            }

            LinkedListNode<ConsoleRow> lastOffset = GetLastOffset();

            if (lastOffset.Value.Ordinal < _offset.Value.Ordinal) _offset = lastOffset;

            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            Invalidate();
        }
    }
}
