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
using System.Diagnostics;
using System.IO;
using System.Text;

#endregion

namespace Interlace.Pinch.Implementation
{
    public class PinchDecoder : IPinchDecoder
    {
        Stream _stream;
        byte[] _streamBuffer;
        byte[] _intelOrderBuffer;

        byte[] _bottomSmallCachedHeader;

        PinchDecoderHeader _header;
        Stack<PinchDecoderHeader> _headerStack;

        int _headerPrepareBitCount;

        public PinchDecoder(Stream stream)
        {
            _stream = stream;
            _streamBuffer = new byte[8];
            _intelOrderBuffer = new byte[8];

            _bottomSmallCachedHeader = new byte[32];

            _header = null;
            _headerStack = new Stack<PinchDecoderHeader>();
        }

        #region Decoding Utilities

        void BufferBytes(int count)
        {
            int totalRead = 0;

            while (totalRead < count)
            {
                int read = _stream.Read(_streamBuffer, totalRead, count - totalRead);

                if (read == 0) throw new PinchEndOfStreamException();

                totalRead += read;
            }
        }

        byte[] ReadBytes(int count)
        {
            byte[] bytes = new byte[count];

            int totalRead = 0;

            while (totalRead < count)
            {
                int read = _stream.Read(bytes, totalRead, count - totalRead);

                if (read == 0) throw new PinchEndOfStreamException();

                totalRead += read;
            }

            return bytes;
        }

        int ReadUnsignedTag()
        {
        	int shift = 0;
        	int tag = 0;
            int readByte;

            do
            {
                readByte = _stream.ReadByte();

                if (readByte == -1) throw new PinchEndOfStreamException();
                if (shift >= 31) throw new PinchInvalidCodingException();

                tag |= (readByte & 0x7f) << shift;

                shift += 7;
            }
            while ((readByte & 0x80) != 0);

            return tag;
        }

        int ReadSignedTag()
        {
        	int shift = 0;
        	uint tag = 0;
            int readByte;

            do
            {
                readByte = _stream.ReadByte();

                if (readByte == -1) throw new PinchEndOfStreamException();
                if (shift >= 32) throw new PinchInvalidCodingException();

                tag |= (uint)(readByte & 0x7f) << shift;

                shift += 7;
            }
            while ((readByte & 0x80) != 0);

            return (int)(tag >> 1) ^ ((int)tag << 31 >> 31);
        }

        long ReadSignedLongTag()
        {
        	int shift = 0;
        	ulong tag = 0;
            int readByte;

            do
            {
                readByte = _stream.ReadByte();

                if (readByte == -1) throw new PinchEndOfStreamException();
                if (shift >= 64) throw new PinchInvalidCodingException();

                tag |= (ulong)(readByte & 0x7f) << shift;

                shift += 7;
            }
            while ((readByte & 0x80) != 0);

            return (long)(tag >> 1) ^ ((long)tag << 63 >> 63);
        }

        decimal ReadDecimal()
        {
            // Read the low bits:
        	int lowShift = 0;
            int lowReadByte;
        	long low = 0;

            do
            {
                lowReadByte = _stream.ReadByte();

                if (lowReadByte == -1) throw new PinchEndOfStreamException();

                low |= (long)(lowReadByte & 0x7f) << lowShift;

                lowShift += 7;
            }
            while ((lowReadByte & 0x80) != 0 && (lowShift < 49));

            // If the last low octet had a continuation bit, continue reading into high:
            long high = 0;

            if ((lowReadByte & 0x80) != 0)
            {
            	int highShift = 0;
                int highReadByte;

                do
                {
                    highReadByte = _stream.ReadByte();

                    if (highReadByte == -1) throw new PinchEndOfStreamException();
                    if (highShift >= 49) throw new PinchInvalidCodingException();

                    high |= (long)(highReadByte & 0x7f) << highShift;

                    highShift += 7;
                }
                while ((highReadByte & 0x80) != 0);
            }

            int intLow = (int)(low & 0xffffffffL);
            int intMiddle = 
                (int)((low & 0x1ffff00000000L) >> 32) | 
                (int)((high & 0x0000000000007fffL) << 17);
            int intHigh = (int)(high >> 15);

            int scale = ReadSignedTag();
            bool isNegative = scale < 0;

            if (isNegative) scale = ~scale;

            return new decimal(intLow, intMiddle, intHigh, isNegative, (byte)scale);
        }

        #endregion

        #region IPinchDecoder Implementation

        public void OpenUncountedContainer()
        {
            Debug.Assert(_headerPrepareBitCount == 0);
        }

        public int OpenCountedContainer()
        {
            Debug.Assert(_headerPrepareBitCount == 0);

            return ReadUnsignedTag();
        }

        public void PrepareDecodeRequiredFloat32(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredFloat64(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredInt8(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredInt16(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredInt32(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredInt64(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredDecimal(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredBool(PinchFieldProperties properties)
        {
            _headerPrepareBitCount++;
        }

        public void PrepareDecodeRequiredString(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredBytes(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredEnumeration(PinchFieldProperties properties)
        {
            // No header used.
        }

        public void PrepareDecodeRequiredStructure(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 2;
        }

        public void PrepareDecodeOptionalFloat32(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalFloat64(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalInt8(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalInt16(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalInt32(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalInt64(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalDecimal(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalBool(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 2;
        }

        public void PrepareDecodeOptionalString(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalBytes(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalEnumeration(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 1;
        }

        public void PrepareDecodeOptionalStructure(PinchFieldProperties properties)
        {
            _headerPrepareBitCount += 2;
        }

        public void PrepareContainer()
        {
            if (_header == null)
            {
                _header = new PinchDecoderHeader(_headerPrepareBitCount, _bottomSmallCachedHeader, _stream);
            }
            else
            {
                _headerStack.Push(_header);
                _header = new PinchDecoderHeader(_headerPrepareBitCount, _header.UpperSmallCachedHeader, _stream);
            }

            _headerPrepareBitCount = 0;
        }

        public float DecodeRequiredFloat32(PinchFieldProperties properties)
        {
            BufferBytes(4);

            _intelOrderBuffer[0] = _streamBuffer[3];
            _intelOrderBuffer[1] = _streamBuffer[2];
            _intelOrderBuffer[2] = _streamBuffer[1];
            _intelOrderBuffer[3] = _streamBuffer[0];

            return BitConverter.ToSingle(_intelOrderBuffer, 0);
        }

        public double DecodeRequiredFloat64(PinchFieldProperties properties)
        {
            BufferBytes(8);

            _intelOrderBuffer[0] = _streamBuffer[7];
            _intelOrderBuffer[1] = _streamBuffer[6];
            _intelOrderBuffer[2] = _streamBuffer[5];
            _intelOrderBuffer[3] = _streamBuffer[4];
            _intelOrderBuffer[4] = _streamBuffer[3];
            _intelOrderBuffer[5] = _streamBuffer[2];
            _intelOrderBuffer[6] = _streamBuffer[1];
            _intelOrderBuffer[7] = _streamBuffer[0];

            return BitConverter.ToDouble(_intelOrderBuffer, 0);
        }

        public byte DecodeRequiredInt8(PinchFieldProperties properties)
        {
            int value = _stream.ReadByte();

            if (value == -1) throw new PinchEndOfStreamException();

            return (byte)value;
        }

        public short DecodeRequiredInt16(PinchFieldProperties properties)
        {
            return (short)ReadSignedTag();
        }

        public int DecodeRequiredInt32(PinchFieldProperties properties)
        {
            return ReadSignedTag();
        }

        public long DecodeRequiredInt64(PinchFieldProperties properties)
        {
            return ReadSignedLongTag();
        }

        public decimal DecodeRequiredDecimal(PinchFieldProperties properties)
        {
            return ReadDecimal();
        }

        public bool DecodeRequiredBool(PinchFieldProperties properties)
        {
            return _header.ReadOneHeaderBit();
        }

        public string DecodeRequiredString(PinchFieldProperties properties)
        {
            int byteCount = ReadUnsignedTag();
            byte[] bytes = ReadBytes(byteCount);

            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] DecodeRequiredBytes(PinchFieldProperties properties)
        {
            int byteCount = ReadUnsignedTag();
            return ReadBytes(byteCount);
        }

        public int DecodeRequiredEnumeration(PinchFieldProperties properties)
        {
            return ReadUnsignedTag();
        }

        public object DecodeRequiredStructure(IPinchableFactory factory, PinchFieldProperties properties)
        {
            int flag = _header.ReadTwoHeaderBits();

            return PinchDecoder.DecodeStructure(this, factory, false);
        }

        public float? DecodeOptionalFloat32(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                BufferBytes(4);

                _intelOrderBuffer[0] = _streamBuffer[3];
                _intelOrderBuffer[1] = _streamBuffer[2];
                _intelOrderBuffer[2] = _streamBuffer[1];
                _intelOrderBuffer[3] = _streamBuffer[0];

                return BitConverter.ToSingle(_intelOrderBuffer, 0);
            }
            else
            {
                return null;
            }
        }

        public double? DecodeOptionalFloat64(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                BufferBytes(8);

                _intelOrderBuffer[0] = _streamBuffer[7];
                _intelOrderBuffer[1] = _streamBuffer[6];
                _intelOrderBuffer[2] = _streamBuffer[5];
                _intelOrderBuffer[3] = _streamBuffer[4];
                _intelOrderBuffer[4] = _streamBuffer[3];
                _intelOrderBuffer[5] = _streamBuffer[2];
                _intelOrderBuffer[6] = _streamBuffer[1];
                _intelOrderBuffer[7] = _streamBuffer[0];

                return BitConverter.ToDouble(_intelOrderBuffer, 0);
            }
            else
            {
                return null;
            }
        }

        public byte? DecodeOptionalInt8(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                int value = _stream.ReadByte();

                if (value == -1) throw new PinchEndOfStreamException();

                return (byte)value;
            }
            else
            {
                return null;
            }
        }

        public short? DecodeOptionalInt16(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                return (short)ReadSignedTag();
            }
            else
            {
                return null;
            }
        }

        public int? DecodeOptionalInt32(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                return ReadSignedTag();
            }
            else
            {
                return null;
            }
        }

        public long? DecodeOptionalInt64(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                return ReadSignedLongTag();
            }
            else
            {
                return null;
            }
        }

        public decimal? DecodeOptionalDecimal(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                return ReadDecimal();
            }
            else
            {
                return null;
            }
        }

        public bool? DecodeOptionalBool(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                return _header.ReadOneHeaderBit();
            }
            else
            {
                _header.ReadOneHeaderBit();

                return null;
            }
        }

        public string DecodeOptionalString(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                int byteCount = ReadUnsignedTag();
                byte[] bytes = ReadBytes(byteCount);

                return Encoding.UTF8.GetString(bytes);
            }
            else
            {
                return null;
            }
        }

        public byte[] DecodeOptionalBytes(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                int byteCount = ReadUnsignedTag();
                return ReadBytes(byteCount);
            }
            else
            {
                return null;
            }
        }

        public int? DecodeOptionalEnumeration(PinchFieldProperties properties)
        {
            if (_header.ReadOneHeaderBit())
            {
                return ReadUnsignedTag();
            }
            else
            {
                return null;
            }
        }

        public object DecodeOptionalStructure(IPinchableFactory factory, PinchFieldProperties properties)
        {
            int flag = _header.ReadTwoHeaderBits();

            if (flag != CodedFlags.HeaderStructureNotPresent)
            {
                return PinchDecoder.DecodeStructure(this, factory, false);
            }
            else
            {
                return null;
            }
        }

        public void CloseContainer()
        {
            if (_headerStack.Count > 0)
            {
                _header = _headerStack.Pop();
            }
            else
            {
                _header = null;
            }
        }

        #endregion

        internal static object DecodeStructure(PinchDecoder pinchDecoder, IPinchableFactory factory, bool expectHeader)
        {
            object value = factory.Create(null);

            if (value is IPinchable)
            {
                IPinchable pinchable = value as IPinchable;

                if (expectHeader)
                {
                    int protocolVersion;

                    pinchDecoder.DecodeHeader(out protocolVersion);

                    if (protocolVersion == 0)
                    {
                        throw new NotImplementedException("Protocol identifier tagging is not yet implemented in this reader.");
                    }

                    if (protocolVersion != pinchable.ProtocolVersion)
                    {
                        throw new NotImplementedException(string.Format(
                            "Versioning is not yet implemented; version {0} is supported by this " +
                            "reader, but the encoded data was version {1}.", pinchable.ProtocolVersion, protocolVersion));
                    }
                }

                pinchable.Decode(pinchDecoder);
            }
            else
            {
                throw new NotImplementedException("Surrogates are not yet implemented.");
            }

            return value;
        }

        internal void DecodeHeader(out int protocolVersion)
        {
            protocolVersion = ReadUnsignedTag();
        }
    }
}
