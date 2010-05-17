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
using System.Drawing;
using System.IO;
using System.Text;

#endregion

namespace Interlace.Imaging
{
    internal class GifColourTable : List<Color>
    {
        internal void ReadFromFile(BinaryReader reader, int entryCount)
        {
            Clear();

            for (int i = 0; i < entryCount; i++)
            {
                byte red = reader.ReadByte();
                byte green = reader.ReadByte();
                byte blue = reader.ReadByte();

                Add(Color.FromArgb(red, green, blue));
            }
        }

        internal void WriteToFile(BinaryWriter writer)
        {
            int totalEntries = FieldToColourCount(ColourCountToField(Count));
            int paddingEntries = totalEntries - Count;

            foreach (Color color in this)
            {
                writer.Write((byte)color.R);
                writer.Write((byte)color.G);
                writer.Write((byte)color.B);
            }

            for (int i = 0; i < paddingEntries; i++)
            {
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
            }
        }

        internal static int FieldToColourCount(byte field)
        {
            return 1 << (1 + field);
        }

        internal static byte ColourCountToField(int count)
        {
            if (count > 256) count = 256;
            if (count < 2) count = 2;

            byte k = 0;

            count = count - 1;

            while (count > 0)
            {
                k++;
                count = count >> 1;
            }

            return (byte)(k - 1);
        }
    }
}
