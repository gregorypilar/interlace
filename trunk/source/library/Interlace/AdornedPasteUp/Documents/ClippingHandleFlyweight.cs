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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

using Interlace.AdornedPasteUp.Rendering;
using Interlace.Drawing;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    public class ClippingHandleFlyweight : IHandleFlyweight
    {
        Point _outerRectangleShift = new Point(0, 0);

        bool _isEast;
        bool _isNorth;
        bool _isWest;
        bool _isSouth;

        public static ICollection<IHandleFlyweight> CreateStandardClippingHandles()
        {
            return new IHandleFlyweight[] {
                new ClippingHandleFlyweight(true, false, false, false),
                new ClippingHandleFlyweight(false, true, false, false),
                new ClippingHandleFlyweight(false, false, true, false),
                new ClippingHandleFlyweight(false, false, false, true),
            };
        }

        protected ClippingHandleFlyweight(bool isEast, bool isNorth, bool isWest, bool isSouth)
        {
            const int shift = 1;

            _isEast = isEast;
            _isNorth = isNorth;
            _isWest = isWest;
            _isSouth = isSouth;

            if (_isEast) _outerRectangleShift.X += shift;
            if (_isNorth) _outerRectangleShift.Y += shift;
            if (_isWest) _outerRectangleShift.X -= shift;
            if (_isSouth) _outerRectangleShift.Y -= shift;
        }

        public bool IsHit(DocumentFrame frame, Point point)
        {
            return GetHandleBounds(frame).Contains(point);
        }

        Rectangle GetHandleBounds(DocumentFrame frame)
        {
            Rectangle shiftedRectangle = frame.HitBounds;
            shiftedRectangle.Offset(
                _outerRectangleShift.X * (shiftedRectangle.Width + 10), 
                _outerRectangleShift.Y * (shiftedRectangle.Height + 10));

            Rectangle cutOutRectangle = frame.HitBounds;
            cutOutRectangle.Inflate(20, 20);

            return Rectangle.Intersect(shiftedRectangle, cutOutRectangle);
        }

        public void Paint(DocumentFrame frame, Graphics g, DocumentPaintResources resources)
        {
            SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = Interlace.Drawing.Utilities.CreateRoundedRectanglePath(GetHandleBounds(frame), 5.0f))
            {
                g.FillPath(resources.HandleBrush, path);
            }

            g.SmoothingMode = oldSmoothingMode;
        }

        public void UpdateDrag(Point firstPoint, Point currentPoint, Size delta, DocumentFrame frame, DocumentFrameMemento memento)
        {
            RectangularDocumentFrame rectangularFrame = (RectangularDocumentFrame)frame;
            frame.RestoreFromMemento(memento);

            Rectangle clipBounds = rectangularFrame.ClipBounds;

            if (_isEast) clipBounds.Width += delta.Width;

            if (_isSouth)
            {
                clipBounds.Y += delta.Height;
                clipBounds.Height -= delta.Height;
            }

            if (_isWest)
            {
                clipBounds.X += delta.Width;
                clipBounds.Width -= delta.Width;
            }

            if (_isNorth) clipBounds.Height += delta.Height;

            clipBounds.Intersect(rectangularFrame.ObjectBounds);

            rectangularFrame.ClipBounds = clipBounds;
        }
    }
}
