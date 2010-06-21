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
        byte[] _decimalBuffer;

        public PinchEncoder(Stream stream)
        {
            _stream = stream;
            _streamBuffer = new byte[8];
            _decimalBuffer = null;
        }

        #region Encoding Utilities

        void WriteUnsignedTag(uint tag)
        {
            uint remaining = tag;

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

        #endregion

        #region Encoding Primatives

        void WriteSequenceMarker(int count)
        {
            if (count < 64)
            {
                _stream.WriteByte((byte)(PinchAssignedNumbers.PackedSequenceByte | count));
            }
            else
            {
                _stream.WriteByte(PinchAssignedNumbers.TaggedSequenceByte);

                WriteUnsignedTag((uint)count);
            }
        }

        void WritePrimativeBuffer(byte[] buffer, int offset, int length)
        {
            if (length < 64)
            {
                _stream.WriteByte((byte)(PinchAssignedNumbers.PackedPrimativeBufferByte | length));
            }
            else
            {
                _stream.WriteByte(PinchAssignedNumbers.TaggedPrimativeBufferByte);

                WriteUnsignedTag((uint)length);
            }

            _stream.Write(buffer, offset, length);
        }

        void WritePrimativeSignedOrdinal(int ordinal)
        {
            if (0 <= ordinal && ordinal < 64)
            {
                _stream.WriteByte((byte)(PinchAssignedNumbers.PackedPrimativeOrdinalByte | ordinal));
            }
            else
            {
                _stream.WriteByte(PinchAssignedNumbers.TaggedPrimativeOrdinalByte);

                WriteSignedTag(ordinal);
            }
        }

        void WritePrimativeUnsignedOrdinal(uint ordinal)
        {
            if (0 <= ordinal && ordinal < 64)
            {
                _stream.WriteByte((byte)(PinchAssignedNumbers.PackedPrimativeOrdinalByte | ordinal));
            }
            else
            {
                _stream.WriteByte(PinchAssignedNumbers.TaggedPrimativeOrdinalByte);

                WriteUnsignedTag(ordinal);
            }
        }

        void WritePrimativeLongOrdinal(long ordinal)
        {
            if (0 <= ordinal && ordinal < 64)
            {
                _stream.WriteByte((byte)(PinchAssignedNumbers.PackedPrimativeOrdinalByte | ordinal));
            }
            else
            {
                _stream.WriteByte(PinchAssignedNumbers.TaggedPrimativeOrdinalByte);

                WriteSignedLongTag(ordinal);
            }
        }

        void WriteNull()
        {
            _stream.WriteByte(PinchAssignedNumbers.Null);
        }

        public void EncodeChoiceMarker(int valueKind)
        {
            _stream.WriteByte(PinchAssignedNumbers.TaggedChoiceByte);

            WriteUnsignedTag((uint)valueKind);
        }

        #endregion

        #region Scaler Writing

        void WriteFloat(float value)
        {
            byte[] intelOrder = BitConverter.GetBytes(value);

            _streamBuffer[0] = intelOrder[3];
            _streamBuffer[1] = intelOrder[2];
            _streamBuffer[2] = intelOrder[1];
            _streamBuffer[3] = intelOrder[0];

            WritePrimativeBuffer(_streamBuffer, 0, 4);
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

            WritePrimativeBuffer(_streamBuffer, 0, 8);
        }

        void WriteDecimal(decimal value)
        {
            if (_decimalBuffer == null) _decimalBuffer = new byte[14];

            int[] bits = decimal.GetBits(value);

            // Write the two scale bytes in full:
            _decimalBuffer[0] = (byte)((bits[3] & 0x00ff0000) >> 16);
            _decimalBuffer[1] = (byte)((bits[3] & 0xff000000) >> 24);

            int decimalBufferUsed = 2;

            // Encode the mantissa:
            ulong low = ((ulong)(uint)bits[0]) | (((ulong)(uint)bits[1]) << 32);
            uint high = (uint)bits[2];

            if (high == 0)
            {
                ulong remaining = low;

                while (remaining > 0)
                {
                    _decimalBuffer[decimalBufferUsed++] = (byte)remaining;
                    remaining >>= 8;
                }
            }
            else
            {
                ulong lowRemaining = low;
                int remainingOctets = 8;

                while (remainingOctets > 0)
                {
                    _decimalBuffer[decimalBufferUsed++] = (byte)lowRemaining;
                    lowRemaining >>= 8;
                    remainingOctets--;
                }

                // Write the high word:
                uint highRemaining = high;

                while (highRemaining > 0)
                {
                    _decimalBuffer[decimalBufferUsed++] = (byte)highRemaining;
                    highRemaining >>= 8;
                }
            }

            WritePrimativeBuffer(_decimalBuffer, 0, decimalBufferUsed);
        }

        #endregion

        #region IPinchDecoder Members

        public void OpenSequence(int count)
        {
            if (count < 0) throw new ArgumentException("count");

            WriteSequenceMarker(count);
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
            WritePrimativeUnsignedOrdinal((uint)value);
        }

        public void EncodeRequiredInt16(short value, PinchFieldProperties properties)
        {
            WritePrimativeSignedOrdinal((int)value);
        }

        public void EncodeRequiredInt32(int value, PinchFieldProperties properties)
        {
            WritePrimativeSignedOrdinal(value);
        }

        public void EncodeRequiredInt64(long value, PinchFieldProperties properties)
        {
            WritePrimativeLongOrdinal(value);
        }

        public void EncodeRequiredDecimal(decimal value, PinchFieldProperties properties)
        {
            WriteDecimal(value);
        }

        public void EncodeRequiredBool(bool value, PinchFieldProperties properties)
        {
            WritePrimativeUnsignedOrdinal(value ? 1u : 0u);
        }

        public void EncodeRequiredString(string value, PinchFieldProperties properties)
        {
            if (value == null) throw new PinchNullRequiredFieldException();

            byte[] bytes = Encoding.UTF8.GetBytes(value);

            WritePrimativeBuffer(bytes, 0, bytes.Length);
        }

        public void EncodeRequiredBytes(byte[] value, PinchFieldProperties properties)
        {
            if (value == null) throw new PinchNullRequiredFieldException();

            WritePrimativeBuffer(value, 0, value.Length);
        }

        public void EncodeRequiredEnumeration(object value, PinchFieldProperties properties)
        {
            if (value == null) throw new PinchNullRequiredFieldException();

            WritePrimativeUnsignedOrdinal((uint)(int)value);
        }

        public void EncodeRequiredStructure(object value, PinchFieldProperties properties)
        {
            if (value == null) throw new PinchNullRequiredFieldException();

            EncodeStructure(this, value, false);
        }

        public void EncodeOptionalFloat32(float? value, PinchFieldProperties properties)
        {
            if (value.HasValue)
            {
                WriteFloat(value.Value);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalFloat64(double? value, PinchFieldProperties properties)
        {
            if (value.HasValue)
            {
                WriteDouble(value.Value);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalInt8(byte? value, PinchFieldProperties properties)
        {
            if (value.HasValue)
            {
                WritePrimativeUnsignedOrdinal((uint)value.Value);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalInt16(short? value, PinchFieldProperties properties)
        {
            if (value.HasValue)
            {
                WritePrimativeSignedOrdinal((int)value.Value);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalInt32(int? value, PinchFieldProperties properties)
        {
            if (value.HasValue)
            {
                WritePrimativeSignedOrdinal(value.Value);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalInt64(long? value, PinchFieldProperties properties)
        {
            if (value.HasValue)
            {
                WritePrimativeLongOrdinal(value.Value);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalDecimal(decimal? value, PinchFieldProperties properties)
        {
            if (value.HasValue)
            {
                WriteDecimal(value.Value);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalBool(bool? value, PinchFieldProperties properties)
        {
            if (value.HasValue)
            {
                WritePrimativeUnsignedOrdinal(value.Value ? 1u : 0u);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalString(string value, PinchFieldProperties properties)
        {
            if (value != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);

                WritePrimativeBuffer(bytes, 0, bytes.Length);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalBytes(byte[] value, PinchFieldProperties properties)
        {
            if (value != null)
            {
                WritePrimativeBuffer(value, 0, value.Length);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalEnumeration(object value, PinchFieldProperties properties)
        {
            if (value != null)
            {
                WritePrimativeUnsignedOrdinal((uint)(int)value);
            }
            else
            {
                WriteNull();
            }
        }

        public void EncodeOptionalStructure(object value, PinchFieldProperties properties)
        {
            if (value != null)
            {
                EncodeStructure(this, value, false);
            }
            else
            {
                WriteNull();
            }
        }

        public void CloseSequence()
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
        }

        #endregion
    }
}
