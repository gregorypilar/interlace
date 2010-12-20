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

#endregion

namespace Interlace.Drawing
{
    public class Utilities
    {
        public static GraphicsPath CreateRoundedRectanglePath(float x, float y, float width, float height, float radius)
        {
            return CreateRoundedRectanglePath(new RectangleF(x, y, width, height), radius);
        }

        public static GraphicsPath CreateRoundedRectanglePath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (rect.Width <= 0.0f || rect.Height <= 0.0f) return path;

            float x1 = rect.X;
            float x2 = rect.X + rect.Width;
            float y1 = rect.Y;
            float y2 = rect.Y + rect.Height;

            if (radius > rect.Width / 2) radius = rect.Width / 2;
            if (radius > rect.Height / 2) radius = rect.Width / 2;

            // Top left arc and top edge:
            if (radius > 0.0) path.AddArc(new RectangleF(x1, y1, radius * 2, radius * 2), 180, 90);
            path.AddLine(x1 + radius, y1, x2 - radius, y1);

            // Top right arc and right edge:
            if (radius > 0.0) path.AddArc(new RectangleF(x2 - radius * 2, y1, radius * 2, radius * 2), 270, 90);
            path.AddLine(x2, y1 + radius, x2, y2 - radius);

            // Bottom right arc and bottom edge:
            if (radius > 0.0) path.AddArc(new RectangleF(x2 - radius * 2, y2 - radius * 2, radius * 2, radius * 2), 0, 90);
            path.AddLine(x2 - radius, y2, x1 + radius, y2);

            // Bottom left arc and left edge:
            if (radius > 0.0) path.AddArc(new RectangleF(x1, y2 - radius * 2, radius * 2, radius * 2), 90, 90);
            path.AddLine(x1, y2 - radius, x1, y1 + radius);

            return path;
        }

        public static void DrawRecolouredImage(Graphics g, Image image, Point location, float width, Color newColour, DrawRecolouredImageFlags flags)
        {
            using (ImageAttributes attributes = new ImageAttributes())
            {
                // Create the recolouring:
                ColorMap colorMap = new ColorMap();
                colorMap.OldColor = Color.Black;
                colorMap.NewColor = newColour;
                attributes.SetRemapTable(new ColorMap[] { colorMap });

                // Calculate the icon dimensions:
                Size iconSize = new Size((int)width, 
                    (int)(((double)image.Height * width) / (double)image.Width));

                Rectangle iconRectangle = new Rectangle(new Point(location.X, location.Y), iconSize);

                // Draw the icon:
                if ((flags & DrawRecolouredImageFlags.DrawCentred) == DrawRecolouredImageFlags.DrawCentred)
                {
                    iconRectangle.Offset(-iconSize.Width / 2, -iconSize.Height / 2);
                }

                g.DrawImage(image, iconRectangle, 0.0f, 0.0f, 
                    image.Width, image.Height, GraphicsUnit.Pixel, attributes);
            }
        }

        public static void DrawTransparentBalloon(Graphics g, Rectangle bounds)
        {
            using (GraphicsPath roundPath = Utilities.CreateRoundedRectanglePath(bounds, 6))
            {
                using (SolidBrush backgroundBrush = new SolidBrush(Color.FromArgb(128, 255, 255, 255)))
                {
                    using (Pen backgroundPen = new Pen(Color.FromArgb(230, 255, 255, 255), 2.0f))
                    {
                        g.FillPath(backgroundBrush, roundPath);
                        g.DrawPath(backgroundPen, roundPath);
                    }
                }
            }
        }

        public static void DrawBalloon(Graphics g, Rectangle bounds, Brush backgroundBrush, Pen backgroundPen)
        {
            using (GraphicsPath roundPath = Utilities.CreateRoundedRectanglePath(bounds, 6))
            {
                g.FillPath(backgroundBrush, roundPath);
                g.DrawPath(backgroundPen, roundPath);
            }
        }
    }

    public enum DrawRecolouredImageFlags
    {
        None = 0,
        DrawCentred = 1
    }
}
