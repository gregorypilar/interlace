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
using System.Net;
using System.Text;

using Interlace.ReactorCore;

#endregion

namespace Interlace.Sharpcap
{
    public abstract class RemoteCaptureFramingProtocol : Protocol
    {
        public const byte RPCAP_VERSION = 0;

        public const byte RPCAP_MSG_ERROR = 1;
        public const byte RPCAP_MSG_FINDALLIF_REQ = 2;
        public const byte RPCAP_MSG_OPEN_REQ = 3;
        public const byte RPCAP_MSG_STARTCAP_REQ = 4;
        public const byte RPCAP_MSG_UPDATEFILTER_REQ = 5;
        public const byte RPCAP_MSG_CLOSE = 6;
        public const byte RPCAP_MSG_PACKET = 7;
        public const byte RPCAP_MSG_AUTH_REQ = 8;
        public const byte RPCAP_MSG_STATS_REQ = 9;
        public const byte RPCAP_MSG_ENDCAP_REQ = 10;
        public const byte RPCAP_MSG_SETSAMPLING_REQ = 11;

        public const byte RPCAP_MSG_FINDALLIF_REPLY = 128 + RPCAP_MSG_FINDALLIF_REQ;
        public const byte RPCAP_MSG_OPEN_REPLY = 128 + RPCAP_MSG_OPEN_REQ;
        public const byte RPCAP_MSG_STARTCAP_REPLY = 128 + RPCAP_MSG_STARTCAP_REQ;
        public const byte RPCAP_MSG_UPDATEFILTER_REPLY = 128 + RPCAP_MSG_UPDATEFILTER_REQ;
        public const byte RPCAP_MSG_AUTH_REPLY = 128 + RPCAP_MSG_AUTH_REQ;
        public const byte RPCAP_MSG_STATS_REPLY = 128 + RPCAP_MSG_STATS_REQ;
        public const byte RPCAP_MSG_ENDCAP_REPLY = 128 + RPCAP_MSG_ENDCAP_REQ;
        public const byte RPCAP_MSG_SETSAMPLING_REPLY = 128 + RPCAP_MSG_SETSAMPLING_REQ;

        public const uint DLT_PPP_SERIAL = 50;
        public const uint DLT_EN10MB = 1;

        byte[] _receiveBuffer;
        bool _receivedHeader;
        int _receiveBufferUsed;

        byte _frameVersion;
        byte _frameType;
        ushort _frameValue;

        public RemoteCaptureFramingProtocol()
        {
            StartReceivingHeader();
        }

        void StartReceivingHeader()
        {
            _receiveBuffer = new byte[8];
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
                        HandleReceivedFrame(_frameVersion, _frameType, _frameValue, _receiveBuffer);

                        StartReceivingHeader();
                    }
                }
            }
        }

        void ProcessHeader()
        {
            _frameVersion = _receiveBuffer[0];
            _frameType = _receiveBuffer[1];
            _frameValue = ByteOrder.NetworkToHost(BitConverter.ToUInt16(_receiveBuffer, 2));

            uint receiveSize = ByteOrder.NetworkToHost(
                BitConverter.ToUInt32(_receiveBuffer, 4));

            _receiveBuffer = new byte[receiveSize];
            _receivedHeader = true;
            _receiveBufferUsed = 0;
        }

        protected abstract void HandleReceivedFrame(byte frameVersion, byte frameType, ushort frameValue, byte[] data);

        protected MemoryStream PrepareSendFrame(byte frameVersion, byte frameType, ushort frameValue)
        {
            MemoryStream stream = new MemoryStream();

            stream.WriteByte(frameVersion);
            stream.WriteByte(frameType);
            stream.Write(BitConverter.GetBytes(ByteOrder.HostToNetwork(frameValue)), 0, 2);

            stream.Write(new byte[4], 0, 4);

            return stream;
        }

        protected void CompleteSendFrame(MemoryStream stream)
        {
            uint networkOrderLength = ByteOrder.NetworkToHost((uint)(stream.Length - 8));
            stream.Seek(4, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes(networkOrderLength), 0, 4);

            byte[] streamArray = stream.ToArray();
            Connection.Send(streamArray, 0, streamArray.Length, false);
        }

        protected void SendEmptyFrame(byte frameVersion, byte frameType, ushort frameValue)
        {
            MemoryStream stream = PrepareSendFrame(frameVersion, frameType, frameValue);

            CompleteSendFrame(stream);
        }
    }
}
