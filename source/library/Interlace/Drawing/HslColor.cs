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

namespace Talcasoft.Drawing
{
    public class HslColor
    {
        // Hue, saturation, and luminosity are represented on a scale of 0-1 and
        // are scaled for use externally based on the scale constant.
        double _hue = 1.0;
        double _saturation = 1.0;
        double _luminosity = 1.0;

        const double _scale = 240.0;

        public double Hue
        {
            get { return _hue * _scale; }
            set { _hue = CheckRange(value / _scale); }
        }

        public double Saturation
        {
            get { return _saturation * _scale; }
            set { _saturation = CheckRange(value / _scale); }
        }

        public double Luminosity
        {
            get { return _luminosity * _scale; }
            set { _luminosity = CheckRange(value / _scale); }
        }

        private double CheckRange(double value)
        {
            value = Math.Max(value, 0);
            value = Math.Min(value, 1);

            return value;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        public static Color ToRGB(HslColor hslColor)
        {
            double r = 0, g = 0, b = 0;

            if (hslColor._luminosity != 0)
            {
                if (hslColor._saturation == 0)
                    r = g = b = hslColor._luminosity;
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor._luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor._hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor._hue);
                    b = GetColorComponent(temp1, temp2, hslColor._hue - 1.0 / 3.0);
                }
            }

            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);

            if (temp3 < 1.0 / 6.0)
            {
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            }
            else if (temp3 < 0.5)
            {
                return temp2;
            }
            else if (temp3 < 2.0 / 3.0)
            {
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            }
            else
            {
                return temp1;
            }
        }
        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
            {
                temp3 += 1.0;
            }
            else if (temp3 > 1.0)
            {
                temp3 -= 1.0;
            }

            return temp3;
        }
        private static double GetTemp2(HslColor hslColor)
        {
            double temp2;

            if (hslColor._luminosity < 0.5)
            {
                temp2 = hslColor._luminosity * (1.0 + hslColor._saturation);
            }
            else
            {
                temp2 = hslColor._luminosity + hslColor._saturation - (hslColor._luminosity * hslColor._saturation);
            }

            return temp2;
        }

        public static HslColor ToHSL(Color color)
        {
            HslColor hslColor = new HslColor();

            hslColor._hue = color.GetHue() / 360.0; // We store hue as 0-1 as opposed to 0-360 
            hslColor._luminosity = color.GetBrightness();
            hslColor._saturation = color.GetSaturation();

            return hslColor;
        }

        public void SetRGB(int red, int green, int blue)
        {
            HslColor hslColor = HslColor.ToHSL(Color.FromArgb(red, green, blue));

            this._hue = hslColor._hue;
            this._saturation = hslColor._saturation;
            this._luminosity = hslColor._luminosity;
        }

        public HslColor()
        {
        }

        public HslColor(Color color)
        {
            SetRGB(color.R, color.G, color.B);
        }

        public HslColor(int red, int green, int blue)
        {
            SetRGB(red, green, blue);
        }

        public HslColor(double hue, double saturation, double luminosity)
        {
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }

        public static implicit operator Color(HslColor hslColor)
        {
            return HslColor.ToRGB(hslColor);
        }

        public static implicit operator HslColor(Color color)
        {
            return HslColor.ToHSL(color);
        }
    }
}
