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
    public class MachineNetworkWriter
    {
        Stream _stream;
        byte[] _buffer = new byte[8];

        public MachineNetworkWriter(Stream stream)
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
            _stream.Write(BitConverter.GetBytes(value), 0, 2);
        }

        public void WriteSigned16(short value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, 2);
        }

        public void WriteUnsigned32(uint value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteSigned32(int value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public void WriteFloat32(float value)
        {
            byte[] machineOrder = BitConverter.GetBytes((float)value);

            _stream.Write(machineOrder, 0, 4);
        }

        public void WriteFloat64(double value)
        {
            byte[] machineOrder = BitConverter.GetBytes((double)value);

            _stream.Write(machineOrder, 0, 8);
        }

        public void WriteBytes(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }
    }
}
