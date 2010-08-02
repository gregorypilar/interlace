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

using Interlace.Collections;
using Interlace.ReactorCore;

#endregion

namespace Interlace.Sharpcap
{
    public class RemoteCaptureProtocol : RemoteCaptureFramingProtocol
    {
        RemoteCaptureService _service;

        ConnectorHandle _dataConnectorHandle = null;
        Set<RemoteCaptureDataProtocol> _dataProtocols = new Set<RemoteCaptureDataProtocol>();

        public RemoteCaptureProtocol(RemoteCaptureService service)
        {
            _service = service;
        }

        protected override void HandleReceivedFrame(byte frameVersion, byte frameType, ushort frameValue, byte[] data)
        {
            if (frameValue != RPCAP_VERSION) throw new NotImplementedException();

            switch (frameType)
            {
                case RPCAP_MSG_AUTH_REQ:
                    SendEmptyFrame(RPCAP_VERSION, RPCAP_MSG_AUTH_REPLY, 0);
                    break;

                case RPCAP_MSG_OPEN_REQ:
                    HandleOpenRequest(data);
                    break;

                case RPCAP_MSG_STARTCAP_REQ:
                    HandleStartCaptureRequest(data);
                    break;

                default:
                    throw new NotImplementedException(
                        string.Format("The message type {0} is not supported.", frameType));
            }
        }

        void HandleOpenRequest(byte[] data)
        {
            string deviceName = Encoding.UTF8.GetString(data);

            MemoryStream stream = PrepareSendFrame(RPCAP_VERSION, RPCAP_MSG_OPEN_REPLY, 0);
            NetworkWriter writer = new NetworkWriter(stream);

            uint linkType = DLT_EN10MB;
            uint offsetFromUtc = 0;

            writer.WriteUnsigned32(linkType);
            writer.WriteUnsigned32(offsetFromUtc);

            CompleteSendFrame(stream);
        }

        void HandleStartCaptureRequest(byte[] data)
        {
            // Read in the various fields:
            NetworkReader reader = new NetworkReader(data);

            uint snapLength = reader.ReadUnsigned32();
            uint readTimeout = reader.ReadUnsigned32();
            ushort flags = reader.ReadUnsigned16();
            ushort clientDataPortOrZero = reader.ReadUnsigned16();

            ushort filterType = reader.ReadUnsigned16();
            ushort dummy = reader.ReadUnsigned16();
            uint numberOfItems = reader.ReadUnsigned32();

            for (int i = 0; i < numberOfItems; i++)
            {
                ushort code = reader.ReadUnsigned16();
                byte jumpTrue = reader.ReadUnsigned8();
                byte jumpFalse = reader.ReadUnsigned8();
                int instructionValue = reader.ReadSigned32();
            }

            // Set up a data port:
            if (_dataConnectorHandle == null)
            {
                _dataConnectorHandle = _service.Reactor.ListenStream(new RemoteCaptureProtocolDataFactory(this));
            }

            int bufferSize = (int)snapLength;
            ushort serverDataPort = (ushort)_dataConnectorHandle.ListeningOnPort;

            MemoryStream stream = PrepareSendFrame(RPCAP_VERSION, RPCAP_MSG_STARTCAP_REPLY, 0);
            NetworkWriter writer = new NetworkWriter(stream);

            writer.WriteSigned32(bufferSize);
            writer.WriteUnsigned16(serverDataPort);
            writer.WriteUnsigned16(0); // (This is a reserved field.)

            CompleteSendFrame(stream);
        }

        protected internal override void ConnectionLost(CloseReason reason)
        {
            if (_dataConnectorHandle != null)
            {
                _dataConnectorHandle.Close();

                _dataConnectorHandle = null;
            }

            base.ConnectionLost(reason);
        }

        internal void HandleDataConnectionMade(RemoteCaptureDataProtocol dataProtocol)
        {
            _dataProtocols.UnionUpdate(dataProtocol);

            if (_dataConnectorHandle != null)
            {
                _dataConnectorHandle.Close();

                _dataConnectorHandle = null;
            }

            _service.AddListener(dataProtocol);
        }

        internal void HandleDataConnectionLost(RemoteCaptureDataProtocol dataProtocol)
        {
            _service.RemoveListener(dataProtocol);

            _dataProtocols.DifferenceUpdate(dataProtocol);
        }
    }
}
