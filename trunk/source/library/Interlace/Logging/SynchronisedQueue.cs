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

using System.Collections;
using System.Threading;

#endregion

namespace Interlace.Logging
{
    /// <summary>
    /// A thread safe queue (for multiple producers and comsumers) that blocks consumers
    /// on an empty queue and blocks producers on a full queue.
    /// </summary>
    /// <remarks>
    /// This class is an implementation of the "Producer-Consumer with Binary Semaphores" from:
    /// 
    ///     M. Ben-Ari. Principles of Concurrent and Distributed Programming. 1990.
    /// 
    /// Windows NT auto-reset events are used rather than binary semaphores (they are equivalent)
    /// because .NET did not in early versions include semaphores or binary semaphores.
    /// </remarks>
    public class SynchronisedQueue
    {
        object[] _circularBuffer;

        AutoResetEvent _notEmpty = new AutoResetEvent(false);
        AutoResetEvent _notFull = new AutoResetEvent(false);

        int _inPointer = 0;
        int _outPointer = 0;

        int _count = 0;
        object _countLock = new object();

        int _enqueueCount = 0;
        int _dequeueCount = 0;

        ManualResetEvent _shutdownQueueEvent = new ManualResetEvent(false);

        object _enqueueLock = new object();
        object _dequeueLock = new object();

        public SynchronisedQueue(int capacity)
        {
            _circularBuffer = new object[capacity];
        }

        public int Capacity
        {
            get { return _circularBuffer.Length; }
        }

        public void Shutdown()
        {
            _shutdownQueueEvent.Set();
        }

        public void Enqueue(object value, int timeout)
        {
            lock (_enqueueLock)
            {
                if (_enqueueCount == _circularBuffer.Length)
                {
                    int waitResult = WaitHandle.WaitAny(new WaitHandle[] { _notFull, _shutdownQueueEvent }, timeout, false);

                    if (waitResult == WaitHandle.WaitTimeout)
                    {
                        throw new SynchronisedQueueTimeoutException();
                    }

                    if (waitResult == 1)
                    {
                        throw new SynchronisedQueueShutdownException();
                    }
                }

                _circularBuffer[_inPointer] = value;

                lock (_countLock)
                {
                    _count += 1;
                    _enqueueCount = _count;
                }

                if (_enqueueCount == 1) _notEmpty.Set();

                _inPointer = (_inPointer + 1) % _circularBuffer.Length;
            }
        }

        public object Dequeue(int timeout)
        {
            lock (_dequeueLock)
            {
                if (_dequeueCount == 0)
                {
                    int waitResult = WaitHandle.WaitAny(new WaitHandle[] { _notEmpty, _shutdownQueueEvent }, timeout, false);

                    if (waitResult == WaitHandle.WaitTimeout)
                    {
                        throw new SynchronisedQueueTimeoutException();
                    }

                    if (waitResult == 1)
                    {
                        throw new SynchronisedQueueShutdownException();
                    }
                }

                object value = _circularBuffer[_outPointer];

                lock (_countLock)
                {
                    _count -= 1;
                    _dequeueCount = _count;
                }

                if (_dequeueCount == _circularBuffer.Length - 1) _notFull.Set();

                _outPointer = (_outPointer + 1) % _circularBuffer.Length;

                return value;
            }
        }
    }
}
