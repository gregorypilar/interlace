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
using System.Drawing.Imaging;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Interlace.ScreenShooting
{
    public partial class ScreenShotControl : Control
    {
        ScreenShot _screenShot;

        ScreenShotWindow _currentWindow = null;
        Region _currentWindowRegion = null;
        Region _currentHitRegion = null;

        Bitmap _capturedBitmap;

        public event EventHandler BitmapCaptured;

        public ScreenShot ScreenShot
        { 	 
           get { return _screenShot; }
           set 
           {
               Invalidate();

               _screenShot = value; 
           }
        }

        public ScreenShotControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        void DisposeNonDesigner()
        {
            if (_currentHitRegion != null)
            {
                _currentHitRegion.Dispose();
                _currentHitRegion = null;
            }

            if (_currentWindowRegion != null)
            {
                _currentWindowRegion.Dispose();
                _currentWindowRegion = null;
            }

            if (_capturedBitmap != null)
            {
                _capturedBitmap.Dispose();
                _capturedBitmap = null;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ColorMatrix matrix = new ColorMatrix(
                new float[][] {
                    new float[] {0.3f, 0.3f, 0.3f, 0.0f, 0.0f}, 
                    new float[] {0.59f, 0.59f, 0.59f, 0.0f, 0.0f}, 
                    new float[] {0.11f, 0.11f, 0.11f, 0.0f, 0.0f}, 
                    new float[] {0.0f, 0.0f, 0.0f, 1.0f, 0.0f}, 
                    new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}, 
                });

            using (ImageAttributes imageAttributes = new ImageAttributes())
            {
                imageAttributes.SetColorMatrix(matrix);

                e.Graphics.DrawImage(_screenShot.CaptureBitmap,
                    new Rectangle(new Point(0, 0), _screenShot.CaptureBitmap.Size), 0, 0,
                    _screenShot.CaptureBitmap.Size.Width, _screenShot.CaptureBitmap.Size.Height, GraphicsUnit.Pixel, imageAttributes);
            }

            if (_currentWindowRegion != null)
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(128, Color.Red)))
                {
                    e.Graphics.FillRegion(brush, _currentWindowRegion);
                }
            }
        }

        private void ScreenShotControl_MouseMove(object sender, MouseEventArgs e)
        {
            Point screenPoint = PointToScreen(e.Location);

            if (_currentHitRegion == null || !_currentHitRegion.IsVisible(screenPoint))
            {
                ScreenShotWindow window = ScreenShotWindow.FindWindowByPoint(_screenShot.ScreenWindow, screenPoint);

                if (window != _currentWindow)
                {
                    if (_currentWindowRegion != null) _currentWindowRegion.Dispose();
                    if (_currentHitRegion != null) _currentHitRegion.Dispose();

                    _currentWindow = window;
                    _currentWindowRegion = ScreenShotWindow.FindWindowVisibleRegion(_screenShot.ScreenWindow, window, true);
                    _currentHitRegion = ScreenShotWindow.FindWindowVisibleRegion(_screenShot.ScreenWindow, window, false);

                    Invalidate();
                }
            }
        }

        public Bitmap GetAndClearCapturedBitmap()
        {
            if (_capturedBitmap == null) throw new InvalidOperationException();

            Bitmap toReturn = _capturedBitmap;
            _capturedBitmap = null;

            return toReturn;
        }

        private void ScreenShotControl_Click(object sender, EventArgs e)
        {
            if (_currentWindowRegion == null) return;

            Rectangle fromBounds;

            using (Graphics g = Graphics.FromImage(_screenShot.CaptureBitmap))
            {
                fromBounds = Rectangle.Ceiling(_currentWindowRegion.GetBounds(g));
            }

            Bitmap bitmap = new Bitmap(fromBounds.Width, fromBounds.Height);

            try
            {
                bitmap.MakeTransparent();

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    using (Region clipRegion = new Region())
                    {
                        clipRegion.Union(_currentWindowRegion);
                        clipRegion.Translate(-fromBounds.Left, -fromBounds.Top);

                        Region oldClip = g.Clip;
                        g.Clip = clipRegion;

                        Rectangle destinationBounds = new Rectangle(new Point(0, 0), fromBounds.Size);
                        g.DrawImage(_screenShot.CaptureBitmap, destinationBounds, fromBounds, GraphicsUnit.Pixel);

                        g.Clip = oldClip;
                    }
                }

                _capturedBitmap = bitmap;

                if (BitmapCaptured != null) BitmapCaptured(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                bitmap.Dispose();

                throw;
            }
        }
    }
}
