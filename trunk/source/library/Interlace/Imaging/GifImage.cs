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
using System.IO;
using System.Text;

using Interlace.Utilities;

#endregion

namespace Interlace.Imaging
{
    public class GifImage
    {
        GifColourTable _colourTable;

        ushort _width;
        ushort _height;
        byte _backgroundColourIndex;
        byte _pixelAspectRatio;

        int _colourResolutionBits;
        bool _colourTableIsSorted;

        List<GifFrame> _frames = new List<GifFrame>();

        readonly byte[] _headerMagic = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };

        public GifImage(Stream stream)
        {
            ReadFromFile(stream);
        }

        public void WriteToFile(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                WriteHeaderToFile(writer);
                WriteFramesToFile(writer);
                WriteTerminatorToFile(writer);
            }
        }

        private void WriteHeaderToFile(BinaryWriter writer)
        {
            writer.Write(_headerMagic);

            writer.Write((ushort)_width);
            writer.Write((ushort)_height);

            byte flags = 0;

            if (_colourTable != null) flags |= 0x80;
            flags |= (byte)((_colourResolutionBits & 0x07) << 4);
            if (_colourTableIsSorted) flags |= 0x08;
            if (_colourTable != null) flags |= GifColourTable.ColourCountToField(_colourTable.Count);

            writer.Write((byte)flags);
            writer.Write((byte)_backgroundColourIndex);
            writer.Write((byte)_pixelAspectRatio);

            if (_colourTable != null)
            {
                _colourTable.WriteToFile(writer);
            }
        }

        private void WriteFramesToFile(BinaryWriter writer)
        {
            foreach (GifFrame frame in _frames)
            {
                frame.WriteImageDescriptorToFile(writer);
            }
        }

        private void WriteTerminatorToFile(BinaryWriter writer)
        {
            writer.Write((byte)GifConstants.FileTerminatorLabel);
        }

        void ReadFromFile(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                ReadHeaderFromFile(reader);
                ReadBlocksFromFile(reader);
            }
        }

        private void ReadHeaderFromFile(BinaryReader reader)
        {
            byte[] header = reader.ReadBytes(_headerMagic.Length);

            if (!ByteUtilities.CompareBytes(header, _headerMagic))
            {
                throw new FormatException(
                    "The expected header was not found; the file is not a valid GIF file.");
            }

            _width = reader.ReadUInt16();
            _height = reader.ReadUInt16();
            byte flags = reader.ReadByte();
            _backgroundColourIndex = reader.ReadByte();
            _pixelAspectRatio = reader.ReadByte();

            bool hasGlobalColourTable = (flags & 0x80) != 0;
            _colourResolutionBits = 1 + (flags & 0x70) >> 4;
            _colourTableIsSorted = (flags & 0x08) != 0;
            int sizeOfGlobalColourTable = 1 << (1 + (flags & 0x07));

            if (hasGlobalColourTable)
            {
                _colourTable = new GifColourTable();
                _colourTable.ReadFromFile(reader, sizeOfGlobalColourTable);
            }
            else
            {
                _colourTable = null;
            }
        }

        void ReadBlocksFromFile(BinaryReader reader)
        {
            GifGraphicsControlExtension capturedExtension = null;

            while (true)
            {
                byte blockType = reader.ReadByte();
                byte extensionType;

                switch (blockType)
                {
                    case GifConstants.ImageDescriptorLabel:
                        GifFrame frame = new GifFrame();
                        frame.ReadImageDescriptorFromFile(reader, capturedExtension);
                        _frames.Add(frame);
                        capturedExtension = null;
                        break;

                    case GifConstants.FileTerminatorLabel:
                        return;

                    case GifConstants.ExtensionLabel:
                        extensionType = reader.ReadByte();

                        switch (extensionType)
                        {
                            case GifConstants.GraphicsControlExtensionSubLabel:
                                capturedExtension = new GifGraphicsControlExtension();
                                capturedExtension.ReadFromFile(reader);
                                break;

                            default:
                                ReadUnknownBlockFromFile(reader, true);
                                break;
                        }

                        ReadBlockTerminatorFromFile(reader);
                        break;

                    default:
                        ReadUnknownBlockFromFile(reader, false);
                        ReadBlockTerminatorFromFile(reader);
                        break;
                }
            }
        }

        void ReadUnknownBlockFromFile(BinaryReader reader, bool subLabelRead)
        {
            if (!subLabelRead) reader.ReadByte();
            byte blockLength = reader.ReadByte();

            byte[] blockData = reader.ReadBytes(blockLength);

            if (blockData.Length != blockLength)
            {
                throw new FormatException(
                    "The file ended within a block; the GIF file is corrupt.");
            }
        }

        void ReadBlockTerminatorFromFile(BinaryReader reader)
        {
            byte blockTerminator = reader.ReadByte();

            if (blockTerminator != 0) 
            {
                throw new FormatException(
                    "The file contained a block without a terminator; the GIF file is corrupt.");
            }
        }
    }
}
