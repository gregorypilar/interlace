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
using System.Net.Sockets;
using System.Text;

#endregion

namespace Interlace.ReactorCore
{
    public class DatagramSocketConnection : SocketConnection
    {
        EndPoint _receiveEndpoint;
        bool _truncationOccurred = false;

        public DatagramSocketConnection(IReactor reactor, Protocol protocol)
        : base(reactor, protocol)
        {
        }

        public bool TruncationOccurred
        { 	 
           get { return _truncationOccurred; }
        }

        bool IsTemporaryError(SocketError error)
        {
            switch (error)
            {
                case SocketError.ConnectionReset:
                case SocketError.ConnectionRefused:
                    return true;

                default:
                    return false;
            }
        }

        protected override void ContinueReceive()
        {
            IAsyncResult result;

            try
            {
                _receiveEndpoint = new IPEndPoint(IPAddress.Any, 0);

                result = _socket.BeginReceiveFrom(_receiveBuffer, 0,
                    _receiveBuffer.Length, SocketFlags.None, ref _receiveEndpoint, null, null);
            }
            catch (SocketException ex)
            {
                // Some errors (for instance, the error from sending to a closed 
                // port on localhost) should be ignored:
                if (IsTemporaryError(ex.SocketErrorCode))
                {
                    ContinueReceive();

                    return;
                }

                HandleSocketException(ex);

                return;
            }
            catch (ObjectDisposedException ex)
            {
                HandleSocketClosed(ex);

                return;
            }

            _reactor.AddResult(result, ReceiveCompleted);
        }

        void ReceiveCompleted(IAsyncResult result, object state)
        {
            int bytesReceived;

            try
            {
                bytesReceived = _socket.EndReceiveFrom(result, ref _receiveEndpoint);
            }
            catch (SocketException ex)
            {
                // Some errors (for instance, the error from sending to a closed 
                // port on localhost) should be ignored:
                if (IsTemporaryError(ex.SocketErrorCode))
                {
                    ContinueReceive();

                    return;
                }

                HandleSocketException(ex);

                return;
            }
            catch (ObjectDisposedException ex)
            {
                HandleSocketClosed(ex);

                return;
            }

            try
            {
                _protocol.DataReceived(_receiveEndpoint as IPEndPoint, _receiveBuffer, 0, bytesReceived);
                _receiveEndpoint = null;
            }
            catch (Exception ex)
            {
                HandleProtocolException(ex);
            }

            ContinueReceive();
        }

        protected override void ContinueSend()
        {
            IAsyncResult result;

            try
            {
                if (_transmitBuffer.EndPointOrNull == null)
                {
                    result = _socket.BeginSend(_transmitBuffer.Data.Array, _transmitBuffer.Data.Offset,
                        _transmitBuffer.Data.Count, SocketFlags.None, null, null);
                }
                else
                {
                    result = _socket.BeginSendTo(_transmitBuffer.Data.Array, _transmitBuffer.Data.Offset,
                        _transmitBuffer.Data.Count, SocketFlags.None, _transmitBuffer.EndPointOrNull, null, null);
                }
            }
            catch (SocketException ex)
            {
                HandleSocketException(ex);

                return;
            }
            catch (ObjectDisposedException ex)
            {
                HandleSocketClosed(ex);

                return;
            }

            _reactor.AddResult(result, SendCompleted);
        }

        void SendCompleted(IAsyncResult result, object state)
        {
            int bytesSent;

            try
            {
                if (_transmitBuffer.EndPointOrNull == null)
                {
                    bytesSent = _socket.EndSend(result);
                }
                else
                {
                    bytesSent = _socket.EndSendTo(result);
                }
            }
            catch (SocketException ex)
            {
                if (IsTemporaryError(ex.SocketErrorCode))
                {
                    _isTransmitting = false;

                    KickSendQueue();

                    return;
                }

                HandleSocketException(ex);

                return;
            }
            catch (ObjectDisposedException ex)
            {
                HandleSocketClosed(ex);

                return;
            }

            if (bytesSent != _transmitBuffer.Data.Count)
            {
                _truncationOccurred = true;

                return;
            }

            _isTransmitting = false;

            KickSendQueue();
        }
    }
}
