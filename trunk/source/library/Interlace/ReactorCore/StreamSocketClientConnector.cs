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
    class StreamSocketClientConnector
    {
        Socket _socket = null;
        Reactor _reactor;
        IProtocolFactory _factory = null;

        public StreamSocketClientConnector(Reactor reactor)
        {
            _reactor = reactor;
        }

        public void Connect(IProtocolFactory factory, string addressString, int port)
        {
            IPAddress address;

            if (IPAddress.TryParse(addressString, out address))
            {
                Connect(factory, address, port);
            }
            else
            {
                IAsyncResult result = Dns.BeginGetHostEntry(addressString, null, 
                    new Pair<string, int>(addressString, port));

                _reactor.AddResult(result, ResolveCompleted, 0, factory);
            }
        }

        void ResolveCompleted(IAsyncResult result, object state)
        {
            IProtocolFactory factory = state as IProtocolFactory;
            Pair<string, int> pair = result.AsyncState as Pair<string, int>;

            IPHostEntry entry;

            entry = Dns.EndGetHostEntry(result);

            if (entry.AddressList.Length == 0)
            {
                _factory.ConnectionFailed(new ApplicationException(string.Format(
                    "Address resolution for the address \"{0}\" failed.",
                    pair.First)));
            }
            else
            {
                Connect(factory, entry.AddressList[0], pair.Second);
            }
        }

        public void Connect(IProtocolFactory factory, IPAddress address, int port)
        {
            Connect(factory, new IPEndPoint(address, port));
        }

        public void Connect(IProtocolFactory factory, IPEndPoint peer)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _factory = factory;

            _factory.StartedConnecting();

            IAsyncResult result = _socket.BeginConnect(peer, null, null);

            _reactor.AddResult(result, ConnectCompleted);
        }

        void ConnectCompleted(IAsyncResult result, object state)
        {
            try
            {
                _socket.EndConnect(result);
            }
            catch (SocketException ex)
            {
                _factory.ConnectionFailed(ex);

                return;
            }
            catch (ObjectDisposedException ex)
            {
                _factory.ConnectionFailed(ex);

                return;
            }

            Protocol protocol = _factory.BuildProtocol();

            SocketConnection connection = new StreamSocketConnection(_reactor, protocol);
            connection.StartConnection(_socket, protocol.ReceiveBufferSize);
            protocol.MakeConnection(connection);
        }
    }
}
