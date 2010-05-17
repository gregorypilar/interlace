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

#endregion

namespace Interlace.Imaging
{
    public class GifGraphicsControlExtension
    {
        enum DisposalMethod
        {
            None = 0,
            NoNotDispose = 1,
            RestoreToBackgroundColor = 2,
            RestoreToPrevious = 3,
            Reserved4 = 4,
            Reserved5 = 5,
            Reserved6 = 6,
            Reserved7 = 7
        }

        DisposalMethod _disposalMethod;
        ushort _delayTime;
        byte _transparentColourIndex;
        bool _userInputFlag;
        bool _transparentColorFlag;

        public GifGraphicsControlExtension()
        {
            _disposalMethod = DisposalMethod.None;
            _delayTime = 0;
            _transparentColourIndex = 0;
            _userInputFlag = false;
            _transparentColorFlag = false;
        }

        internal void ReadFromFile(BinaryReader reader)
        {
            byte blockSize = reader.ReadByte();

            if (blockSize != 4) throw new FormatException("A graphics control extension block has an incorrect length; the GIF is corrupt.");

            byte flags = reader.ReadByte();

            _disposalMethod = (DisposalMethod)((flags & 0x1c) >> 2);
            _userInputFlag = (flags & 0x02) != 0;
            _transparentColorFlag = (flags & 0x01) != 0;

            _delayTime = reader.ReadUInt16();
            _transparentColourIndex = reader.ReadByte();
        }

        internal void WriteToFile(BinaryWriter writer)
        {
            writer.Write((byte)GifConstants.ExtensionLabel);
            writer.Write((byte)GifConstants.GraphicsControlExtensionSubLabel);
            writer.Write((byte)4);

            byte flags = 0x00;

            if ((int)_disposalMethod > 7) throw new InvalidOperationException();

            flags |= (byte)((int)_disposalMethod << 2);
            if (_userInputFlag) flags |= 0x02;
            if (_transparentColorFlag) flags |= 0x01;

            writer.Write((byte)flags);

            writer.Write((ushort)_delayTime);
            writer.Write((byte)_transparentColourIndex);

            writer.Write((byte)GifConstants.BlockTerminator);
        }
    }
}
