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
using System.Text;

#endregion

namespace Interlace.Sharpcap
{
    public class NetworkReader
    {
        byte[] _data;
        int _offset;

        byte[] _buffer = new byte[8];

        public NetworkReader(byte[] data)
        {
            _data = data;
            _offset = 0;
        }

        public void Seek(int offset)
        {
            _offset = offset;

            if (_offset < 0) _offset = 0;
            if (_offset > _data.Length) _offset = _data.Length;
        }

        public bool AtEnd
        {
            get
            {
                return _offset >= _data.Length;
            }
        }

        public int Position
        {
            get { return _offset; }
        }

        public int Length
        {
            get { return _data.Length; }
        }

        public byte[] ReadBytes(int count)
        {
            if (_data.Length - _offset < count) throw new NetworkReaderTruncationException();

            byte value = _data[_offset];

            byte[] bytes = new byte[count];
            Array.Copy(_data, _offset, bytes, 0, count);

            _offset += count;

            return bytes;
        }

        public byte ReadUnsigned8()
        {
            if (_data.Length - _offset < 1) throw new NetworkReaderTruncationException();

            byte value = _data[_offset];

            _offset++;

            return value;
        }

        public sbyte ReadSigned8()
        {
            if (_data.Length - _offset < 1) throw new NetworkReaderTruncationException();

            sbyte value = (sbyte)_data[_offset];

            _offset++;

            return value;
        }

        public ushort ReadUnsigned16()
        {
            if (_data.Length - _offset < 2) throw new NetworkReaderTruncationException();

            ushort value = ByteOrder.NetworkToHost(BitConverter.ToUInt16(_data, _offset));

            _offset += 2;

            return value;
        }

        public short ReadSigned16()
        {
            if (_data.Length - _offset < 2) throw new NetworkReaderTruncationException();

            short value = ByteOrder.NetworkToHost(BitConverter.ToInt16(_data, _offset));

            _offset += 2;

            return value;
        }

        public uint ReadUnsigned32()
        {
            if (_data.Length - _offset < 4) throw new NetworkReaderTruncationException();

            uint value = ByteOrder.NetworkToHost(BitConverter.ToUInt32(_data, _offset));

            _offset += 4;

            return value;
        }

        public int ReadSigned32()
        {
            if (_data.Length - _offset < 4) throw new NetworkReaderTruncationException();

            int value = ByteOrder.NetworkToHost(BitConverter.ToInt32(_data, _offset));

            _offset += 4;

            return value;
        }

        public float ReadFloat32()
        {
            if (_data.Length - _offset < 4) throw new NetworkReaderTruncationException();

            _buffer[0] = _data[_offset + 3];
            _buffer[1] = _data[_offset + 2];
            _buffer[2] = _data[_offset + 1];
            _buffer[3] = _data[_offset + 0];

            _offset += 4;

            return BitConverter.ToSingle(_buffer, 0);
        }

        public double ReadFloat64()
        {
            if (_data.Length - _offset < 8) throw new NetworkReaderTruncationException();

            _buffer[0] = _data[_offset + 7];
            _buffer[1] = _data[_offset + 6];
            _buffer[2] = _data[_offset + 5];
            _buffer[3] = _data[_offset + 4];
            _buffer[4] = _data[_offset + 3];
            _buffer[5] = _data[_offset + 2];
            _buffer[6] = _data[_offset + 1];
            _buffer[7] = _data[_offset + 0];

            _offset += 8;

            return BitConverter.ToDouble(_buffer, 0);
        }
    }
}
