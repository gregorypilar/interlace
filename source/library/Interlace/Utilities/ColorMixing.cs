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

#endregion

namespace Interlace.Utilities
{
    public static class ColorMixing
    {
        public static Color MixColorsEqually(Color first, Color second)
        {
            byte R = (byte)((first.R + second.R) / 2);
            byte G = (byte)((first.G + second.G) / 2);
            byte B = (byte)((first.B + second.B) / 2);
            byte A = (byte)((first.A + second.A) / 2);

            return Color.FromArgb(A, R, G, B);
        }

        /// <summary>
        /// Mixes the <paramref name="first"/> color with the <paramref name="second"/> color,
        /// using the <paramref name="firstProportion"/> proportion of the first color.
        /// </summary>
        /// <param name="first">The first color.</param>
        /// <param name="second">The second color.</param>
        /// <param name="firstProportion">The proportion of the first color to use. Values
        /// less than 0.0 and greater than 1.0 are clipped to the nearest bounds.
        /// <returns>A new color that is a blend of the first and second.</returns>
        public static Color MixColors(Color first, Color second, float firstProportion)
        {
            if (firstProportion < 0.0f) firstProportion = 0.0f;
            if (1.0f < firstProportion) firstProportion = 1.0f;

            float secondProportion = 1.0f - firstProportion;

            byte R = (byte)(first.R * firstProportion + second.R * secondProportion);
            byte G = (byte)(first.G * firstProportion + second.G * secondProportion);
            byte B = (byte)(first.B * firstProportion + second.B * secondProportion);
            byte A = (byte)(first.A * firstProportion + second.A * secondProportion);

            return Color.FromArgb(A, R, G, B);
        }
    }
}
