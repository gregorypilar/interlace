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
using System.Text;

using Interlace.ReactorCore;
using Interlace.ReactorService;

#endregion

namespace Interlace.Sharpcap
{
    public class RemoteCaptureService : IService
    {
        IReactor _reactor;

        ConnectorHandle _controlConnector;

        List<ICaptureListener> _listeners = new List<ICaptureListener>();

        int _listenPort;

        public RemoteCaptureService(int listenPort)
        {
            _listenPort = listenPort;
        }

        internal IReactor Reactor
        {
            get { return _reactor; }
        }

        internal void AddListener(ICaptureListener listener)
        {
            _listeners.Add(listener);
        }

        internal void RemoveListener(ICaptureListener listener)
        {
            for (int i = 0; i < _listeners.Count; i++)
            {
                if (object.ReferenceEquals(_listeners[i], listener))
                {
                    _listeners.RemoveAt(i);

                    return;
                }
            }
        }

        public void BroadcastCapture(byte[] buffer, int offset, int length)
        {
            foreach (ICaptureListener listener in _listeners)
            {
                listener.HandleCapture(buffer, offset, length);
            }
        }

        #region IService Members

        public void Open(IServiceHost host)
        {
            _reactor = host.Reactor;

            _controlConnector = host.Reactor.ListenStream(new RemoteCaptureProtocolFactory(this), _listenPort);
        }

        public void Close(IServiceHost host)
        {
            _controlConnector.Close();

            _reactor = null;
        }

        #endregion
    }
}
