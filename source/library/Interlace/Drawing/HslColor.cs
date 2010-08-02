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
    public struct HslColor
    {
        double _hue;
        double _saturation;
        double _luminosity;

        public HslColor(double hue, double saturation, double luminosity)
        {
            _hue = hue % 360.0;
            _saturation = Math.Min(1.0, Math.Max(0.0, saturation));
            _luminosity = Math.Min(1.0, Math.Max(0.0, luminosity));
        }

        public Color ToRgb()
        {
            double red;
            double green;
            double blue;

            HslToRgbInternal(_hue, _saturation, _luminosity, out red, out green, out blue);

            return Color.FromArgb(
                (int)Math.Round(255 * red),
                (int)Math.Round(255 * green),
                (int)Math.Round(255 * blue));
        }

        public static implicit operator Color(HslColor hslColor)
        {
            return hslColor.ToRgb();
        }

        public double Hue
        {
            get { return _hue; }
            set 
            {
                _hue = Math.IEEERemainder(value, 360.0);
            }
        }

        public double Saturation
        {
            get { return _saturation; }
            set { _saturation = Math.Min(1.0, Math.Max(0.0, value)); }
        }

        public double Luminosity
        {
            get { return _luminosity; }
            set { _luminosity = Math.Min(1.0, Math.Max(0.0, value)); }
        }

        static void HslToRgbInternal(double hue, double saturation, double luminosity, 
            out double red, out double green, out double blue)
        {
            double chroma = saturation * (luminosity <= 0.5 ? 2.0 * luminosity : 2.0 - 2.0 * luminosity);

            double scaledHue = hue / 60.0;

            double x = chroma * (1.0 - Math.Abs((scaledHue % 2.0) - 1.0));

            switch ((int)Math.Floor(scaledHue))
            {
                case 0:
                    red = chroma;
                    green = x;
                    blue = 0.0;
                    break;

                case 1:
                    red = x;
                    green = chroma;
                    blue = 0.0;
                    break;

                case 2:
                    red = 0.0;
                    green = chroma;
                    blue = x;
                    break;

                case 3:
                    red = 0.0;
                    green = x;
                    blue = chroma;
                    break;

                case 4:
                    red = x;
                    green = 0.0;
                    blue = chroma;
                    break;

                default:
                    red = chroma;
                    green = 0;
                    blue = x;
                    break;
            }
        }
    }
}
