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

#endregion

// Portions of this code were originally developed for Bit Plantation BitLibrary.
// (Portions Copyright © 2006 Bit Plantation)

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

using Interlace.ReactorCore;
using Interlace.ReactorService;

namespace Interlace.ReactorUtilities
{
    public abstract class FrameProtocol : Protocol
    {
        bool _nativeEndian;

        byte[] _receiveBuffer;
        bool _receivedHeader;
        int _receiveBufferUsed;

        int _maximumFrameSize = 2097152;

        public FrameProtocol()
        {
            _nativeEndian = false;

            StartReceivingHeader();
        }

        public int MaximumFrameSize
        {
            get { return _maximumFrameSize; }
            set { _maximumFrameSize = value; }
        }

        public bool NativeEndian
        {
            get { return _nativeEndian; }
            set { _nativeEndian = value; }
        }

        void StartReceivingHeader()
        {
            _receiveBuffer = new byte[4];
            _receivedHeader = false;
            _receiveBufferUsed = 0;
        }

        protected internal override void DataReceived(IPEndPoint endPoint, byte[] data, int offset, int length)
        {
            int newBytesUsed = 0;

            while (length - newBytesUsed > 0)
            {
                int bytesToCopy = Math.Min(_receiveBuffer.Length - _receiveBufferUsed,
                    length - newBytesUsed);

                Array.Copy(data, newBytesUsed, _receiveBuffer, _receiveBufferUsed, bytesToCopy);

                _receiveBufferUsed += bytesToCopy;
                newBytesUsed += bytesToCopy;

                if (_receiveBufferUsed == _receiveBuffer.Length)
                {
                    if (!_receivedHeader)
                    {
                        ProcessHeader();
                    }
                    else
                    {
                        HandleReceivedFrame(_receiveBuffer);

                        StartReceivingHeader();
                    }
                }
            }
        }

        void ProcessHeader()
        {
            uint receiveSize;

            if (!_nativeEndian)
            {
                receiveSize = (uint)IPAddress.NetworkToHostOrder((int)BitConverter.ToUInt32(_receiveBuffer, 0));
            }
            else
            {
                receiveSize = BitConverter.ToUInt32(_receiveBuffer, 0);
            }

            if (receiveSize > _maximumFrameSize)
            {
                Connection.LoseConnection();

                throw new InvalidDataException(
                    string.Format("Frame length ({0} bytes) exceeds maximum allowed frame rate of {1} bytes", receiveSize, _maximumFrameSize)
                    );
            }

            _receiveBuffer = new byte[receiveSize];
            _receivedHeader = true;
            _receiveBufferUsed = 0;
        }

        protected abstract void HandleReceivedFrame(byte[] data);

        protected MemoryStream PrepareSendFrame()
        {
            MemoryStream stream = new MemoryStream();

            stream.Write(new byte[4], 0, 4);

            return stream;
        }

        protected void CompleteSendFrame(MemoryStream stream)
        {
            uint networkOrderLength;
            
            if (!_nativeEndian)
            {
                networkOrderLength = (uint)IPAddress.HostToNetworkOrder((int)(stream.Length - 4));
            }
            else
            {
                networkOrderLength = (uint)(stream.Length - 4);
            }

            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes(networkOrderLength), 0, 4);

            byte[] streamArray = stream.ToArray();
            Connection.Send(streamArray, 0, streamArray.Length, false);
        }
    }
}
