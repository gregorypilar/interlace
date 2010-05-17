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

using Interlace.Logging;

#endregion

namespace Interlace.Threads
{
    public delegate WorkerThread CreateWorkerThreadDelegate();

    public class WorkerThreadPool
    {
        private SynchronisedQueue _readyQueue;
        private CreateWorkerThreadDelegate _workerConstructor;

        private List<WorkerThread> _workerThreads = new List<WorkerThread>();

        private bool _running;

        public WorkerThreadPool(CreateWorkerThreadDelegate workerConstructor, int poolSize)
        {
            _workerConstructor = workerConstructor;

            _readyQueue = new SynchronisedQueue(poolSize);

            for (int i = 0; i <= poolSize - 1; i++)
            {
                CreateWorker();
            }

            _running = true;
        }

        public void QueueRequest(object request)
        {
            if (!_running)
            {
                throw new InvalidOperationException("A request can not be queued once the " + 
                    "thread pool has been shut down.");
            }

            WorkerThread thread = (WorkerThread)_readyQueue.Dequeue(Timeout.Infinite);

            thread.AcceptRequest(request);
        }

        /// <summary>
        /// Shuts down the pool of workers. A pool instance becomes unusable once it is shut down.
        /// </summary>
        public void Shutdown()
        {
            if (_running)
            {
                _running = false;

                foreach (WorkerThread thread in _workerThreads)
                {
                    thread.Shutdown();
                }
            }
        }

        private void CreateWorker()
        {
            WorkerThread worker = _workerConstructor();

            _readyQueue.Enqueue(worker, Timeout.Infinite);
            _workerThreads.Add(worker);

            worker.Attach(this);
            worker.CreateThread();
        }

        internal void RequestCompleted(WorkerThread worker)
        {
            _readyQueue.Enqueue(worker, Timeout.Infinite);
        }
    }
}
