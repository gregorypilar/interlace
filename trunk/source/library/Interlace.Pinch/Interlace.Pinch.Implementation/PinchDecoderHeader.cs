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
    class PinchDecoderHeader
    {
        int _bitHeaderBitCount;
        byte[] _bitHeaderBytes;
        int _bitHeaderByteCount;

        byte[] _upperSmallCachedHeader;

        int _bitHeaderUsed;

        public PinchDecoderHeader(int bitsInHeader, byte[] smallCachedHeader, Stream stream)
        {
            _bitHeaderBitCount = bitsInHeader;
            _bitHeaderByteCount = _bitHeaderBitCount / 8 + ((_bitHeaderBitCount % 8) == 0 ? 0 : 1);

            // Avoid allocating small byte arrays by keeping a cached one around and 
            // using it if it is large enough to fit the header in:
            if (_bitHeaderByteCount <= smallCachedHeader.Length)
            {
                _bitHeaderBytes = smallCachedHeader;
            }
            else
            {
                _bitHeaderBytes = new byte[_bitHeaderByteCount];
            }

            int totalRead = 0;

            while (totalRead < _bitHeaderByteCount)
            {
                int read = stream.Read(_bitHeaderBytes, totalRead, _bitHeaderByteCount - totalRead);

                if (read == 0) throw new PinchEndOfStreamException();

                totalRead += read;
            }

            _bitHeaderUsed = 0;
        }

        public byte[] UpperSmallCachedHeader
        { 	 
            get 
            {
                if (_upperSmallCachedHeader == null) _upperSmallCachedHeader = new byte[32];

                return _upperSmallCachedHeader; 
            }
        }

        public bool ReadOneHeaderBit()
        {
            Debug.Assert(_bitHeaderUsed < _bitHeaderBitCount);

            int bit = _bitHeaderBytes[_bitHeaderUsed / 8] >> (_bitHeaderUsed % 8);

            _bitHeaderUsed++;

            return (bit & 1) == 1;
        }

        public int ReadTwoHeaderBits()
        {
            int bits =
                ((_bitHeaderBytes[_bitHeaderUsed / 8] >> (_bitHeaderUsed % 8)) & 1) |
                (((_bitHeaderBytes[_bitHeaderUsed / 8] >> (_bitHeaderUsed % 8)) << 1) & 2);

            _bitHeaderUsed += 2;

            return bits;
        }

    }
}
