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
using System.Text;
using System.IO;

#endregion

// Portions of this code were originally developed for Bit Plantation internal use.
// (Portions Copyright © 2006 Bit Plantation)

namespace Interlace.Utilities
{
    public class BitReader
    {
        Stream _stream;
        byte _bitOffset;
        byte _bits;

        public BitReader(Stream stream)
        {
            _stream = stream;
            _bitOffset = 8;
        }

        private uint ReadUnsignedBits(int bitCount)
        {
            if (bitCount > 32)
            {
                throw new ArgumentException("The number of bits requested is too large " +
                    "to fit in to the storage space allocated to be returned.", "bitCount");
            }

            byte bitsLeft = (byte)bitCount;
            uint result = 0;

            while (bitsLeft > 0)
            {
                // Ensure some bits are available for this iteration:
                if (_bitOffset == 8)
                {
                    int readByte = _stream.ReadByte();

                    if (readByte == -1) throw new IOException(
                        "The end of a stream was reached while reading bits into a bit reader.");

                    _bits = (byte)readByte;
                    _bitOffset = 0;
                }

                if (_bitOffset < 8) throw new InvalidOperationException();

                // Pull all or some bits off:
                byte requiredCount = Math.Min((byte)(8 - _bitOffset), bitsLeft);
                byte maskBits = (byte)(0xff >> (8 - requiredCount));
                byte extractedBits = (byte)(maskBits & (_bits >> (8 - requiredCount - _bitOffset)));

                bitsLeft -= requiredCount;
                _bitOffset += requiredCount;

                result |= (uint)extractedBits << bitsLeft;
            }

            return result;
        }

        private int ReadSignedBits(int bitCount)
        {
            uint result = ReadUnsignedBits(bitCount);

            if (result >= (uint)(1 << (bitCount - 1)))
            {
                return (int)(result | (uint.MaxValue << bitCount));
            }
            else
            {
                return (int)result;
            }
        }

        public uint ReadUnsignedInteger(int bitCount)
        {
            return ReadUnsignedBits(bitCount);
        }

        public int ReadSignedInteger(int bitCount)
        {
            return ReadSignedBits(bitCount);
        }

        public ushort ReadUnsignedShort(int bitCount)
        {
            if (bitCount > 16)
            {
                throw new ArgumentException("The number of bits requested is too large " +
                    "to fit in to the storage space allocated to be returned.", "bitCount");
            }

            return (ushort)ReadUnsignedBits(bitCount);
        }

        public short ReadSignedShort(int bitCount)
        {
            if (bitCount > 16)
            {
                throw new ArgumentException("The number of bits requested is too large " +
                    "to fit in to the storage space allocated to be returned.", "bitCount");
            }

            return (short)ReadSignedBits(bitCount);
        }

        public byte ReadByte(int bitCount)
        {
            if (bitCount > 8)
            {
                throw new ArgumentException("The number of bits requested is too large " +
                    "to fit in to the storage space allocated to be returned.", "bitCount");
            }

            return (byte)ReadUnsignedBits(bitCount);
        }

        public byte[] ReadByteArray(int byteCount)
        {
            byte[] buffer = new byte[byteCount];
            int bufferUsed = 0;

            while (bufferUsed < byteCount)
            {
                int bytesRead = _stream.Read(buffer, bufferUsed, buffer.Length - bufferUsed);

                if (bytesRead == 0)
                {
                    throw new EndOfStreamException();
                }

                bufferUsed += bytesRead;
            }

            return buffer;
        }

        public bool ReadBit()
        {
            return ReadUnsignedBits(1) == 1;
        }
    }
}
