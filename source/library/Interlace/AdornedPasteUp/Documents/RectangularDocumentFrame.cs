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
using Interlace.PropertyLists;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    enum DocumentFrameDragFocus
    {
        None,
        Location
    }

    public class RectangularDocumentFrame : DocumentFrame
    {
        static List<IHandleFlyweight> _handles;

        IDocumentObject _framedObject;

        public Rectangle _clipBounds;

        bool _enableTopCutMarks = true;
        bool _enableBottomCutMarks = true;
        bool _enableLeftCutMarks = true;
        bool _enableRightCutMarks = true;

        class Memento : DocumentFrameMemento
        {
            internal Rectangle _clipBounds;

            public Memento(RectangularDocumentFrame frame)
            : base(frame)
            {
                _clipBounds = frame._clipBounds;
            }

            internal override void Restore(DocumentFrame frame)
            {
                base.Restore(frame);

                RectangularDocumentFrame rectangularFrame = (RectangularDocumentFrame)frame;
                rectangularFrame._clipBounds = _clipBounds;
            }
        }

        static RectangularDocumentFrame()
        {
            _handles = new List<IHandleFlyweight>();

            _handles.Add(new MoveHandleFlyweight());
            _handles.AddRange(ClippingHandleFlyweight.CreateStandardClippingHandles());
        }

        public RectangularDocumentFrame(IDocumentObject framedObject)
        {
            _offsetInDocument = new Point(50, 50);
            _clipBounds = framedObject.DefaultBounds;
            _framedObject = framedObject;
        }

        public RectangularDocumentFrame(DocumentDeserializationContext context, PropertyDictionary frameDictionary)
        : base(context, frameDictionary)
        {
            if (!frameDictionary.HasDictionaryFor("clipBounds"))
            {
                throw new DocumentReadingException("A label frame is missing the required \"clipBounds\" field.");
            }

            if (!frameDictionary.HasDictionaryFor("framedObject"))
            {
                throw new DocumentReadingException("A label frame is missing the required \"framedObject\" field.");
            }

            _clipBounds = PropertyBuilders.ToRectangle(frameDictionary.DictionaryFor("clipBounds"));

            PropertyDictionary framedObjectDictionary = frameDictionary.DictionaryFor("framedObject");

            string framedObjectType = framedObjectDictionary.StringFor("type", "missing");

            switch (framedObjectType)
            {
                case "image":
                    _framedObject = new DocumentImage(context, framedObjectDictionary);
                    break;

                case "fill":
                    _framedObject = new DocumentFill(context, framedObjectDictionary);
                    break;

                default:
                    throw new DocumentReadingException(string.Format("The document object type \"{0}\" is not supported in this version.",
                        framedObjectType));
            }
        }

        internal override PropertyDictionary Serialize(DocumentSerializationContext context)
        {
            PropertyDictionary dictionary = base.Serialize(context);

            dictionary.SetValueFor("type", "rectangular");
            dictionary.SetValueFor("clipBounds", PropertyBuilders.FromRectangle(_clipBounds));
            dictionary.SetValueFor("framedObject", _framedObject.Serialize(context));

            return dictionary;
        }

        public override Document Document
        {
            get
            {
                return base.Document;
            }
            internal set
            {
                base.Document = value;

                _framedObject.Document = value;
            }
        }

        public IDocumentObject FramedObject
        { 	 
            get { return _framedObject; }
        }

        public Rectangle ClipBounds
        { 	 
           get { return _clipBounds; }
           set { _clipBounds = value; }
        }

        public Rectangle ObjectBounds
        {
            get { return _framedObject.ObjectBounds; }
        }

        public override Rectangle HitBounds
        {
            get
            {
                Rectangle hitBounds = _clipBounds;
                hitBounds.Offset(_offsetInDocument);

                return hitBounds;
            }
        }

        public override Rectangle PaintBounds
        {
            get 
            {
                Rectangle paintBounds = _clipBounds;
                paintBounds.Offset(_offsetInDocument);

                return paintBounds;
            }
        }

        public override void Paint(Graphics g, DocumentPaintResources resources)
        {
            Point paintTopLeft = _clipBounds.Location + new Size(_offsetInDocument);

            _framedObject.Paint(_clipBounds, paintTopLeft, g, resources);

            Rectangle border = _clipBounds;
            border.Offset(_offsetInDocument);

            Rectangle objectBounds = _framedObject.ObjectBounds;

            if (_enableTopCutMarks && objectBounds.Top != _clipBounds.Top) PaintBorder(g, resources, border.Location, border.Location + new Size(border.Width - 1, 0));
            if (_enableBottomCutMarks && objectBounds.Bottom != _clipBounds.Bottom) PaintBorder(g, resources, border.Location + new Size(0, border.Height - 1), border.Location + new Size(border.Width - 1, border.Height - 1));
            if (_enableLeftCutMarks && objectBounds.Left != _clipBounds.Left) PaintBorder(g, resources, border.Location, border.Location + new Size(0, border.Height - 1));
            if (_enableRightCutMarks && objectBounds.Right != _clipBounds.Right) PaintBorder(g, resources, border.Location + new Size(border.Width - 1, 0), border.Location + new Size(border.Width - 1, border.Height - 1));
        }

        public void PaintBorder(Graphics g, DocumentPaintResources resources, Point first, Point second)
        {
            using (Pen pen = new Pen(SystemColors.ControlDark, 1.0f))
            {
                pen.DashStyle = DashStyle.Dash;

                g.DrawLine(pen, first, second);
            }
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
            get { return _handles; }
        }

        public bool EnableTopCutMarks
        { 	 
            get { return _enableTopCutMarks; }
            set 
            { 
                _enableTopCutMarks = value;

                FirePropertyChanged("EnableTopCutMarks");
            }
        }

        public bool EnableBottomCutMarks
        { 	 
            get { return _enableBottomCutMarks; }
            set 
            { 
                _enableBottomCutMarks = value; 

                FirePropertyChanged("EnableBottomCutMarks");
            }
        }

        public bool EnableLeftCutMarks
        { 	 
            get { return _enableLeftCutMarks; }
            set 
            { 
                _enableLeftCutMarks = value; 

                FirePropertyChanged("EnableLeftCutMarks");
            }
        }

        public bool EnableRightCutMarks
        { 	 
            get { return _enableRightCutMarks; }
            set 
            { 
                _enableRightCutMarks = value; 

                FirePropertyChanged("EnableRightCutMarks");
            }
        }
    }
}
