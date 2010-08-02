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
using System.Text;

using Interlace.ReactorCore;

#endregion

namespace Interlace.Sharpcap
{
    public class RemoteCaptureDataProtocol : RemoteCaptureFramingProtocol, ICaptureListener
    {
        RemoteCaptureProtocol _parentProtocol;

        DateTime _connectedAt;
        int _snapLength = 0xffff;
        uint _packetNumber = 1;

        public RemoteCaptureDataProtocol(RemoteCaptureProtocol parentProtocol)
        {
            _parentProtocol = parentProtocol;
        }

        protected internal override void ConnectionMade()
        {
            base.ConnectionMade();

            _connectedAt = DateTime.UtcNow;

            _parentProtocol.HandleDataConnectionMade(this);
        }

        protected internal override void ConnectionLost(CloseReason reason)
        {
            _parentProtocol.HandleDataConnectionLost(this);

            base.ConnectionLost(reason);
        }

        protected override void HandleReceivedFrame(byte frameVersion, byte frameType, ushort frameValue, byte[] data)
        {
            throw new NotImplementedException();
        }

        #region ICaptureListener Members

        public void HandleCapture(byte[] buffer, int offset, int length)
        {
            TimeSpan timeOffset = DateTime.UtcNow - _connectedAt;

            MemoryStream stream = PrepareSendFrame(RPCAP_VERSION, RPCAP_MSG_PACKET, 0);
            NetworkWriter writer = new NetworkWriter(stream);

            uint seconds = (uint)Math.Floor(timeOffset.TotalSeconds);
            uint microseconds = (uint)((timeOffset.Ticks % TimeSpan.TicksPerSecond) / 10L);

            int bytesToTransmit = Math.Min(_snapLength, length);

            writer.WriteUnsigned32(seconds);
            writer.WriteUnsigned32(microseconds);
            writer.WriteUnsigned32((uint)bytesToTransmit);
            writer.WriteUnsigned32((uint)length);
            writer.WriteUnsigned32(_packetNumber++);

            stream.Write(buffer, offset, bytesToTransmit);

            CompleteSendFrame(stream);
        }

        #endregion
    }
}
