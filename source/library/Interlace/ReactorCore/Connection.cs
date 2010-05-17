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
    public abstract class Connection : IConnection
    {
        protected IReactor _reactor;
        protected Protocol _protocol;

        protected bool _isConnected = false;

        protected int _receiveBufferSize;
        protected byte[] _receiveBuffer = null;

        protected bool _isTransmitting = false;
        protected SocketSendBuffer _transmitBuffer;

        protected Queue<SocketSendBuffer> _transmitQueue = new Queue<SocketSendBuffer>();

        protected Connection(IReactor reactor, Protocol protocol)
        {
            _reactor = reactor;
            _protocol = protocol;
        }

        public void Send(IPEndPoint sendTo, byte[] data, int offset, int length, bool copyData)
        {
            if (!_isConnected) throw new InvalidOperationException(
                "An attempt was made to send data through a closed socket.");

            _transmitQueue.Enqueue(new SocketSendBuffer(sendTo, data, offset, length, copyData));

            KickSendQueue();
        }

        public void Send(byte[] data, int offset, int length, bool copyData)
        {
            Send(null, data, offset, length, copyData);
        }

        void StartReceive()
        {
            _receiveBuffer = new byte[_receiveBufferSize];

            ContinueReceive();
        }

        protected abstract void ContinueReceive();

        protected void KickSendQueue()
        {
            if (!_isTransmitting && _transmitQueue.Count > 0)
            {
                _isTransmitting = true;
                _transmitBuffer = _transmitQueue.Dequeue();

                ContinueSend();
            }
        }

        internal void StartConnection(int receiveBufferSize)
        {
            if (_isConnected) throw new InvalidOperationException();

            _receiveBufferSize = receiveBufferSize;
            _isConnected = true;

            StartReceive();
        }

        protected abstract void ContinueSend();

        protected abstract void CloseSocketWhileIgnoringErrors();

        public void LoseConnection()
        {
            if (!_isConnected) return;

            _protocol.ConnectionLost(CloseReason.LocalClose);

            CloseSocketWhileIgnoringErrors();
            _isConnected = false;
        }

        protected void HandleSocketClosed(ObjectDisposedException ex)
        {
            if (!_isConnected) return;

            _isConnected = false;

            _protocol.ConnectionLost(CloseReason.RemoteClose);
        }

        protected void HandleProtocolException(Exception ex)
        {
            if (!_isConnected) return;

            CloseSocketWhileIgnoringErrors();
            _isConnected = false;

            _protocol.ConnectionLost(CloseReason.ProtocolException);
        }

        protected virtual void HandleSocketException(Exception ex)
        {
            if (!_isConnected) return;

            CloseSocketWhileIgnoringErrors();
            _isConnected = false;

            _protocol.ConnectionLost(CloseReason.SocketError);
        }

        protected virtual void HandleSocketReadFailed()
        {
            if (!_isConnected) return;

            CloseSocketWhileIgnoringErrors();
            _isConnected = false;

            _protocol.ConnectionLost(CloseReason.SocketError);
        }
    }
}
