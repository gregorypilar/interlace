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

using Interlace.ReactorCore;

#endregion

namespace Interlace.Threads
{
    public abstract class WorkerThread
    {
        WorkerThreadPool _pool = null;
        ThreadedSlot _slot = new ThreadedSlot();
        Thread _thread;
        string _name = "Unnamed Worker Thread";

        public WorkerThread()
        {
        }

        public WorkerThread(string name)
        {
            _name = name;
        }

        internal void Attach(WorkerThreadPool pool)
        {
            _pool = pool;
        }

        internal void CreateThread()
        {
            _thread = new Thread(ThreadMethod);
            _thread.Name = _name;
            _thread.Start();
        }

        internal void Shutdown()
        {
            AcceptRequest(WorkerThreadShutdownRequest.Value);

            ShutdownHasBeenRequested();

            _thread.Join();
        }

        internal void ThreadMethod()
        {
            SetUp();

            while (true)
            {
                object request = _slot.GetFromSlot();

                if (object.ReferenceEquals(request, WorkerThreadShutdownRequest.Value)) return;

                try
                {
                    Run(request);
                }
                catch (Exception ex)
                {
                    HandleExceptionInRun(ex);
                }

                _pool.RequestCompleted(this);
            }

            TearDown();
        }

        public event EventHandler<ServiceExceptionEventArgs> RunException;

        private void HandleExceptionInRun(Exception ex)
        {
            if (RunException != null) RunException(this, new ServiceExceptionEventArgs(ServiceExceptionKind.DuringHandler, ex));
        }

        internal void AcceptRequest(object request)
        {
            _slot.PutInToSlot(request);
        }

        public virtual void SetUp()
        {
        }

        public virtual void Run(object request)
        {
        }

        public virtual void ShutdownHasBeenRequested()
        {
        }

        public virtual void TearDown()
        {
        }
    }
}
