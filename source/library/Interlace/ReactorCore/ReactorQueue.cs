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
using System.Text;
using System.Threading;

#endregion

namespace Interlace.ReactorCore
{
    /// <summary>
    /// References the callback method to be called when an item has been automatically dequeued.
    /// </summary>
    /// <typeparam name="T">The type of item contained in the queue.</typeparam>
    /// <param name="queue">The queue that produced the item.</param>
    /// <param name="item">The item that was enqueued on the queue.</param>
    public delegate void ReactorQueueDequeueCallback<T>(ReactorQueue<T> queue, T item);

    /// <summary>
    /// A queue for sending a single consumer thread items from a single producer thread,
    /// with additional support for reactors.
    /// </summary>
    /// <typeparam name="T">The type of item the queue stores.</typeparam>
    public class ReactorQueue<T> : IDisposable
    {
        T[] _elements;
        int _nextEnqueueElement;
        int _nextDequeueElement;

        Semaphore _elementsSemaphore;
        Semaphore _spacesSemaphore;

        IReactor _reactor;
        ReactorQueueDequeueCallback<T> _callback;

        object _enqueueLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReactorQueue&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="capacity">The capacity of the queue.</param>
        public ReactorQueue(int capacity)
        {
            _elements = new T[capacity];
            _nextDequeueElement = 0;
            _nextDequeueElement = 0;

            _elementsSemaphore = new Semaphore(0, capacity);
            _spacesSemaphore = new Semaphore(capacity, capacity);

            _reactor = null;
            _callback = null;
        }

        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <remarks>The calling thread will block until space is available in the queue.
        /// This method must only be called by a single producing thread; it is not 
        /// itself thread-safe.</remarks>
        /// <param name="item">The item.</param>
        public void Enqueue(T item)
        {
            lock (_enqueueLock)
            {
                _spacesSemaphore.WaitOne();
                _elements[_nextEnqueueElement] = item;
                _nextEnqueueElement = (_nextEnqueueElement + 1) % _elements.Length;
                _elementsSemaphore.Release();
            }
        }

        /// <summary>
        /// Dequeues a single item from the queue.
        /// </summary>
        /// <remarks>The calling thread will block until an item is available to dequeue. This
        /// method must only be called by a single consuming thread; it is not itself 
        /// threadsafe.</remarks>
        /// <returns>The item.</returns>
        public T Dequeue()
        {
            _elementsSemaphore.WaitOne();
            T item = _elements[_nextDequeueElement];
            _nextDequeueElement = (_nextDequeueElement + 1) % _elements.Length;
            _spacesSemaphore.Release();

            return item;
        }

        /// <summary>
        /// Starts automatically dequeuing any items enqueued and calls a callback
        /// delegate for each item.
        /// </summary>
        /// <remarks>This method must only be called in the reactor thread.</remarks>
        /// <param name="reactor">The reactor. Callbacks will occur in the reactor thread.</param>
        /// <param name="callback">The method to call for each item.</param>
        public void StartDequeuingToCallback(IReactor reactor, ReactorQueueDequeueCallback<T> callback)
        {
            _reactor = reactor;
            _callback = callback;

            _reactor.AddHandle(_elementsSemaphore, DequeueCompleted);
        }

        void DequeueCompleted(IAsyncResult result, object state)
        {
            T item = _elements[_nextDequeueElement];
            _nextDequeueElement = (_nextDequeueElement + 1) % _elements.Length;
            _spacesSemaphore.Release();

            try
            {
                _callback(this, item);
            }
            finally
            {
                _reactor.AddHandle(_elementsSemaphore, DequeueCompleted);
            }
        }

        /// <summary>
        /// Stops automatic dequeuing.
        /// </summary>
        public void StopDequeuingToCallback()
        {
            _reactor.EnsureHandleRemoved(_elementsSemaphore);

            _reactor = null;
            _callback = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_reactor != null) StopDequeuingToCallback();

            if (_elementsSemaphore != null) _elementsSemaphore.Close();
            if (_spacesSemaphore != null) _spacesSemaphore.Close();

            _elementsSemaphore = null;
            _spacesSemaphore = null;
        }

        #endregion
    }
}
