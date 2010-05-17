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
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;

using Interlace.AdornedPasteUp.Rendering;
using Interlace.Drawing;
using Interlace.PropertyLists;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    public class LabelDocumentFrame : DocumentFrame
    {
        static IHandleFlyweight[] _handles;

        Size _callOutOffsetInDocument = new Size(100, 0);

        string _label;
        string _caption;

        class Memento : DocumentFrameMemento
        {
            Size _callOutOffsetInDocument;

            public Memento(LabelDocumentFrame frame)
            : base(frame)
            {
                _callOutOffsetInDocument = frame._callOutOffsetInDocument;
            }

            internal override void Restore(DocumentFrame frame)
            {
                base.Restore(frame);

                LabelDocumentFrame labelFrame = (LabelDocumentFrame)frame;

                labelFrame._callOutOffsetInDocument = _callOutOffsetInDocument;
            }
        }

        static LabelDocumentFrame()
        {
            _handles = new IHandleFlyweight[] { new MoveHandleFlyweight(), new MoveCallOutHandleFlyweight() };
        }

        public LabelDocumentFrame()
        {
        }

        public LabelDocumentFrame(DocumentDeserializationContext context, PropertyDictionary frameProperties)
        : base(context, frameProperties)
        {
            if (!frameProperties.HasDictionaryFor("callOutOffsetInDocument"))
            {
                throw new DocumentReadingException("A label frame is missing the required \"callOutOffsetInDocument\" field.");
            }

            _callOutOffsetInDocument = PropertyBuilders.ToSize(frameProperties.DictionaryFor("callOutOffsetInDocument"));
            _label = frameProperties.StringFor("label", "");
            _caption = frameProperties.StringFor("caption", "");
        }

        internal override PropertyDictionary Serialize(DocumentSerializationContext context)
        {
            PropertyDictionary dictionary = base.Serialize(context);

            dictionary.SetValueFor("type", "label");
            dictionary.SetValueFor("callOutOffsetInDocument", PropertyBuilders.FromSize(_callOutOffsetInDocument));
            dictionary.SetValueFor("label", _label);
            dictionary.SetValueFor("caption", _caption);

            return dictionary;
        }

        public event EventHandler CallOutOffsetInDocumentChanged;

        public Size CallOutOffsetInDocument
        { 	 
           get { return _callOutOffsetInDocument; }
           set 
           { 
               _callOutOffsetInDocument = value;

               if (CallOutOffsetInDocumentChanged != null) CallOutOffsetInDocumentChanged(this, EventArgs.Empty);
           }
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string Label
        {
            get { return _label; }
            set 
            { 
                _label = value;

                FirePropertyChanged("Label");
            }
        }

        public string Caption
        {
            get { return _caption; }
            set 
            { 
                _caption = value;

                FirePropertyChanged("Caption");
            }
        }

        public override Rectangle PaintBounds
        {
            get 
            {
                return LabelBounds;
            }
        }

        public Rectangle LabelBounds
        {
            get 
            {
                Rectangle rectangle = new Rectangle(_offsetInDocument, _lastLabelSize);

                if (rectangle.Width < 5 || rectangle.Height < 5)
                {
                    rectangle.Width = 80;
                    rectangle.Height = 30;
                }

                rectangle.Offset(-rectangle.Width / 2, -rectangle.Height / 2);

                return rectangle;
            }
        }

        public override Rectangle HitBounds
        {
            get 
            {
                Rectangle rectangle = LabelBounds;
                rectangle.Inflate(5, 5);

                return rectangle;
            }
        }

        Size _lastLabelSize = new Size(0, 0);

        public override void Paint(Graphics g, DocumentPaintResources resources)
        {
            TextRenderingHint oldTextRenderingHint = g.TextRenderingHint;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            SmoothingMode oldSmoothingMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Update the label size:
            _lastLabelSize = Size.Ceiling(g.MeasureString(_label, resources.LabelFont));

            Rectangle paintAtRectangle = LabelBounds;
            PointF paintAt = paintAtRectangle.Location;

            PaintLabelText(g, resources, paintAt);

            PaintLine(g, resources, paintAtRectangle, _offsetInDocument, new Point(_callOutOffsetInDocument));

            g.SmoothingMode = oldSmoothingMode;

            g.TextRenderingHint = oldTextRenderingHint;
        }

        private void PaintLine(Graphics g, DocumentPaintResources resources, 
            Rectangle labelRectangle, Point labelCentre, Point lineEnd)
        {
            const int marginAroundLabel = 5;

            Size halfLabelSize = new Size(
                labelRectangle.Width / 2 + marginAroundLabel, 
                labelRectangle.Height / 2 + marginAroundLabel);

            // Form two partitions (one "divided" by y = x, the other by y = -x) and join
            // them to determine which of the four ways the line is headed:
            Size delta = new Size(lineEnd) - new Size(labelCentre);
            Size absoluteDelta = new Size(Math.Abs(delta.Width), Math.Abs(delta.Height));

            bool inTopLeftTriangle = delta.Height >= delta.Width;
            bool inTopRightTriangle = delta.Height >= -delta.Width;

            bool aboveOrBelow = (inTopLeftTriangle == inTopRightTriangle);

            // Snap the line in to purely vertical or horizontal:
            const int bandHalfWidth = 15;

            bool inHorizontalBand = absoluteDelta.Height < bandHalfWidth;
            bool inVerticalBand = absoluteDelta.Width < bandHalfWidth;

            if (inVerticalBand) delta.Width = 0;
            if (inHorizontalBand) delta.Height = 0;

            Point[] points = new Point[3];

            Size leftRightMovement = new Size(delta.Width, 0);
            Size upDownMovement = new Size(0, delta.Height);

            points[0] = labelCentre;
            points[1] = points[0] + (aboveOrBelow ? upDownMovement : leftRightMovement);
            points[2] = points[1] + (aboveOrBelow ? leftRightMovement : upDownMovement);

            if (!aboveOrBelow) points[0].X += Math.Sign(delta.Width) * halfLabelSize.Width;
            if (aboveOrBelow) points[0].Y += Math.Sign(delta.Height) * halfLabelSize.Height;

            g.DrawLines(Pens.Black, points);
        }

        void PaintLabelText(Graphics g, DocumentPaintResources resources, PointF paintAt)
        {
            // If the label is blank or we're selected, draw a background:
            if (string.IsNullOrEmpty(_label) || resources.IsSelectedFrame)
            {
                using (GraphicsPath roundPath = Interlace.Drawing.Utilities.CreateRoundedRectanglePath(HitBounds, 10.0f))
                {
                    g.FillPath(resources.HandleBrush, roundPath);
                }
            }

            // Draw the label:
            g.DrawString(_label, resources.LabelFont, Brushes.Black, paintAt);
        }

        public override DocumentFrameMemento CreateMemento()
        {
            return new Memento(this);
        }

        public override void RestoreFromMemento(DocumentFrameMemento memento)
        {
            memento.Restore(this);
        }

        protected override IEnumerable<IHandleFlyweight> Handles
        {
            get 
            {
                return _handles;
            }
        }
    }
}
