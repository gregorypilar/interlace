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

using Interlace.Utilities;

#endregion

namespace Interlace.ReactorCore
{
    class StreamSocketServerConnector
    {
        Socket _socket = null;
        Reactor _reactor;
        IProtocolFactory _factory = null;
        bool _closing = false;

        public StreamSocketServerConnector(Reactor reactor)
        {
            _reactor = reactor;
        }

        public void Listen(IProtocolFactory factory)
        {
            Listen(factory, 0, IPAddress.Any);
        }

        public void Listen(IProtocolFactory factory, int port)
        {
            Listen(factory, port, IPAddress.Any);
        }

        public void Listen(IProtocolFactory factory, int port, IPAddress address)
        {
            if (_socket != null) throw new InvalidOperationException("The connector is already listening.");
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _factory = factory;

            _socket.Bind(new IPEndPoint(address, port));
            _socket.Listen(5);

            StartAccepting();
        }

        public IPAddress ListeningOnAddress
        {
            get
            {
                IPEndPoint ipEndpoint = _socket.LocalEndPoint as IPEndPoint;

                if (ipEndpoint != null)
                {
                    return ipEndpoint.Address;
                }
                else
                {
                    return null;
                }
            }
        }

        public int ListeningOnPort
        {
            get
            {
                IPEndPoint ipEndpoint = _socket.LocalEndPoint as IPEndPoint;

                if (ipEndpoint != null)
                {
                    return ipEndpoint.Port;
                }
                else
                {
                    return 0;
                }
            }
        }

        void StartAccepting()
        {
            if (!_closing)
            {
                IAsyncResult result = _socket.BeginAccept(null, null);

                _reactor.AddResult(result, AcceptCompleted);
            }
        }

        void AcceptCompleted(IAsyncResult result, object state)
        {
            Socket newSocket;
            
            try
            {
                newSocket = _socket.EndAccept(result);
            }
            catch (SocketException)
            {
                if (_closing) return;

                throw;
            }
            catch (ObjectDisposedException)
            {
                if (_closing) return;

                throw;
            }

            Protocol protocol = _factory.BuildProtocol();

            SocketConnection connection = new StreamSocketConnection(_reactor, protocol);
            connection.StartConnection(newSocket, protocol.ReceiveBufferSize);
            protocol.MakeConnection(connection);

            StartAccepting();
        }

        public void Close()
        {
            _closing = true;

            _socket.Close();
        }
    }
}
