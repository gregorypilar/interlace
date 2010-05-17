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
    public class Reactor : IReactor
    {
        TimerQueue _queue = new TimerQueue();
        List<ReactorSlot> _slots = new List<ReactorSlot>();

        public event EventHandler<ServiceExceptionEventArgs> ReactorException;

        public void ConnectStream(IProtocolFactory factory, string addressString, int port)
        {
            StreamSocketClientConnector connector = new StreamSocketClientConnector(this);
            connector.Connect(factory, addressString, port);
        }

        public void ConnectStream(IProtocolFactory factory, IPAddress address, int port)
        {
            StreamSocketClientConnector connector = new StreamSocketClientConnector(this);
            connector.Connect(factory, address, port);
        }

        public void ConnectStream(IProtocolFactory factory, IPEndPoint peer)
        {
            StreamSocketClientConnector connector = new StreamSocketClientConnector(this);
            connector.Connect(factory, peer);
        }

        public void BindDatagram(IProtocolFactory factory)
        {
            DatagramSocketConnector connector = new DatagramSocketConnector(this);
            connector.Bind(factory);
        }

        public void BindDatagram(IProtocolFactory factory, int localPort)
        {
            DatagramSocketConnector connector = new DatagramSocketConnector(this);
            connector.Bind(factory, localPort);
        }

        public void BindDatagram(IProtocolFactory factory, IPAddress localAddress, int localPort)
        {
            DatagramSocketConnector connector = new DatagramSocketConnector(this);
            connector.Bind(factory, localAddress, localPort);
        }

        public void BindToPeerDatagram(IProtocolFactory factory, string addressString, int port)
        {
            DatagramSocketConnector connector = new DatagramSocketConnector(this);
            connector.BindToPeer(factory, addressString, port);
        }

        public void BindToPeerDatagram(IProtocolFactory factory, IPAddress address, int port)
        {
            DatagramSocketConnector connector = new DatagramSocketConnector(this);
            connector.BindToPeer(factory, address, port);
        }

        public void BindToPeerDatagram(IProtocolFactory factory, IPEndPoint peer)
        {
            DatagramSocketConnector connector = new DatagramSocketConnector(this);
            connector.BindToPeer(factory, peer);
        }

        public ConnectorHandle ListenStream(IProtocolFactory factory, int port, IPAddress address)
        {
            StreamSocketServerConnector connector = new StreamSocketServerConnector(this);
            connector.Listen(factory, port, address);

            return new ConnectorHandle(connector);
        }

        public ConnectorHandle ListenStream(IProtocolFactory factory, int port)
        {
            StreamSocketServerConnector connector = new StreamSocketServerConnector(this);
            connector.Listen(factory, port);

            return new ConnectorHandle(connector);
        }

        public void AttachStream(IProtocolFactory factory, Stream stream)
        {
            AttachStream(factory, stream, stream);
        }

        public void AttachStream(IProtocolFactory factory, Stream readStream, Stream writeStream)
        {
            factory.StartedConnecting();

            Protocol protocol = factory.BuildProtocol();

            StreamConnection connection = new StreamConnection(this, protocol);

            connection.StartConnection(readStream, writeStream, protocol.ReceiveBufferSize);
            protocol.MakeConnection(connection);
        }

        public void AddResult(IAsyncResult result, ResultCallback callback, int integerState, object state)
        {
            ReactorSlot slot = new ReactorSlot();
            slot.Result = result;
            slot.Callback = callback;
            slot.State = state;
            slot.IsPermanent = false;

            slot.Handle = slot.Result.AsyncWaitHandle;

            _slots.Add(slot);
        }

        public void AddResult(IAsyncResult result, ResultCallback callback, int integerState)
        {
            AddResult(result, callback, integerState, null);
        }

        public void AddResult(IAsyncResult result, ResultCallback callback)
        {
            AddResult(result, callback, 0, null);
        }

        public void AddHandle(WaitHandle handle, ResultCallback callback, int integerState, object state)
        {
            ReactorSlot slot = new ReactorSlot();
            slot.Result = null;
            slot.Callback = callback;
            slot.State = state;
            slot.IsPermanent = false;

            slot.Handle = handle;

            _slots.Add(slot);
        }

        public void AddHandle(WaitHandle handle, ResultCallback callback, int integerState)
        {
            AddHandle(handle, callback, integerState, null);
        }

        public void AddHandle(WaitHandle handle, ResultCallback callback)
        {
            AddHandle(handle, callback, 0, null);
        }

        public void AddPermanentHandle(WaitHandle handle, ResultCallback callback, int integerState, object state)
        {
            ReactorSlot slot = new ReactorSlot();
            slot.Result = null;
            slot.Callback = callback;
            slot.State = state;
            slot.IsPermanent = true;

            slot.Handle = handle;

            _slots.Add(slot);
        }

        public void AddPermanentHandle(WaitHandle handle, ResultCallback callback, int integerState)
        {
            AddPermanentHandle(handle, callback, integerState, null);
        }

        public void AddPermanentHandle(WaitHandle handle, ResultCallback callback)
        {
            AddPermanentHandle(handle, callback, 0, null);
        }

        public void EnsureHandleRemoved(WaitHandle handle)
        {
            int i = 0;

            // This loop modifies the slots list in the loop body:
            while (i < _slots.Count)
            {
                // Check the native handle for equality:
                if (_slots[i].Handle.Handle.Equals(handle.Handle))
                {
                    _slots.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        public TimerHandle AddTimer(DateTime fireAt, TimerCallback callback, object state)
        {
            return _queue.Add(fireAt, callback, state);
        }

        public TimerHandle AddTimer(TimeSpan fireAfter, TimerCallback callback, object state)
        {
            return _queue.Add(DateTime.Now + fireAfter, callback, state);
        }

        public RepeatingTimerHandle AddRepeatingTimer(TimeSpan interval, TimerCallback callback, object state)
        {
            RepeatingTimerHandle repeatingTimer = 
                new RepeatingTimerHandle(_queue, interval, callback, state, true);

            return repeatingTimer;
        }

        readonly TimeSpan _forever = new TimeSpan(-1);

        public void RunLoopIteration()
        {
            // Build a list of handles in slot order; assume that the slots won't change index
            // during the run loop:
            WaitHandle[] handles = new WaitHandle[_slots.Count];

            for (int i = 0; i < _slots.Count; i++)
            {
                handles[i] = _slots[i].Handle;
            }

            // Wait on all of the slots until the next timer is ready to be fired:
            TimeSpan timeout = _queue.IsEmpty ? 
                _forever : _queue.GetTimeUntilNextFireable(DateTime.Now);

            int signalledSlotIndex = WaitHandle.WaitAny(handles, timeout, false);

            if (signalledSlotIndex != WaitHandle.WaitTimeout)
            {
                // Fire off the signalled slot:
                ReactorSlot signalledSlot = _slots[signalledSlotIndex];

                if (!signalledSlot.IsPermanent) _slots.RemoveAt(signalledSlotIndex);

                // (The "handles" array no longer matches the slots list.)

                try
                {
                    signalledSlot.Callback(signalledSlot.Result, signalledSlot.State);
                }
                catch (Exception ex)
                {
                    ServiceExceptionEventArgs args = 
                        new ServiceExceptionEventArgs(ServiceExceptionKind.DuringHandler, ex);

                    if (ReactorException != null) ReactorException(this, args);

                    if (!args.Handled) throw;
                }
            }

            // Fire any timers:
            try
            {
                _queue.FireAllFireable(DateTime.Now);
            }
            catch (Exception ex)
            {
                ServiceExceptionEventArgs args = 
                    new ServiceExceptionEventArgs(ServiceExceptionKind.DuringTimer, ex);

                if (ReactorException != null) ReactorException(this, args);

                if (!args.Handled) throw;
            }
        }
    }
}
