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
using System.Net;
using System.Text;

#endregion

namespace Interlace.ReactorCore
{
    public class SocketSendBuffer
    {
        EndPoint _endPointOrNull;
        ArraySegment<byte> _data;

        public SocketSendBuffer(EndPoint endPointOrNull, byte[] data, int offset, int length, bool copyData)
        {
            _endPointOrNull = endPointOrNull;

            if (!copyData)
            {
                _data = new ArraySegment<byte>(data, offset, length);
            }
            else
            {
                byte[] copiedData = new byte[length];

                Array.Copy(data, offset, copiedData, 0, length);

                _data = new ArraySegment<byte>(copiedData, 0, length);
            }
        }

        public EndPoint EndPointOrNull
        { 	 
           get { return _endPointOrNull; }
        }

        public ArraySegment<byte> Data
        { 	 
           get { return _data; }
        }

        public SocketSendBuffer WithConsumedBytes(int bytesToConsume)
        {
            if (bytesToConsume > _data.Count)
            {
                throw new InvalidOperationException(
                    "An attempt was made to consume more bytes from a socket transmit buffer than are in the buffer.");
            }

            return new SocketSendBuffer(_endPointOrNull, _data.Array, _data.Offset + bytesToConsume, 
                _data.Count - bytesToConsume, false);
        }
    }
}
