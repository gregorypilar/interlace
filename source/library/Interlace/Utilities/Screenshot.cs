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
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

#endregion

namespace Interlace.Utilities
{
    public class Screenshot : IDisposable
    {
        Bitmap _bitmap = null;
        bool _captureMouseCursor = false;
        bool _captureRunningApplicationOnly = false;
        bool _captureOtherApplicationOutlines = true;

        Color _backgroundColor;
        Color _borderColor;
        Color _windowColor;

        Color _specialWindowBorderColor;
        Color _specialWindowColor;

        int _borderWidth;

        public Screenshot()
        {
            _backgroundColor = Color.SlateGray;
            _borderColor = ColorMixing.MixColors(Color.SlateGray, Color.White, 0.5f);
            _windowColor = ColorMixing.MixColors(Color.SlateGray, Color.White, 0.6f);

            _specialWindowBorderColor = ColorMixing.MixColors(Color.OliveDrab, Color.White, 0.5f);
            _specialWindowColor = ColorMixing.MixColors(Color.OliveDrab, Color.White, 0.6f);

            _borderWidth = 8;
        }

        public bool CaptureMouseCursor
        { 	 
            get { return _captureMouseCursor; }
            set { _captureMouseCursor = value; }
        }

        public bool CaptureRunningApplicationOnly
        { 	 
            get { return _captureRunningApplicationOnly; }
            set { _captureRunningApplicationOnly = value; }
        }

        public bool CaptureOtherApplicationOutlines
        { 	 
            get { return _captureOtherApplicationOutlines; }
            set { _captureOtherApplicationOutlines = value; }
        }

        public void Capture()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;

            _bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);

            using (Graphics bitmapGraphics = Graphics.FromImage(_bitmap))
            {
                bitmapGraphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);

                if (_captureMouseCursor)
                {
                    Point position = Form.MousePosition;
                    bitmapGraphics.DrawImage(Interlace.Utilities.Resources.Cursor, position.X, position.Y, 15, 25);
                }

                if (_captureRunningApplicationOnly)
                {
                    PaintRegionsFromOtherApplications(bitmapGraphics, new Rectangle(0, 0, bounds.Width, bounds.Height));
                }
            }
        }

        public void PaintRegionsFromOtherApplications(Graphics bitmapGraphics, Rectangle paintArea)
        {
            using (Region thisApplicationRegion = new Region())
            {
                List<SystemWindow> windows = new List<SystemWindow>(SystemWindow.TopLevelWindows);

                windows.Reverse();

                int currentProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;

                foreach (SystemWindow window in windows)
                {
                    if (!window.IsVisible) continue;

                    if (currentProcessId == window.ProcessId)
                    {
                        thisApplicationRegion.Union(window.Bounds);
                    }
                    else
                    {
                        thisApplicationRegion.Exclude(window.Bounds);
                    }
                }

                using (Region regionToPaint = new Region(paintArea))
                {
                    regionToPaint.Exclude(thisApplicationRegion);

                    using (Brush backgroundBrush = new SolidBrush(_backgroundColor))
                    {
                        bitmapGraphics.FillRegion(backgroundBrush, regionToPaint);
                    }

                    if (_captureOtherApplicationOutlines)
                    {
                        using (SolidBrush borderBrush = new SolidBrush(_borderColor))
                        {
                            using (SolidBrush windowBrush = new SolidBrush(_windowColor))
                            {
                                Region oldClip = bitmapGraphics.Clip;

                                bitmapGraphics.Clip = regionToPaint;

                                foreach (SystemWindow window in windows)
                                {
                                    if (!window.IsVisible || currentProcessId == window.ProcessId || window.IsDesktopWindow) continue;
                                    
                                    string className = window.ClassName;

                                    if (window.ClassName == "Progman") continue;

                                    if (window.ClassName != "Shell_TrayWnd")
                                    {
                                        borderBrush.Color = _borderColor;
                                        windowBrush.Color = _windowColor;
                                    }
                                    else
                                    {
                                        borderBrush.Color = _specialWindowBorderColor;
                                        windowBrush.Color = _specialWindowColor;
                                    }

                                    Rectangle rectangle = window.Bounds;

                                    bitmapGraphics.FillRectangle(borderBrush, rectangle);
                                    rectangle.Inflate(-_borderWidth, -_borderWidth);
                                    bitmapGraphics.FillRectangle(windowBrush, rectangle);
                                }

                                bitmapGraphics.Clip = oldClip;
                            }
                        }
                    }
                }
            }
        }
            
        public void Dispose()
        {
            if (_bitmap != null)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }
        }

        public Bitmap Bitmap
        { 	 
            get { return _bitmap; }
        }
    }
}
