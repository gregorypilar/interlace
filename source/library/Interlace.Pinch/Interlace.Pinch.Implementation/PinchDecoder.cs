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

        public PinchDecoder(Stream stream)
        {
            _stream = stream;
            _streamBuffer = new byte[8];
            _intelOrderBuffer = new byte[8];
        }

        #region Decoding Primitives

        enum TokenKind
        {
            Sequence, // (The argument is the number of tokens in the sequence, which must then be read.)
            PrimitiveBuffer, // (The argument is the number of bytes that follow in the buffer.)
            PrimitivePackedOrdinal, // (The argument is the ordinal value; nothing needs to be read.)
            PrimitiveTaggedOrdinal, // (No argument; the ordinal value must be read.)
            Choice, // (The value kind is the argument; the structure follows.)
            Null // (No argument and nothing to read after.)
        }

        bool _readRequired = true;
        TokenKind _readTokenKind;
        int _readTokenArgument;

        void ReadTokenInternal()
        {
            int readByte = _stream.ReadByte();

            if (readByte == -1) throw new PinchEndOfStreamException();

            byte tokenByte = (byte)readByte;
            byte maskedTokenByte = (byte)(tokenByte & PinchAssignedNumbers.PackedByteKindMask);

            switch (maskedTokenByte)
            {
                case PinchAssignedNumbers.PackedPrimativeOrdinalByte:
                    _readTokenKind = TokenKind.PrimitivePackedOrdinal;
                    _readTokenArgument = tokenByte & PinchAssignedNumbers.PackedByteValueMask;
                    break;

                case PinchAssignedNumbers.PackedPrimativeBufferByte:
                    _readTokenKind = TokenKind.PrimitiveBuffer;
                    _readTokenArgument = tokenByte & PinchAssignedNumbers.PackedByteValueMask;
                    break;

                case PinchAssignedNumbers.PackedSequenceByte:
                    _readTokenKind = TokenKind.Sequence;
                    _readTokenArgument = tokenByte & PinchAssignedNumbers.PackedByteValueMask;
                    break;

                default:
                    switch (tokenByte)
                    {
                        case PinchAssignedNumbers.Null:
                            _readTokenKind = TokenKind.Null;
                            break;

                        case PinchAssignedNumbers.TaggedPrimativeOrdinalByte:
                            _readTokenKind = TokenKind.PrimitiveTaggedOrdinal;
                            break;

                        case PinchAssignedNumbers.TaggedPrimativeBufferByte:
                            _readTokenKind = TokenKind.PrimitiveBuffer;
                            _readTokenArgument = (int)ReadUnsignedTag();
                            break;

                        case PinchAssignedNumbers.TaggedSequenceByte:
                            _readTokenArgument = (int)ReadUnsignedTag();
                            _readTokenKind = TokenKind.Sequence;
                            break;

                        case PinchAssignedNumbers.TaggedChoiceByte:
                            _readTokenKind = TokenKind.Choice;
                            _readTokenArgument = (int)ReadUnsignedTag();
                            break;
                    }

                    break;
            }
        }

        TokenKind PeekToken()
        {
            if (_readRequired) ReadTokenInternal();

            _readRequired = false;

            return _readTokenKind;
        }

        TokenKind ReadToken()
        {
            if (_readRequired) ReadTokenInternal();

            _readRequired = true;

            return _readTokenKind;
        }

        #endregion

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

        void SkipBytes(int count)
        {
            if (_stream.CanSeek && count > 32)
            {
                // (Seeking may be more expensive than a small read in some situations.)
                _stream.Seek(count, SeekOrigin.Current);
            }
            else
            {
                byte[] bytes = new byte[Math.Min(count, 1024)];

                int totalRead = 0;

                while (totalRead < count)
                {
                    int read = _stream.Read(bytes, totalRead, Math.Min(count - totalRead, 1024));

                    if (read == 0) throw new PinchEndOfStreamException();

                    totalRead += read;
                }
            }
        }

        uint ReadUnsignedTag()
        {
        	int shift = 0;
        	uint tag = 0;
            int readByte;

            do
            {
                readByte = _stream.ReadByte();

                if (readByte == -1) throw new PinchEndOfStreamException();
                if (shift >= 31) throw new PinchInvalidCodingException();

                tag |= (uint)(readByte & 0x7f) << shift;

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

        void SkipTag()
        {
            int readByte;

            do
            {
                readByte = _stream.ReadByte();

                if (readByte == -1) throw new PinchEndOfStreamException();
            }
            while ((readByte & 0x80) != 0);
        }

        decimal ReadDecimal()
        {
            // Read the buffer token and length, ensuring it has enough bytes to be valid:
            TokenKind token = ReadToken();

            if (token != TokenKind.PrimitiveBuffer || _readTokenArgument < 2) throw new PinchInvalidCodingException();

            BufferBytes(2);

            byte scale = _streamBuffer[0];
            bool isNegative = (_streamBuffer[1] & 0x80) != 0;

            int bufferLength = _readTokenArgument;
            int bufferUsed = 2;

            // Read the low bits:
        	int lowShift = 0;
            int lowReadByte;
        	ulong low = 0;

            while (lowShift != 64 && bufferUsed != bufferLength)
            {
                lowReadByte = _stream.ReadByte();
                bufferUsed++;

                if (lowReadByte == -1) throw new PinchEndOfStreamException();

                low |= (ulong)((byte)lowReadByte) << lowShift;

                lowShift += 8;
            }

            // If the last low octet had a continuation bit, continue reading into high:
            int highShift = 0;
            int highReadByte;
            uint high = 0;

            while (bufferUsed != bufferLength)
            {
                highReadByte = _stream.ReadByte();
                bufferUsed++;

                if (highReadByte == -1) throw new PinchEndOfStreamException();
                if (highShift == 32) throw new PinchInvalidCodingException();

                high |= (uint)((byte)highReadByte) << highShift;

                highShift += 8;
            }

            int intLow = (int)(uint)low;
            int intMiddle = (int)(uint)(low >> 32);
            int intHigh = (int)high;

            return new decimal(intLow, intMiddle, intHigh, isNegative, scale);
        }

        #endregion

        #region Decoding Assistants

        void BufferPrimitiveBuffer(int expectedLength)
        {
            if (expectedLength > _streamBuffer.Length) throw new InvalidOperationException();

            TokenKind token = ReadToken();

            if (token != TokenKind.PrimitiveBuffer) throw new PinchInvalidCodingException();
            if (_readTokenArgument != expectedLength) throw new PinchInvalidCodingException();

            BufferBytes(expectedLength);
        }

        byte[] ReadPrimitiveBuffer()
        {
            TokenKind token = ReadToken();

            if (token != TokenKind.PrimitiveBuffer) throw new PinchInvalidCodingException();

            return ReadBytes(_readTokenArgument);
        }

        int ReadPrimitiveSignedOrdinal()
        {
            TokenKind token = ReadToken();

            if (token == TokenKind.PrimitivePackedOrdinal)
            {
                return _readTokenArgument;
            }
            else if (token == TokenKind.PrimitiveTaggedOrdinal)
            {
                return ReadSignedTag();
            }
            else
            {
                throw new PinchInvalidCodingException();
            }
        }

        uint ReadPrimitiveUnsignedOrdinal()
        {
            TokenKind token = ReadToken();

            if (token == TokenKind.PrimitivePackedOrdinal)
            {
                return (uint)_readTokenArgument;
            }
            else if (token == TokenKind.PrimitiveTaggedOrdinal)
            {
                return ReadUnsignedTag();
            }
            else
            {
                throw new PinchInvalidCodingException();
            }
        }

        long ReadPrimitiveLongOrdinal()
        {
            TokenKind token = ReadToken();

            if (token == TokenKind.PrimitivePackedOrdinal)
            {
                return _readTokenArgument;
            }
            else if (token == TokenKind.PrimitiveTaggedOrdinal)
            {
                return ReadSignedLongTag();
            }
            else
            {
                throw new PinchInvalidCodingException();
            }
        }

        #endregion

        #region IPinchDecoder Implementation

        public int OpenSequence()
        {
            TokenKind token = ReadToken();

            if (token != TokenKind.Sequence) throw new PinchInvalidCodingException();

            return _readTokenArgument;
        }

        public int? OpenOptionalSequence()
        {
            TokenKind token = ReadToken();

            if (token == TokenKind.Null) return null;

            if (token != TokenKind.Sequence) throw new PinchInvalidCodingException();

            return _readTokenArgument;
        }

        public int DecodeChoiceMarker()
        {
            TokenKind token = ReadToken();

            if (token != TokenKind.Choice) throw new PinchInvalidCodingException();

            return _readTokenArgument;
        }

        public int? DecodeOptionalChoiceMarker()
        {
            TokenKind token = ReadToken();

            if (token == TokenKind.Null) return null;

            if (token != TokenKind.Choice) throw new PinchInvalidCodingException();

            return _readTokenArgument;
        }

        public float DecodeRequiredFloat32(PinchFieldProperties properties)
        {
            BufferPrimitiveBuffer(4);

            _intelOrderBuffer[0] = _streamBuffer[3];
            _intelOrderBuffer[1] = _streamBuffer[2];
            _intelOrderBuffer[2] = _streamBuffer[1];
            _intelOrderBuffer[3] = _streamBuffer[0];

            return BitConverter.ToSingle(_intelOrderBuffer, 0);
        }

        public double DecodeRequiredFloat64(PinchFieldProperties properties)
        {
            BufferPrimitiveBuffer(8);

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
            return (byte)ReadPrimitiveUnsignedOrdinal();
        }

        public short DecodeRequiredInt16(PinchFieldProperties properties)
        {
            return (short)ReadPrimitiveSignedOrdinal();
        }

        public int DecodeRequiredInt32(PinchFieldProperties properties)
        {
            return ReadPrimitiveSignedOrdinal();
        }

        public long DecodeRequiredInt64(PinchFieldProperties properties)
        {
            return ReadPrimitiveLongOrdinal();
        }

        public decimal DecodeRequiredDecimal(PinchFieldProperties properties)
        {
            return ReadDecimal();
        }

        public bool DecodeRequiredBool(PinchFieldProperties properties)
        {
            return ReadPrimitiveUnsignedOrdinal() == 1;
        }

        public string DecodeRequiredString(PinchFieldProperties properties)
        {
            byte[] bytes = ReadPrimitiveBuffer();

            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] DecodeRequiredBytes(PinchFieldProperties properties)
        {
            return ReadPrimitiveBuffer();
        }

        public int DecodeRequiredEnumeration(PinchFieldProperties properties)
        {
            return (int)ReadPrimitiveUnsignedOrdinal();
        }

        public object DecodeRequiredStructure(IPinchableFactory factory, PinchFieldProperties properties)
        {
            return PinchDecoder.DecodeStructure(this, factory, false);
        }

        bool IsOptionalFieldPresent()
        {
            return PeekToken() != TokenKind.Null;
        }

        void ReadNull()
        {
            TokenKind token = ReadToken();

            if (token != TokenKind.Null) throw new InvalidOperationException();
        }

        public float? DecodeOptionalFloat32(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                BufferPrimitiveBuffer(4);

                _intelOrderBuffer[0] = _streamBuffer[3];
                _intelOrderBuffer[1] = _streamBuffer[2];
                _intelOrderBuffer[2] = _streamBuffer[1];
                _intelOrderBuffer[3] = _streamBuffer[0];

                return BitConverter.ToSingle(_intelOrderBuffer, 0);
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public double? DecodeOptionalFloat64(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                BufferPrimitiveBuffer(8);

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
                ReadNull();

                return null;
            }
        }

        public byte? DecodeOptionalInt8(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                return (byte)ReadPrimitiveUnsignedOrdinal();
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public short? DecodeOptionalInt16(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                return (short)ReadPrimitiveSignedOrdinal();
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public int? DecodeOptionalInt32(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                return ReadPrimitiveSignedOrdinal();
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public long? DecodeOptionalInt64(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                return ReadPrimitiveLongOrdinal();
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public decimal? DecodeOptionalDecimal(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                return ReadDecimal();
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public bool? DecodeOptionalBool(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                return ReadPrimitiveUnsignedOrdinal() == 1;
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public string DecodeOptionalString(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                byte[] bytes = ReadPrimitiveBuffer();

                return Encoding.UTF8.GetString(bytes);
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public byte[] DecodeOptionalBytes(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                return ReadPrimitiveBuffer();
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public int? DecodeOptionalEnumeration(PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                return (int)ReadPrimitiveUnsignedOrdinal();
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public object DecodeOptionalStructure(IPinchableFactory factory, PinchFieldProperties properties)
        {
            if (IsOptionalFieldPresent())
            {
                return PinchDecoder.DecodeStructure(this, factory, false);
            }
            else
            {
                ReadNull();

                return null;
            }
        }

        public void SkipFields(int remainingFields)
        {
            for (int i = 0; i < remainingFields; i++)
            {
                TokenKind token = ReadToken();

                switch (token)
                {
                    case TokenKind.Sequence:
                        SkipFields(_readTokenArgument);
                        break;

                    case TokenKind.PrimitiveBuffer:
                        SkipBytes(_readTokenArgument);
                        break;

                    case TokenKind.PrimitivePackedOrdinal:
                        break;

                    case TokenKind.PrimitiveTaggedOrdinal:
                        SkipTag();
                        break;

                    case TokenKind.Choice:
                        SkipTag();
                        SkipFields(1);
                        break;

                    case TokenKind.Null:
                        break;
                }
            }
        }

        public void SkipRemoved()
        {
            SkipFields(1);
        }

        public void CloseSequence()
        {
        }

        #endregion

        internal static object DecodeStructure(PinchDecoder pinchDecoder, IPinchableFactory factory, bool expectHeader)
        {
            object value = factory.Create(null);

            if (value is IPinchable)
            {
                IPinchable pinchable = value as IPinchable;

                /*
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
                */

                pinchable.Decode(pinchDecoder);
            }
            else
            {
                throw new InvalidOperationException();
            }

            return value;
        }

        internal void DecodeHeader(out int protocolVersion)
        {
            protocolVersion = 0;
        }
    }
}
