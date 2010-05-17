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

using MbUnit.Framework;

using Interlace.AdornedPasteUp.Documents;
using Interlace.PropertyLists;

#endregion

namespace AdornedPasteUpTests
{
    [TestFixture]
    public class TestSerialization
    {
        [Test]
        public void Test()
        {
            // Build a document:
            Document document = new Document();

            LabelDocumentFrame label = new LabelDocumentFrame();
            label.Label = "Testing 123";
            label.OffsetInDocument = new Point(1, 2);
            label.CallOutOffsetInDocument = new Size(3, 4);

            DocumentFill fill = new DocumentFill();
            fill.Color = Color.Red;

            RectangularDocumentFrame fillFrame = new RectangularDocumentFrame(fill);
            fillFrame.ClipBounds = new Rectangle(5, 6, 7, 8);
            fillFrame.OffsetInDocument = new Point(9, 10);

            ImageLink imageLink = new ImageLink(@"C:\Windows\Prairie Wind.bmp");
            DocumentImage image = new DocumentImage(imageLink);
            RectangularDocumentFrame imageFrame = new RectangularDocumentFrame(image);

            document.Frames.Add(label);
            document.Frames.Add(fillFrame);
            document.Frames.Add(imageFrame);

            // Round trip it:
            PropertyDictionary serialized = document.Serialize(null);

            Document resultingDocument = Document.Deserialize(serialized, @"C:\");

            // Check the frames list:
            Assert.AreEqual(3, resultingDocument.Frames.Count);
            Assert.IsAssignableFrom(typeof(LabelDocumentFrame), resultingDocument.Frames[0]);
            Assert.IsAssignableFrom(typeof(RectangularDocumentFrame), resultingDocument.Frames[1]);
            Assert.IsAssignableFrom(typeof(RectangularDocumentFrame), resultingDocument.Frames[2]);

            // Check the label frame:
            LabelDocumentFrame resultingLabel = resultingDocument.Frames[0] as LabelDocumentFrame;

            Assert.AreEqual(label.Label, resultingLabel.Label);
            Assert.AreEqual(label.OffsetInDocument, resultingLabel.OffsetInDocument);
            Assert.AreEqual(label.CallOutOffsetInDocument, resultingLabel.CallOutOffsetInDocument);

            // Check the fill frame:
            RectangularDocumentFrame resultingFillFrame = resultingDocument.Frames[1] as RectangularDocumentFrame;
            DocumentFill resultingFill = resultingFillFrame.FramedObject as DocumentFill;

            Assert.AreEqual(resultingFillFrame.ClipBounds, fillFrame.ClipBounds);
            Assert.AreEqual(resultingFillFrame.OffsetInDocument, fillFrame.OffsetInDocument);

            Assert.AreEqual(resultingFill.Color.ToArgb(), fill.Color.ToArgb());

            // Check the image frame:
            RectangularDocumentFrame resultingImageFrame = resultingDocument.Frames[2] as RectangularDocumentFrame;
            DocumentImage resultingImage = resultingImageFrame.FramedObject as DocumentImage;

            Assert.AreEqual(resultingImageFrame.ClipBounds, imageFrame.ClipBounds);
            Assert.AreEqual(resultingImageFrame.OffsetInDocument, imageFrame.OffsetInDocument);

            ImageLink resultingImageLink = resultingImage.ImageLink;

            Assert.AreEqual(imageLink.FileName, resultingImageLink.FileName);
            Assert.AreEqual(imageLink.PhysicalDimension, resultingImageLink.PhysicalDimension);
        }
    }
}
