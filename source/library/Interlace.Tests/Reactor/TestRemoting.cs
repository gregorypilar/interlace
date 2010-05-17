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

using MbUnit.Framework;

using Interlace.ReactorService;
using Interlace.ReactorUtilities;

#endregion

namespace Interlace.Tests.Reactor
{
    [TestFixture]
    public class TestRemoting
    {
        ServiceHost _host;
        RemotingTestService _service;

        BlockingThreadInvoker _invoker;
        IRemotingTest _connection;

        [SetUp]
        public void SetUp()
        {
            _host = new ServiceHost();

            _service = new RemotingTestService();

            _host.AddService(_service);

            _host.StartServiceHost();
            _host.OpenServices();

            _invoker = new BlockingThreadInvoker();

            IRemotingTestServices services = _service.GetServices(_invoker);

            _connection = _invoker.WaitOn(services.Connect());
        }

        [TearDown]
        public void TearDown()
        {
            if (_invoker != null) _invoker.Dispose();

            _host.CloseServices();
            _host.StopServiceHost();
        }

        [Test]
        public void TestRootObjectCall()
        {
            int firstResult = _invoker.WaitOn(_connection.AddTwoIntegers(4, 9));

            Assert.AreEqual(4 + 9, firstResult);
        }

        [Test]
        public void TestMarshalledObjectCall()
        {
            IRemoteAdder adder = _invoker.WaitOn(_connection.CreateAdder(42));

            int secondResult = _invoker.WaitOn(adder.AddTo(68));

            Assert.AreEqual(42 + 68, secondResult);
        }

        [Test]
        public void TestException()
        {
            try
            {
                _invoker.WaitOn(_connection.DivideTwoIntegers(1, 0));

                Assert.Fail();
            }
            catch (ApplicationException e)
            {
                Assert.IsInstanceOfType(typeof(RemotingException), e.InnerException);

                RemotingException remotingException = e.InnerException as RemotingException;

                Assert.IsInstanceOfType(typeof(DivideByZeroException), remotingException.InnerException);
            }
        }

        [Test]
        [Ignore]
        public void TestUnserializableException()
        {
            try
            {
                _invoker.WaitOn(_connection.ThrowUnserializableException());

                Assert.Fail();
            }
            catch (ApplicationException e)
            {
                Assert.IsInstanceOfType(typeof(RemotingException), e.InnerException);

                RemotingException remotingException = e.InnerException as RemotingException;

                Assert.IsNull(remotingException.InnerException);

                Assert.Contains(remotingException.Message, "UnserializableException");
            }
        }

        [Test]
        [Ignore]
        public void TestUnserializableObject()
        {
            try
            {
                _invoker.WaitOn(_connection.GetUnserializableClass());

                Assert.Fail();
            }
            catch (ApplicationException e)
            {
                Assert.IsInstanceOfType(typeof(RemotingException), e.InnerException);

                RemotingException remotingException = e.InnerException as RemotingException;

                Assert.IsInstanceOfType(typeof(UnserializableException), remotingException.InnerException);
            }
        }

        // TODO: Get the unserializable tests working; currently unserializable objects aren't handled.
    }
}
