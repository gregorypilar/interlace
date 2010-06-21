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
using System.Threading;

#endregion

namespace Interlace.ReactorCore
{
    public delegate void ResultCallback(IAsyncResult result, object state);

    public delegate void TimerCallback(DateTime fireAt, object state);

    public interface IReactor
    {
        void ConnectStream(IProtocolFactory factory, string addressString, int port);
        void ConnectStream(IProtocolFactory factory, IPAddress address, int port);
        void ConnectStream(IProtocolFactory factory, IPEndPoint peer);

        void BindDatagram(IProtocolFactory factory);
        void BindDatagram(IProtocolFactory factory, int localPort);
        void BindDatagram(IProtocolFactory factory, IPAddress localAddress, int localPort);
        void BindToPeerDatagram(IProtocolFactory factory, string addressString, int port);
        void BindToPeerDatagram(IProtocolFactory factory, IPAddress address, int port);
        void BindToPeerDatagram(IProtocolFactory factory, IPEndPoint peer);

        ConnectorHandle ListenStream(IProtocolFactory factory, int port, IPAddress address);
        ConnectorHandle ListenStream(IProtocolFactory factory, int port);
        ConnectorHandle ListenStream(IProtocolFactory factory);

        void AttachStream(IProtocolFactory factory, Stream stream);
        void AttachStream(IProtocolFactory factory, Stream readStream, Stream writeStream);

        TimerHandle AddTimer(DateTime fireAt, TimerCallback callback, object state);
        TimerHandle AddTimer(TimeSpan fireAfter, TimerCallback callback, object state);
        RepeatingTimerHandle AddRepeatingTimer(TimeSpan interval, TimerCallback callback, object state);

        void AddResult(IAsyncResult result, ResultCallback callback, int integerState, object state);
        void AddResult(IAsyncResult result, ResultCallback callback, int integerState);
        void AddResult(IAsyncResult result, ResultCallback callback);

        void AddHandle(WaitHandle handle, ResultCallback callback, int integerState, object state);
        void AddHandle(WaitHandle handle, ResultCallback callback, int integerState);
        void AddHandle(WaitHandle handle, ResultCallback callback);

        void AddPermanentHandle(WaitHandle handle, ResultCallback callback, int integerState, object state);
        void AddPermanentHandle(WaitHandle handle, ResultCallback callback, int integerState);
        void AddPermanentHandle(WaitHandle handle, ResultCallback callback);

        void EnsureHandleRemoved(WaitHandle handle);
    }
}
