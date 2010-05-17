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
using System.Reflection;
using System.Text;

using Interlace.ReactorCore;
using Interlace.ReactorUtilities;
using Interlace.Utilities;

#endregion

namespace Interlace.ReactorService
{
    public class InvokeReactorQueue 
    {
        ReactorQueue<InvokeReactorQueueRequest> _queue;

        public InvokeReactorQueue()
        {
            _queue = new ReactorQueue<InvokeReactorQueueRequest>(256);
        }

        public TInterface CreateProxy<TInterface>(IThreadInvoker invoker, TInterface implementation) where TInterface : class
        {
            return Proxies.MakeProxy<TInterface>(new InvokeReactorQueueProxyHandler(this, implementation, invoker));
        }

        public object CreateProxy(Type interfaceType, IThreadInvoker invoker, object implementation) 
        {
            return Proxies.MakeProxy(interfaceType, new InvokeReactorQueueProxyHandler(this, implementation, invoker));
        }

        public void Open(IServiceHost host)
        {
            _queue.StartDequeuingToCallback(host.Reactor, InvokeDequeued);
        }

        public void Close(IServiceHost host)
        {
            _queue.StopDequeuingToCallback();
        }

        void InvokeDequeued(ReactorQueue<InvokeReactorQueueRequest> queue, InvokeReactorQueueRequest item)
        {
            DeferredObject result;

            try
            {
                result = item.Method.Invoke(item.Implementation, item.Arguments) as DeferredObject;
            }
            catch (Exception e)
            {
                item.DeferredObject.Fail(DeferredFailure.FromException(e));

                return;
            }

            // Otherwise, if no failure occurred:
            result.ObjectCompletion(item.DeferredObject);
        }

        internal void AcceptRequest(MethodInfo method, object[] arguments, DeferredObject returnedDeferred, object implementation)
        {
            InvokeReactorQueueRequest request = new InvokeReactorQueueRequest(method, arguments, returnedDeferred, implementation);
            _queue.Enqueue(request);
        }
    }
}
