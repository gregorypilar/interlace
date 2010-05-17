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

using Interlace.ReactorCore;
using Interlace.ReactorService;

#endregion

namespace Interlace.Tests.Reactor
{
    [TestFixture]
    public class TestQueue
    {
        QueueTestService _testService;
        ServiceHost _host;

        bool IsCompleteListPredicate(List<int> dequeuedItems)
        {
            if (dequeuedItems.Count != 10) return false;

            for (int i = 0; i < 10; i++)
            {
                if (dequeuedItems[i] != i) return false;
            }

            return true;
        }

        [SetUp]
        public void SetUp()
        {
            _testService = new QueueTestService(4, IsCompleteListPredicate);

            _host = new ServiceHost();
            _host.AddService(_testService);

            _host.StartServiceHost();
            _host.OpenServices();
        }

        [TearDown]
        public void TearDown()
        {
            _host.CloseServices();
            _host.StopServiceHost();
        }

        [Test]
        public void TestDequeueInReactor()
        {
            for (int i = 0; i < 10; i++)
            {
                _testService.Queue.Enqueue(i);
            }

            bool wasFinished = _testService.FinishedEvent.WaitOne(1000, false);

            Assert.IsTrue(wasFinished);
        }

        [Test]
        public void TestDequeue()
        {
            using (ReactorQueue<int> queue = new ReactorQueue<int>(4))
            {
                queue.Enqueue(1);
                queue.Enqueue(2);

                Assert.AreEqual(1, queue.Dequeue());
                Assert.AreEqual(2, queue.Dequeue());
            }
        }
    }
}
