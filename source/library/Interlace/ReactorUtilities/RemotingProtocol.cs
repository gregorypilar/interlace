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

using Interlace.Utilities;

#endregion

namespace Interlace.ReactorUtilities
{
    public class RemotingProtocol : SerializeProtocol
    {
        // Objects implemented and offered by this side for the other side to call are "stubs":
        Dictionary<int, RemotingStub> _stubs;

        // Each stub is given a unique integer, with the root having a permanent one of "0":
        const int _rootStubId = 0;
        int _nextStubId = 1;

        // Objects existing on the remote side for us to call are "references". Only the root
        // references is cached; others must have one proxy for each time the object was
        // sent to us, otherwise the simplistic distributed garbage collection fails:
        object _remoteRootObjectProxy = null;
        Type _remoteRootObjectProxyType = null;

        // Each time we send a request it is given an asynchronous completion token or "cookie":
        int _nextCompletionCookie = 0;

        // Details of the request must be held (keyed by cookie number) until the response is received:
        Dictionary<int, RemotingRequestToken> _tokens;

        public RemotingProtocol()
        {
            _stubs = new Dictionary<int, RemotingStub>();

            _tokens = new Dictionary<int, RemotingRequestToken>();
        }

        RemotingStub CreateStub(Type type, object implementation)
        {
            RemotingStub stub = new RemotingStub(type, implementation, _nextStubId);
            _nextStubId++;

            _stubs[stub.StubId] = stub;

            return stub;
        }

        public void RegisterLocalRootObject<TInterface>(object implementation) where TInterface : class
        {
            if (_stubs.Count != 0) throw new InvalidOperationException();

            if (_stubs.ContainsKey(_rootStubId))
            {
                throw new InvalidOperationException(
                    "A root object has already been registered; root objects can not be changed.");
            }

            if (implementation as TInterface == null)
            {
                throw new InvalidOperationException("The stub does not implement the specified interface.");
            }

            RemotingStub rootStub = new RemotingStub(typeof(TInterface), implementation, _rootStubId);

            _stubs[rootStub.StubId] = rootStub;
        }

        public TInterface GetRemoteRootObject<TInterface>() where TInterface : class
        {
            if (_remoteRootObjectProxy == null)
            {
                _remoteRootObjectProxy = Proxies.MakeProxy<TInterface>(
                    new RemotingProxy(_rootStubId, typeof(TInterface), this, false));

                _remoteRootObjectProxyType = typeof(TInterface);
            }
            else
            {
                if (!_remoteRootObjectProxyType.Equals(typeof(TInterface)))
                {
                    throw new InvalidOperationException(string.Format(
                        "The remote root object has already been requested as a \"{0}\" type object. The " +
                        "request for a \"{1}\" type object is therefore invalid.",
                        _remoteRootObjectProxyType.Name, typeof(TInterface).Name));
                }
            }

            return _remoteRootObjectProxy as TInterface;
        }

        internal int GetNextCompletionCookieAndAdvance()
        {
            int cookie = _nextCompletionCookie;
            _nextCompletionCookie++;

            return cookie;
        }

        internal void SendRequest(RemotingRequestToken token)
        {
            _tokens.Add(token.Request.CompletionCookie, token);

            SendObject(token.Request);
        }

        protected override void HandleReceivedObject(object obj)
        {
            if (obj is RemotingRequest)
            {
                RemotingRequest request = obj as RemotingRequest;

                HandleReceivedRequest(request);
            }
            else if (obj is RemotingResponse)
            {
                RemotingResponse response = obj as RemotingResponse;

                HandleReceivedResponse(response);
            }
            else if (obj is RemotingExceptionResponse)
            {
                RemotingExceptionResponse exceptionResponse = obj as RemotingExceptionResponse;

                HandleReceivedExceptionResponse(exceptionResponse);
            }
            else if (obj is RemotingProtocolError)
            {
            }
            else
            {
                SendObject(new RemotingProtocolError(RemotingProtocolErrorKind.InvalidType));
            }
        }

        void HandleReceivedRequest(RemotingRequest request)
        {
            // Find the stub and the method to call on it:
            if (!_stubs.ContainsKey(request.StubId))
            {
                SendObject(new RemotingProtocolError(RemotingProtocolErrorKind.ReferenceNotFound));
            }

            RemotingStub stub = _stubs[request.StubId];

            if (stub.Type.Name != request.InterfaceName)
            {
                SendObject(new RemotingProtocolError(RemotingProtocolErrorKind.ReferenceInterfaceNotFound));
            }

            MethodInfo method = stub.Type.GetMethod(request.MethodName);

            if (method == null) 
            {
                SendObject(new RemotingProtocolError(RemotingProtocolErrorKind.InterfaceMethodNotSupported));
            }

            // Check for proxied return values (interfaces that become new stubs):
            ObjectCallback callback;

            if (method.GetCustomAttributes(typeof(ProxiedReturnAttribute), true).Length > 0)
            {
                Type returnType = method.ReturnType;

                if (!Deferred.IsTypedDeferred(returnType))
                {
                    throw new InvalidOperationException(
                        "The type of a deferred returned from a proxied return method must be a typed deferred.");
                }

                Type interfaceType = Deferred.DeferredResultType(returnType);

                if (!interfaceType.IsInterface)
                {
                    throw new InvalidOperationException(
                        "The type of a deferred returned from a proxied return method must be a deferred returning an interface.");
                }

                // Create a stub with the resulting interface, and return a reference to the remote side:
                callback = delegate(object result)
                    {
                        object replacedResult = null;

                        if (result != null)
                        {
                            RemotingStub newStub = CreateStub(interfaceType, result);

                            replacedResult = new RemotingReference(newStub.StubId);
                        }

                        RemotingResponse response = 
                            new RemotingResponse(request.CompletionCookie, replacedResult);

                        SendObject(response);

                        return null;
                    };
            }
            else
            {
                // Otherwise, just return the result:
                callback = delegate(object result)
                    {
                        RemotingResponse response = new RemotingResponse(request.CompletionCookie, result);

                        SendObject(response);

                        return null;
                    };
            }

            try
            {
                // Invoke the stub method:
                DeferredObject deferred = method.Invoke(stub.Implementation, request.Arguments) as DeferredObject;

                deferred.ObjectCompletion(callback,
                    delegate(DeferredFailure failure)
                    {
                        SendObject(new RemotingExceptionResponse(request.CompletionCookie, failure.Exception));

                        return null;
                    }, null);
            }
            catch (TargetInvocationException e)
            {
                SendObject(new RemotingExceptionResponse(request.CompletionCookie, e.InnerException));
            }
            catch (Exception e)
            {
                SendObject(new RemotingExceptionResponse(request.CompletionCookie, e));
            }
        }

        RemotingRequestToken HandleTokenLookupAndRemovalForResponse(int completionCookie)
        {
            // Check if the response is expected:
            if (!_tokens.ContainsKey(completionCookie))
            {
                SendObject(new RemotingProtocolError(RemotingProtocolErrorKind.CompletionTokenNotRecognised));

                return null;
            }

            // Retrieve and remove it form the expected response dictionary:
            RemotingRequestToken token = _tokens[completionCookie];
            _tokens.Remove(completionCookie);

            return token;
        }

        void HandleReceivedResponse(RemotingResponse response)
        {
            RemotingRequestToken token = HandleTokenLookupAndRemovalForResponse(response.CompletionCookie);

            if (token == null) return;

            object returnedResult = response.Result;

            // If a remoting reference was returned, replace it with a proxy:
            if (response.Result is RemotingReference)
            {
                if (!Deferred.IsTypedDeferred(token.ReturnType))
                {
                    SendObject(new RemotingProtocolError(RemotingProtocolErrorKind.Unknown));

                    return;
                }

                Type interfaceType = Deferred.DeferredResultType(token.ReturnType);

                if (!interfaceType.IsInterface)
                {
                    SendObject(new RemotingProtocolError(RemotingProtocolErrorKind.Unknown));

                    return;
                }

                RemotingReference reference = response.Result as RemotingReference;

                returnedResult = Proxies.MakeProxy(interfaceType,
                    new RemotingProxy(reference.StubId, interfaceType, this, true));
            }

            // Complete the deferred:
            token.Deferred.SucceedObject(returnedResult);
        }

        void HandleReceivedExceptionResponse(RemotingExceptionResponse exceptionResponse)
        {
            RemotingRequestToken token = HandleTokenLookupAndRemovalForResponse(exceptionResponse.CompletionCookie);

            if (token == null) return;

            if (exceptionResponse.Exception != null)
            {
                token.Deferred.Fail(DeferredFailure.FromException(
                    new RemotingException("An exception occurred accessing a remote service. The inner exception contains more details.", 
                    exceptionResponse.Exception)));
            }
            else
            {
                token.Deferred.Fail(DeferredFailure.FromException(
                    new RemotingException("An exception occurred accessing a remote service, but the \"{0}\" exception could not be serialized.", 
                    exceptionResponse.Exception)));
            }
        }
    }
}
