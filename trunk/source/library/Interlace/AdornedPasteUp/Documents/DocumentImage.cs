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
using System.Drawing.Imaging;
using System.Text;

using Interlace.AdornedPasteUp.Rendering;
using Interlace.PropertyLists;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    public class DocumentImage : IDocumentObject
    {
        ImageLink _imageLink;

        Document _document;

        public DocumentImage(ImageLink imageLink)
        {
            _imageLink = imageLink;

            _document = null;
        }

        public DocumentImage(DocumentDeserializationContext context, PropertyDictionary framedObjectDictionary)
        {
            int imageLinkIndex = framedObjectDictionary.IntegerFor("imageLinkIndex").Value;

            if (!context.ImageLinksByKey.ContainsKey(imageLinkIndex))
            {
                throw new DocumentReadingException(string.Format(
                    "The image link index \"{0}\" was not found in the image link manager dictionary.",
                    imageLinkIndex));
            }

            _imageLink = context.ImageLinksByKey[imageLinkIndex];
        }

        public ImageLink ImageLink
        { 	 
            get { return _imageLink; }
        }

        public PropertyDictionary Serialize(DocumentSerializationContext context)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor("type", "image");
            dictionary.SetValueFor("imageLinkIndex", context.ImageLinkKeys[_imageLink]);

            return dictionary;
        }

        public void Paint(Rectangle objectRectangle, Point screenTopLeft, Graphics g, DocumentPaintResources resources)
        {
            using (ImageAttributes attributes = new ImageAttributes())
            {
                if (resources.IsFadedFrame)
                {
                    ColorMatrix matrix = new ColorMatrix();
                    matrix.Matrix33 = 0.5f;

                    attributes.SetColorMatrix(matrix);
                }

                Rectangle paintRectangle = new Rectangle(screenTopLeft, objectRectangle.Size);

                g.DrawImage(resources.ImageLinkCache.GetCachedImage(_imageLink), new Rectangle(screenTopLeft, objectRectangle.Size), 
                    objectRectangle.X, objectRectangle.Y, objectRectangle.Width, objectRectangle.Height, 
                    GraphicsUnit.Pixel, attributes);
            }
        }

        public Rectangle ObjectBounds 
        {
            get 
            { 
                Point defaultLocation = new Point(0, 0);
                Size size = Size.Truncate(_imageLink.PhysicalDimension);

                return new Rectangle(defaultLocation, size);
            }
        }

        public Rectangle DefaultBounds
        {
            get { return ObjectBounds; }
        }

        public Document Document
        {
            set 
            {
                if (value != null)
                {
                    if (_document != null) throw new InvalidOperationException();

                    value.ImageLinkManager.Attach(_imageLink);
                }
                else
                {
                    if (_document != null)
                    {
                        _document.ImageLinkManager.Detach(_imageLink);
                    }
                }

                _document = value;
            }
        }
    }
}
