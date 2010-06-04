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
using System.Diagnostics;
using System.IO;
using System.Text;

#endregion

namespace Interlace.Pinch.Implementation 
{
    public class PinchEncoder : IPinchEncoder
    {
        Stream _stream;
        byte[] _streamBuffer;

        int _headerBitsBuffer;
        int _headerBitsUsed;

        public PinchEncoder(Stream stream)
        {
            _stream = stream;
            _streamBuffer = new byte[8];

            _headerBitsBuffer = 0;
            _headerBitsUsed = 0;
        }

        #region Encoding Utilities

        void WriteOneHeaderBit(bool bit)
        {
            int maskedBit = bit ? 1 : 0;

            _headerBitsBuffer |= maskedBit << _headerBitsUsed;
            _headerBitsUsed++;

            if (_headerBitsUsed == 8)
            {
                _stream.WriteByte((byte)_headerBitsBuffer);

                _headerBitsBuffer = 0;
                _headerBitsUsed = 0;
            }
        }

        void WriteTwoHeaderBits(int bits)
        {
            int maskedBits = bits & 0x3;

            _headerBitsBuffer |= maskedBits << _headerBitsUsed;
            _headerBitsUsed += 2;

            if (_headerBitsUsed >= 8)
            {
                _stream.WriteByte((byte)_headerBitsBuffer);

                _headerBitsBuffer >>= 8;
                _headerBitsUsed -= 8;
            }
        }

        void FlushHeaderBits()
        {
            if (_headerBitsUsed > 0)
            {
                _stream.WriteByte((byte)_headerBitsBuffer);

                _headerBitsBuffer = 0;
                _headerBitsUsed = 0;
            }
        }

        void WriteUnsignedTag(int tag)
        {
            Debug.Assert(tag >= 0);

            int remaining = tag;

            while (remaining > 0x7f)
        	{
                _stream.WriteByte((byte)(remaining | 0x80));
                remaining >>= 7;
        	}

            _stream.WriteByte((byte)remaining);
        }

        void WriteSignedTag(int tag)
        {
        	uint remaining = ((uint)tag << 1) ^ (uint)(tag >> 31);

            while (remaining > 0x7f)
        	{
                _stream.WriteByte((byte)(remaining | 0x80));
                remaining >>= 7;
        	}

            _stream.WriteByte((byte)remaining);
        }

        void WriteSignedLongTag(long tag)
        {
        	ulong remaining = ((ulong)tag << 1) ^ (ulong)(tag >> 63);

            while (remaining > 0x7f)
        	{
                _stream.WriteByte((byte)(remaining | 0x80));
                remaining >>= 7;
        	}

            _stream.WriteByte((byte)remaining);
        }

        void WriteFloat(float value)
        {
            byte[] intelOrder = BitConverter.GetBytes(value);

            _streamBuffer[0] = intelOrder[3];
            _streamBuffer[1] = intelOrder[2];
            _streamBuffer[2] = intelOrder[1];
            _streamBuffer[3] = intelOrder[0];

            _stream.Write(_streamBuffer, 0, 4);
        }

        void WriteDouble(double value)
        {
            byte[] intelOrder = BitConverter.GetBytes(value);

            _streamBuffer[0] = intelOrder[7];
            _streamBuffer[1] = intelOrder[6];
            _streamBuffer[2] = intelOrder[5];
            _streamBuffer[3] = intelOrder[4];
            _streamBuffer[4] = intelOrder[3];
            _streamBuffer[5] = intelOrder[2];
            _streamBuffer[6] = intelOrder[1];
            _streamBuffer[7] = intelOrder[0];

            _stream.Write(_streamBuffer, 0, 8);
        }

        void WriteDecimal(decimal value)
        {
            int[] bits = decimal.GetBits(value);

            // Encode the mantissa:
            long lowBits = (long)(uint)bits[0];
            long middleLowBits = (long)((uint)bits[1] & 0x0001ffff) << 32;

            long middleHighBits = (long)((uint)bits[1] & 0xfffe0000) >> 17;
            long highBits = (long)(uint)bits[2] << 15;

            long low = lowBits | middleLowBits;
            long high = middleHighBits | highBits;

            if (high == 0)
            {
                long remaining = low;

                while (remaining > 0x7f)
                {
                    _stream.WriteByte((byte)(remaining | 0x80L));
                    remaining >>= 7;
                }

                _stream.WriteByte((byte)remaining);
            }
            else
            {
                long lowRemaining = low;
                int remainingOctets = 7;

                while (remainingOctets > 0)
                {
                    _stream.WriteByte((byte)(lowRemaining | 0x80L));
                    lowRemaining >>= 7;
                    remainingOctets--;
                }

                // Write the high word:
                long highRemaining = high;

                while (highRemaining > 0x7f)
                {
                    _stream.WriteByte((byte)(highRemaining | 0x80L));
                    highRemaining >>= 7;
                }

                _stream.WriteByte((byte)highRemaining);
            }

            // Encode the scale and sign:
            int scale = ((bits[3] & 0x7fff0000) >> 16);

            if (bits[3] < 0) scale = ~scale;

            WriteSignedTag(scale);
        }

        #endregion

        #region IPinchDecoder Members

        public void OpenUncountedContainer()
        {
        }

        public void OpenCountedContainer(int count)
        {
            if (count < 0) throw new ArgumentException("count");

            WriteUnsignedTag(count);
        }

        public void PrepareEncodeRequiredFloat32(float value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredFloat64(double value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredInt8(byte value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredInt16(short value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredInt32(int value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredInt64(long value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredDecimal(decimal value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredBool(bool value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value);
        }

        public void PrepareEncodeRequiredString(string value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredBytes(byte[] value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredEnumeration(object value, PinchFieldProperties properties)
        {
            // No header required.
        }

        public void PrepareEncodeRequiredStructure(object value, PinchFieldProperties properties)
        {
            WriteTwoHeaderBits(CodedFlags.HeaderStructurePresent);
        }
        
        public void PrepareEncodeOptionalFloat32(float? value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value.HasValue);
        }

        public void PrepareEncodeOptionalFloat64(double? value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value.HasValue);
        }

        public void PrepareEncodeOptionalInt8(byte? value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value.HasValue);
        }

        public void PrepareEncodeOptionalInt16(short? value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value.HasValue);
        }

        public void PrepareEncodeOptionalInt32(int? value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value.HasValue);
        }

        public void PrepareEncodeOptionalInt64(long? value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value.HasValue);
        }

        public void PrepareEncodeOptionalDecimal(decimal? value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value.HasValue);
        }

        public void PrepareEncodeOptionalBool(bool? value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value.HasValue);
            WriteOneHeaderBit(value.HasValue ? value.Value : false);
        }

        public void PrepareEncodeOptionalString(string value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value != null);
        }

        public void PrepareEncodeOptionalBytes(byte[] value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value != null);
        }

        public void PrepareEncodeOptionalEnumeration(object value, PinchFieldProperties properties)
        {
            WriteOneHeaderBit(value != null);
        }

        public void PrepareEncodeOptionalStructure(object value, PinchFieldProperties properties)
        {
            WriteTwoHeaderBits(value != null ? CodedFlags.HeaderStructurePresent : CodedFlags.HeaderStructureNotPresent);
        }

        public void PrepareContainer()
        {
            FlushHeaderBits();
        }

        public void EncodeRequiredFloat32(float value, PinchFieldProperties properties)
        {
            WriteFloat(value);
        }

        public void EncodeRequiredFloat64(double value, PinchFieldProperties properties)
        {
            WriteDouble(value);
        }

        public void EncodeRequiredInt8(byte value, PinchFieldProperties properties)
        {
            _stream.WriteByte(value);
        }

        public void EncodeRequiredInt16(short value, PinchFieldProperties properties)
        {
            WriteSignedTag(value);
        }

        public void EncodeRequiredInt32(int value, PinchFieldProperties properties)
        {
            WriteSignedTag(value);
        }

        public void EncodeRequiredInt64(long value, PinchFieldProperties properties)
        {
            WriteSignedLongTag(value);
        }

        public void EncodeRequiredDecimal(decimal value, PinchFieldProperties properties)
        {
            WriteDecimal(value);
        }

        public void EncodeRequiredBool(bool value, PinchFieldProperties properties)
        {
            // The boolean is encoded in the bit header.
        }

        public void EncodeRequiredString(string value, PinchFieldProperties properties)
        {
            if (value == null) throw new PinchNullRequiredFieldException();

            byte[] bytes = Encoding.UTF8.GetBytes(value);

            WriteUnsignedTag(bytes.Length);

            _stream.Write(bytes, 0, bytes.Length);
        }

        public void EncodeRequiredBytes(byte[] value, PinchFieldProperties properties)
        {
            if (value == null) throw new PinchNullRequiredFieldException();

            WriteUnsignedTag(value.Length);

            _stream.Write(value, 0, value.Length);
        }

        public void EncodeRequiredEnumeration(object value, PinchFieldProperties properties)
        {
            if (value == null) throw new PinchNullRequiredFieldException();

            WriteUnsignedTag((int)value);
        }

        public void EncodeRequiredStructure(object value, PinchFieldProperties properties)
        {
            if (value == null) throw new PinchNullRequiredFieldException();

            EncodeStructure(this, value, false);
        }

        public void EncodeOptionalFloat32(float? value, PinchFieldProperties properties)
        {
            if (value.HasValue) WriteFloat(value.Value);
        }

        public void EncodeOptionalFloat64(double? value, PinchFieldProperties properties)
        {
            if (value.HasValue) WriteDouble(value.Value);
        }

        public void EncodeOptionalInt8(byte? value, PinchFieldProperties properties)
        {
            if (value.HasValue) _stream.WriteByte(value.Value);
        }

        public void EncodeOptionalInt16(short? value, PinchFieldProperties properties)
        {
            if (value.HasValue) WriteSignedTag(value.Value);
        }

        public void EncodeOptionalInt32(int? value, PinchFieldProperties properties)
        {
            if (value.HasValue) WriteSignedTag(value.Value);
        }

        public void EncodeOptionalInt64(long? value, PinchFieldProperties properties)
        {
            if (value.HasValue) WriteSignedLongTag(value.Value);
        }

        public void EncodeOptionalDecimal(decimal? value, PinchFieldProperties properties)
        {
            if (value == null) return;

            WriteDecimal(value.Value);
        }

        public void EncodeOptionalBool(bool? value, PinchFieldProperties properties)
        {
            // The boolean is encoded in the bit header.
        }

        public void EncodeOptionalString(string value, PinchFieldProperties properties)
        {
            if (value == null) return;

            byte[] bytes = Encoding.UTF8.GetBytes(value);

            WriteUnsignedTag(bytes.Length);

            _stream.Write(bytes, 0, bytes.Length);
        }

        public void EncodeOptionalBytes(byte[] value, PinchFieldProperties properties)
        {
            if (value == null) return;

            WriteUnsignedTag(value.Length);

            _stream.Write(value, 0, value.Length);
        }

        public void EncodeOptionalEnumeration(object value, PinchFieldProperties properties)
        {
            if (value == null) return;

            WriteUnsignedTag((int)value);
        }

        public void EncodeOptionalStructure(object value, PinchFieldProperties properties)
        {
            if (value == null) return;

            EncodeStructure(this, value, false);
        }

        public void CloseContainer()
        {
        }

        #endregion

        #region Structure Encoding

        internal static void EncodeStructure(PinchEncoder encoder, object value, bool encodeHeader)
        {
            if (value is IPinchable)
            {
                IPinchable pinchable = value as IPinchable;

                if (encodeHeader) encoder.EncodeHeader(pinchable.ProtocolVersion);

                pinchable.Encode(encoder);
            }
            else
            {
                throw new NotImplementedException("Surrogates are not yet implemented.");
            }
        }

        internal void EncodeHeader(int protocolVersion)
        {
            WriteUnsignedTag(protocolVersion);
        }

        #endregion
    }
}
