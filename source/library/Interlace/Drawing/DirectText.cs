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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

#endregion

// Portions of this code were originally developed for Bit Plantation Bit Library.
// (Portions Copyright © 2006 Bit Plantation)

namespace Interlace.Drawing
{
    public class DirectText : IDisposable
    {
        IntPtr _context;
        float _dpi;

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;

            public SIZE(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }
        } 

        public DirectText(Graphics context)
        {
            _dpi = context.DpiX;

            IntPtr contextPtr = context.GetHdc();

            try
            {
                _context = CreateCompatibleDC(contextPtr);

                if (_context == IntPtr.Zero) 
                {
                    throw new ExternalException("Could not create a device context for " + 
                        "the StyleRepository.");
                }
            }
            finally
            {
                context.ReleaseHdc();
            }
        }

        public void Dispose()
        {
            DeleteDC(_context);
        }

        public float Dpi
        {
            get { return _dpi; }
        }

        public int MeasureFittingCharacterCount(string text, float width, Font font)
        {
            if (width <= 0) return 0;

            IntPtr fontHandle = font.ToHfont();

            try
            {
                int characterCount = 0;
                int[] characterStarts = null;
                SIZE size = new SIZE();

                IntPtr oldObject = SelectObject(_context, fontHandle);

                bool successful = GetTextExtentExPoint(_context, text, text.Length,
                    (int)width, out characterCount, characterStarts, ref size);

                SelectObject(_context, oldObject);

                if (!successful)
                {
                    throw new ExternalException("Failed to calculate word wrapping information.");
                }

                return characterCount;
            }
            finally
            {
                DeleteObject(fontHandle);
            }
        }

        public Size MeasureString(string text, Font font)
        {
            IntPtr fontHandle = font.ToHfont();

            try
            {
                SIZE size = new SIZE();

                IntPtr oldObject = SelectObject(_context, fontHandle);

                bool successful = GetTextExtentPoint(_context, text, text.Length, ref size);

                SelectObject(_context, oldObject);

                if (!successful)
                {
                    throw new ExternalException("Failed to calculate word wrapping information.");
                }

                return new Size(size.cx, size.cy);
            }
            finally
            {
                DeleteObject(fontHandle);
            }
        }

        public void DrawString(Graphics g, string text, Font font, Color color, int x, int y)
        {
            IntPtr fontHandle = font.ToHfont();

            IntPtr deviceContext = g.GetHdc();

            try
            {
                IntPtr oldObject = SelectObject(deviceContext, fontHandle);
                int oldMode = SetBkMode(deviceContext, TRANSPARENT);
                int oldColor = SetTextColor(deviceContext, 
                    (int)color.B << 16 |
                    (int)color.G << 8 |
                    (int)color.R << 0);

                bool successful = TextOut(deviceContext, x, y, text, text.Length);

                SetTextColor(deviceContext, oldColor);
                SetBkMode(deviceContext, oldMode);
                SelectObject(deviceContext, oldObject);

                if (!successful)
                {
                    throw new ExternalException("Failed to calculate word wrapping information.");
                }
            }
            finally
            {
                g.ReleaseHdc(deviceContext); 

                DeleteObject(fontHandle);
            }
        }

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern bool GetTextExtentExPoint(IntPtr hdc, string lpszStr,
           int cchString, int nMaxExtent, out int lpnFit, int[] alpDx, ref SIZE lpSize);

        [DllImport("gdi32.dll")]
        static extern bool GetTextExtentPoint(IntPtr hdc, string lpString,
           int cbString, ref SIZE lpSize);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr objectHandle);

        [DllImport("gdi32.dll")]
        static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart,
           string lpString, int cbString);

        [DllImport("gdi32.dll")]
        static extern int SetTextColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll")]
        static extern int SetBkMode(IntPtr hdc, int iBkMode);

        const int TRANSPARENT = 1;
        const int OPAQUE = 2;
    }
}
