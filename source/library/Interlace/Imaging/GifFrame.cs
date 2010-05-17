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
using System.IO;
using System.Text;

#endregion

namespace Interlace.Imaging
{
    public class GifFrame
    {
        Rectangle _bounds;
        GifColourTable _colourTable;
        bool _isInterlaced;
        bool _isSorted;
        byte _minimumCodeSize;
        List<byte[]> _subBlocks;

        const byte _subBlockTerminator = 0;

        GifGraphicsControlExtension _extensionOrNull;

        public GifFrame()
        {
            _bounds = new Rectangle();
            _colourTable = null;
            _subBlocks = null;
            _extensionOrNull = null;
        }

        internal void ReadImageDescriptorFromFile(BinaryReader reader, GifGraphicsControlExtension extensionOrNull)
        {
            _extensionOrNull = extensionOrNull;

            _bounds.X = (int)reader.ReadUInt16();
            _bounds.Y = (int)reader.ReadUInt16();
            _bounds.Width = (int)reader.ReadUInt16();
            _bounds.Height = (int)reader.ReadUInt16();

            byte flags = reader.ReadByte();

            bool hasLocalColourTable = (flags & 0x80) != 0;
            _isInterlaced = (flags & 0x40) != 0;
            _isSorted = (flags & 0x20) != 0;
            int sizeOfLocalColourTable = 1 << (1 + (flags & 0x07));

            if (hasLocalColourTable)
            {
                _colourTable = new GifColourTable();
                _colourTable.ReadFromFile(reader, sizeOfLocalColourTable);
            }
            else
            {
                _colourTable = null;
            }

            ReadImageDataFromFile(reader);
        }

        void ReadImageDataFromFile(BinaryReader reader)
        {
            _minimumCodeSize = reader.ReadByte();

            _subBlocks = new List<byte[]>();

            byte subBlockLength;

            while ((subBlockLength = reader.ReadByte()) != _subBlockTerminator)
            {
                byte[] subBlock = reader.ReadBytes(subBlockLength);

                if (subBlock.Length != subBlockLength)
                {
                    throw new FormatException(
                        "The file ended within an image data subblock; the GIF file is corrupt.");
                }

                _subBlocks.Add(subBlock);
            }
        }

        internal void WriteImageDescriptorToFile(BinaryWriter writer)
        {
            if (_extensionOrNull != null) _extensionOrNull.WriteToFile(writer);

            writer.Write((byte)GifConstants.ImageDescriptorLabel);
            writer.Write((ushort)_bounds.X);
            writer.Write((ushort)_bounds.Y);
            writer.Write((ushort)_bounds.Width);
            writer.Write((ushort)_bounds.Height);

            byte flags = 0;

            if (_colourTable != null) flags |= 0x80;
            if (_isInterlaced) flags |= 0x40;
            if (_isSorted) flags |= 0x20;
            if (_colourTable != null) GifColourTable.ColourCountToField(_colourTable.Count);

            writer.Write((byte)flags);

            if (_colourTable != null) _colourTable.WriteToFile(writer);

            WriteImageDataToFile(writer);
        }

        void WriteImageDataToFile(BinaryWriter writer)
        {
            writer.Write((byte)_minimumCodeSize);

            foreach (byte[] subBlock in _subBlocks)
            {
                writer.Write((byte)subBlock.Length);
                writer.Write(subBlock);
            }

            writer.Write((byte)_subBlockTerminator);
        }

        public Rectangle Bounds
        { 	 
           get { return _bounds; }
        }

        public GifGraphicsControlExtension ExtensionOrNull
        { 	 
           get { return _extensionOrNull; }
           set { _extensionOrNull = value; }
        }
    }
}
