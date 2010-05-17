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
using System.Net;
using System.Text;

using Interlace.ReactorCore;
using Interlace.ReactorService;
using Interlace.ReactorUtilities;
using Interlace.Utilities;

#endregion

namespace Interlace.Tests.Reactor
{
    public class UnserializableClass
    {
    }

    public class UnserializableException : Exception
    {
    }

    public interface IRemotingTest
    {
        Deferred<int> AddTwoIntegers(int first, int second);
        Deferred<int> DivideTwoIntegers(int numerator, int denominator);
        Deferred<UnserializableClass> GetUnserializableClass();
        VoidDeferred ThrowUnserializableException();

        [ProxiedReturn]
        Deferred<IRemoteAdder> CreateAdder(int number);
    }

    public interface IRemoteAdder
    {
        Deferred<int> AddTo(int number);
    }

    public class RemoteAdder : IRemoteAdder
    {
        int _first;

        public RemoteAdder(int first)
        {
            _first = first;
        }

        #region IRemoteAdder Members

        public Deferred<int> AddTo(int number)
        {
            return Deferred.Success(_first + number);
        }

        #endregion
    }

    public class RemotingImplementation : IRemotingTest
    {
        public Deferred<int> AddTwoIntegers(int first, int second)
        {
            return Deferred.Success(first + second);
        }

        public Deferred<int> DivideTwoIntegers(int numerator, int denominator)
        {
            return Deferred.Success(numerator / denominator);
        }

        public Deferred<UnserializableClass> GetUnserializableClass()
        {
            return Deferred.Success(new UnserializableClass());
        }

        public VoidDeferred ThrowUnserializableException()
        {
            throw new UnserializableException();
        }

        public Deferred<IRemoteAdder> CreateAdder(int number)
        {
            return Deferred.Success((new RemoteAdder(number)) as IRemoteAdder);
        }
    }

    public class RemotingTestServerProtocol : RemotingProtocol
    {
        public RemotingTestServerProtocol()
        {
            RegisterLocalRootObject<IRemotingTest>(new RemotingImplementation());
        }
    }

    public interface IRemotingTestServices
    {
        [ProxiedReturn]
        Deferred<IRemotingTest> Connect();
    }

    public class RemotingTestService : IService, IRemotingTestServices
    {
        RemotingTestServerProtocol _server;
        IServiceHost _host;

        InvokeReactorQueue _queue = new InvokeReactorQueue();

        public void Open(IServiceHost host)
        {
            _host = host;

            _server = new RemotingTestServerProtocol();

            host.Reactor.ListenStream(new NoArgumentProtocolFactory<RemotingTestServerProtocol>(), 1337);

            _queue.Open(host);
        }

        Deferred<IRemotingTest> IRemotingTestServices.Connect()
        {
            RemotingClientProtocolAndFactory<IRemotingTest> client = new RemotingClientProtocolAndFactory<IRemotingTest>();

            _host.Reactor.ConnectStream(client, new IPEndPoint(IPAddress.Loopback, 1337));

            return client.ConnectedDeferred;
        }

        public IRemotingTestServices GetServices(IThreadInvoker invoker)
        {
            return _queue.CreateProxy<IRemotingTestServices>(invoker, this);
        }

        public void Close(IServiceHost host)
        {
            _queue.Close(host);

            _host = null;
        }
    }
}
