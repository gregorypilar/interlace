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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

#endregion

namespace Interlace.ReactorCore
{
    public class StreamConnection : Connection
    {
        Stream _readStream = null;
        Stream _writeStream = null;

        internal StreamConnection(IReactor reactor, Protocol protocol)
        : base(reactor, protocol)
        {
        }

        internal void StartConnection(Stream readStream, Stream writeStream, int receiveBufferSize)
        {
            if (_isConnected) throw new InvalidOperationException(
                "An attempt was made to connect an already connected connection.");

            _readStream = readStream;
            _writeStream = writeStream;

            StartConnection(receiveBufferSize);
        }

        protected override void CloseSocketWhileIgnoringErrors()
        {
            if (!_isConnected) return;

            try
            {
                _readStream.Close();
            }
            catch (ObjectDisposedException)
            {
                // Ignore.
            }

            try
            {
                _writeStream.Close();
            }
            catch (ObjectDisposedException)
            {
                // Ignore.
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get { return null; }
        }

        protected override void ContinueReceive()
        {
            IAsyncResult result;

            try
            {
                result = _readStream.BeginRead(_receiveBuffer, 0,
                    _receiveBuffer.Length, null, null);
            }
            catch (IOException ex)
            {
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
                bytesReceived = _readStream.EndRead(result);
            }
            catch (IOException ex)
            {
                HandleSocketException(ex);

                return;
            }
            catch (ObjectDisposedException ex)
            {
                HandleSocketClosed(ex);

                return;
            }

            if (bytesReceived == 0)
            {
                HandleSocketReadFailed();

                return;
            }

            try
            {
                _protocol.DataReceived(null, _receiveBuffer, 0, bytesReceived);
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
                result = _writeStream.BeginWrite(_transmitBuffer.Data.Array, _transmitBuffer.Data.Offset,
                    _transmitBuffer.Data.Count, null, _transmitBuffer.Data.Count);
            }
            catch (IOException ex)
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
            int bytesSent = (int)result.AsyncState;

            try
            {
                _writeStream.EndWrite(result);

                _writeStream.Flush();
            }
            catch (IOException ex)
            {
                HandleSocketException(ex);

                return;
            }
            catch (ObjectDisposedException ex)
            {
                HandleSocketClosed(ex);

                return;
            }

            if (bytesSent == 0)
            {
                HandleSocketReadFailed();

                return;
            }

            _transmitBuffer = _transmitBuffer.WithConsumedBytes(bytesSent);

            if (_transmitBuffer.Data.Count == 0)
            {
                _isTransmitting = false;

                KickSendQueue();
            }
            else
            {
                ContinueSend();
            }
        }
    }
}
