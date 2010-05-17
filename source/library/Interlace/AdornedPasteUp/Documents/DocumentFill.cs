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
using System.Text;

using Interlace.AdornedPasteUp.Rendering;
using Interlace.PropertyLists;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    public class DocumentFill : IDocumentObject
    {
        Color _color = Color.White;

        public DocumentFill()
        {
        }

        public DocumentFill(DocumentDeserializationContext context, PropertyDictionary framedObjectDictionary)
        {
            if (framedObjectDictionary.HasDictionaryFor("color"))
            {
                _color = PropertyBuilders.ToColor(framedObjectDictionary.DictionaryFor("color"));
            }
        }

        public PropertyDictionary Serialize(DocumentSerializationContext context)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor("type", "fill");
            dictionary.SetValueFor("color", PropertyBuilders.FromColor(_color));

            return dictionary;
        }

        public event EventHandler ColorChanged;

        public Color Color
        { 	 
            get { return _color; }
            set 
            { 
                _color = value;

                if (ColorChanged != null) ColorChanged(this, EventArgs.Empty);
            }
        }

        public void Paint(Rectangle objectRectangle, Point screenTopLeft, Graphics g, DocumentPaintResources resources)
        {
            Rectangle paintRectangle = new Rectangle(screenTopLeft, objectRectangle.Size);

            using (SolidBrush brush = new SolidBrush(_color))
            {
                g.FillRectangle(brush, paintRectangle);
            }
        }

        public Rectangle ObjectBounds 
        {
            get 
            { 
                return new Rectangle(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue);
            }
        }

        public Rectangle DefaultBounds
        {
            get 
            {
                return new Rectangle(0, 0, 200, 200);
            }
        }

        public void Dispose()
        {
        }

        public Document Document
        {
            set { }
        }
    }
}
