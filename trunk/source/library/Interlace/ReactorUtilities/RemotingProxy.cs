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
    public class RemotingProxy : IProxyHandler
    {
        int _stubId;
        Type _interfaceType;
        RemotingProtocol _protocol;
        string _interfaceName;
        bool _notifyProtocolOfFinalize;

        public RemotingProxy(int stubId, Type interfaceType, RemotingProtocol protocol, bool notifyProtocolOfFinalize)
        {
            _stubId = stubId;
            _interfaceType = interfaceType;
            _protocol = protocol;
            _interfaceName = _interfaceType.Name;
            _notifyProtocolOfFinalize = notifyProtocolOfFinalize;

            // TODO: Implement notifying the protocol of finalization.
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
                throw new InvalidOperationException("Remoting interface methods must return a deferred.");
            }

            RemotingRequest request = new RemotingRequest(_protocol.GetNextCompletionCookieAndAdvance(),
                _stubId, _interfaceName, method.Name, arguments);

            DeferredObject deferred = Activator.CreateInstance(returnType) as DeferredObject;

            RemotingRequestToken token = new RemotingRequestToken(request, deferred, returnType);

            _protocol.SendRequest(token);

            return deferred;
        }
    }
}
