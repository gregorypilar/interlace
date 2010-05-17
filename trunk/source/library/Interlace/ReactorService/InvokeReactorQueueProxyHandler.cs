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

using Interlace.ReactorUtilities;
using Interlace.Utilities;

#endregion

namespace Interlace.ReactorService
{
    class InvokeReactorQueueProxyHandler : IProxyHandler
    {
        InvokeReactorQueue _queue;
        object _implementation;
        IThreadInvoker _invoker;

        public InvokeReactorQueueProxyHandler(InvokeReactorQueue queue, object implementation, IThreadInvoker invoker)
        {
            _queue = queue;
            _implementation = implementation;
            _invoker = invoker;
        }

        public object Invoke(object proxyObject, MethodInfo method, object[] arguments)
        {
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                if (parameter.ParameterType.IsByRef)
                {
                    throw new InvalidOperationException("Remoting interfaces can not include methods that " +
                        "have \"out\" or \"ref\" parameters.");
                }
            }

            Type returnType = method.ReturnType;

            if (!Deferred.IsTypedDeferred(returnType)) 
            {
                throw new InvalidOperationException("Remoting interface methods must return a typed deferred.");
            }

            Type deferredPairType = typeof(DeferredThreadingPair<>).MakeGenericType(new Type[] { returnType });
            IDeferredThreadingPair deferredPair = 
                Activator.CreateInstance(deferredPairType, (object)_invoker) as IDeferredThreadingPair;

            _queue.AcceptRequest(method, arguments, deferredPair.ForThreadDeferred, _implementation);

            bool isProxiedReturn = method.GetCustomAttributes(typeof(ProxiedReturnAttribute), true).Length > 0;

            if (isProxiedReturn)
            {
                Type interfaceType = Deferred.DeferredResultType(returnType);

                if (!interfaceType.IsInterface)
                {
                    throw new InvalidOperationException("A return type on a method marked with the \"ProxiedReturn\" " +
                        "attribute must be a deferred of an interface.");
                }

                DeferredObject convertedDeferred = Activator.CreateInstance(returnType) as DeferredObject;

                deferredPair.ReturnDeferred.ObjectCompletion(
                    delegate(object result)
                    {
                        return _queue.CreateProxy(interfaceType, _invoker, result);
                    },
                    DeferredObject.IdentityFailback,
                    convertedDeferred);

                return convertedDeferred;
            }
            else
            {
                return deferredPair.ReturnDeferred;
            }
        }
    }
}
