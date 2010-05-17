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

using MbUnit.Framework;

using Interlace.ReactorService;

#endregion

namespace Interlace.Tests.Reactor
{
    [TestFixture]
    public class TestInvokeQueue
    {

        [Test]
        public void TestQueue()
        {
            ServiceHost host = new ServiceHost();

            InvokeQueueTestService testService = new InvokeQueueTestService();

            host.AddService(testService);

            host.StartServiceHost();
            host.OpenServices();

            using (ManualResetEvent callbackEvent = new ManualResetEvent(false))
            {
                object state = new object();

                IAsyncResult resultFromCallback;
                object stateFromCallback;
                bool callbackWorked = false;

                IInvokeQueueTestInterface testInterface = testService.GetTestInterface(null);

                /*
                Hmm, blocking from deferreds is a bit tricky.
                 
                Deferred<int> result = testInterface.GetReactorThreadId(false);

                int reactorThreadId = testService.EndGetReactorThreadId(result);

                Assert.AreNotEqual(Thread.CurrentThread.ManagedThreadId, reactorThreadId);
                Assert.AreNotEqual(0, reactorThreadId);
                */

                bool callbackCompleted = callbackEvent.WaitOne(2000, false);

                Assert.IsTrue(callbackCompleted);
                Assert.IsTrue(callbackWorked);
            }

            host.StopServiceHost();
        }
    }
}
