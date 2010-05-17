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

namespace Interlace.PropertyLists
{
    public static class PropertyBuilders
    {
        public static PropertyDictionary FromRectangle(Rectangle rectangle)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor("x", rectangle.X);
            dictionary.SetValueFor("y", rectangle.Y);
            dictionary.SetValueFor("width", rectangle.Width);
            dictionary.SetValueFor("height", rectangle.Height);

            return dictionary;
        }

        public static Rectangle ToRectangle(PropertyDictionary dictionary)
        {
            if (!dictionary.HasIntegerFor("x", "y", "width", "height"))
            {
                throw new PropertyListException("A rectangle property dictionary is missing some or " +
                    "all of the required fields (x, y, width or height).");
            }

            return new Rectangle(
                dictionary.IntegerFor("x").Value,
                dictionary.IntegerFor("y").Value,
                dictionary.IntegerFor("width").Value,
                dictionary.IntegerFor("height").Value
                );
        }

        public static PropertyDictionary FromPoint(Point point)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor("x", point.X);
            dictionary.SetValueFor("y", point.Y);

            return dictionary;
        }

        public static Point ToPoint(PropertyDictionary dictionary)
        {
            if (!dictionary.HasIntegerFor("x", "y"))
            {
                throw new PropertyListException("A point property dictionary is missing one or " +
                    "both of the required fields (x or y).");
            }

            return new Point(
                dictionary.IntegerFor("x").Value,
                dictionary.IntegerFor("y").Value
                );
        }

        public static PropertyDictionary FromSize(Size size)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor("width", size.Width);
            dictionary.SetValueFor("height", size.Height);

            return dictionary;
        }

        public static Size ToSize(PropertyDictionary dictionary)
        {
            if (!dictionary.HasIntegerFor("width", "height"))
            {
                throw new PropertyListException("A size property dictionary is missing one or " +
                    "both of the required fields (width or height).");
            }

            return new Size(
                dictionary.IntegerFor("width").Value,
                dictionary.IntegerFor("height").Value
                );
        }

        public static PropertyDictionary FromColor(Color color)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor("red", (int)color.R);
            dictionary.SetValueFor("green", (int)color.G);
            dictionary.SetValueFor("blue", (int)color.B);
            if (color.A != 255) dictionary.SetValueFor("alpha", color.A);

            return dictionary;
        }

        public static Color ToColor(PropertyDictionary dictionary)
        {
            if (!dictionary.HasIntegerFor("red", "green", "blue"))
            {
                throw new PropertyListException("A color property dictionary is missing one or " +
                    "both of the required fields (red, green or blue).");
            }

            return Color.FromArgb(
                (byte)dictionary.IntegerFor("alpha", 255),
                (byte)dictionary.IntegerFor("red").Value,
                (byte)dictionary.IntegerFor("green").Value,
                (byte)dictionary.IntegerFor("blue").Value
                );
        }
    }
}
