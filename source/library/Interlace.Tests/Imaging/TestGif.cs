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
using System.IO;
using System.Text;

using MbUnit.Framework;

using Interlace.Imaging;
using Interlace.Testing;
using Interlace.Utilities;

#endregion

namespace Interlace.Tests.Imaging
{
    [TestFixture]
    public class TestGif
    {
        Bitmap _originalBitmap;
        MemoryStream _originalFile;

        [SetUp]
        public void SetUp()
        {
            _originalBitmap = new Bitmap(20, 20);
            
            using (Graphics g = Graphics.FromImage(_originalBitmap))
            {
                g.FillRectangle(Brushes.Red, new Rectangle(0, 0, 20, 20));
                g.DrawLine(Pens.White, new Point(2, 2), new Point(18, 18));
            }

            _originalFile = new MemoryStream();
            _originalBitmap.Save(_originalFile, ImageFormat.Gif);
            _originalFile.Seek(0, SeekOrigin.Begin);
        }

        [TearDown]
        public void TearDown()
        {
            if (_originalBitmap != null) _originalBitmap.Dispose();
            if (_originalFile != null) _originalFile.Dispose();
        }

        [Test]
        public void TestRoundTrips()
        {
            // Test loading:
            GifImage image;

            image = new GifImage(new UnclosableStream(_originalFile));
            _originalFile.Seek(0, SeekOrigin.Begin);

            // Test saving from the loaded image:
            Bitmap resultingBitmap;

            using (MemoryStream stream = new MemoryStream())
            {
                image.WriteToFile(new UnclosableStream(stream));
                stream.Seek(0, SeekOrigin.Begin);

                TestUtilities.AssertArraysAreEqual(_originalFile.ToArray(), stream.ToArray());

                GifImage roundTripImage = new GifImage(new UnclosableStream(stream));
                stream.Seek(0, SeekOrigin.Begin);

                resultingBitmap = new Bitmap(stream);
            }

            AssertBitmapsAreEqual(_originalBitmap, resultingBitmap);
        }

        public void AssertBitmapsAreEqual(Bitmap left, Bitmap right)
        {
            Assert.AreEqual(left.Width, right.Width);
            Assert.AreEqual(left.Height, right.Height);

            Assert.LowerThan(left.Width, 50, "Bitmaps larger than 50 pixels wide are too large for fast pixel comparisons.");
            Assert.LowerThan(left.Height, 50, "Bitmaps larger than 50 pixels high are too large for fast pixel comparisons.");

            for (int x = 0; x < left.Width; x++)
            {
                for (int y = 0; y < left.Height; y++)
                {
                    Assert.AreEqual(left.GetPixel(x, y), right.GetPixel(x, y));
                }
            }
        }
    }
}
