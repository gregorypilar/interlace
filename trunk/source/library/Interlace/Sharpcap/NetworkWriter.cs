#region Using Directives and Copyright Notice

// Copyright (c) 2010, Bit Plantation
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Bit Plantation nor the
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

namespace Interlace.Sharpcap
{
    public class NetworkWriter
    {
        Stream _stream;
        byte[] _buffer = new byte[8];

        public NetworkWriter(Stream stream)
        {
            _stream = stream;
        }

        public void WriteUnsigned8(byte value)
        {
            _stream.WriteByte(value);
        }

        public void WriteSigned8(sbyte value)
        {
            _stream.WriteByte((byte)value);
        }

        public void WriteUnsigned16(ushort value)
        {
            _stream.Write(BitConverter.GetBytes(ByteOrder.HostToNetwork(value)), 0, 2);
        }

        public void WriteSigned16(short value)
        {
            _stream.Write(BitConverter.GetBytes(ByteOrder.HostToNetwork(value)), 0, 2);
        }

        public void WriteUnsigned32(uint value)
        {
            _stream.Write(BitConverter.GetBytes(ByteOrder.HostToNetwork(value)), 0, 4);
        }

        public void WriteSigned32(int value)
        {
            _stream.Write(BitConverter.GetBytes(ByteOrder.HostToNetwork(value)), 0, 4);
        }

        public void WriteFloat32(float value)
        {
            byte[] intelOrder = BitConverter.GetBytes((float)value);

            _buffer[0] = intelOrder[3];
            _buffer[1] = intelOrder[2];
            _buffer[2] = intelOrder[1];
            _buffer[3] = intelOrder[0];

            _stream.Write(_buffer, 0, 4);
        }

        public void WriteFloat64(double value)
        {
            byte[] intelOrder = BitConverter.GetBytes((double)value);

            _buffer[0] = intelOrder[7];
            _buffer[1] = intelOrder[6];
            _buffer[2] = intelOrder[5];
            _buffer[3] = intelOrder[4];
            _buffer[4] = intelOrder[3];
            _buffer[5] = intelOrder[2];
            _buffer[6] = intelOrder[1];
            _buffer[7] = intelOrder[0];

            _stream.Write(_buffer, 0, 8);
        }
    }
}
