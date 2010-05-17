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

#endregion

namespace Interlace.Utilities
{
    public static class Proxies
    {
        static ProxyGeneratorCore _generator;
        static Dictionary<Type, ProxyFactoryBase> _proxyFactories;
        static ReaderWriterLock _lock;

        static Proxies()
        {
            _generator = new ProxyGeneratorCore();
            _proxyFactories = new Dictionary<Type, ProxyFactoryBase>();
            _lock = new ReaderWriterLock();
        }

        public static TInterface MakeProxy<TInterface>(IProxyHandler handler) where TInterface : class
        {
            return MakeProxyFactory<TInterface>().Create(handler) as TInterface;
        }

        public static object MakeProxy(Type interfaceType, IProxyHandler handler) 
        {
            return MakeProxyFactory(interfaceType).CreateBase(handler);
        }

        public static ProxyFactory<TInterface> MakeProxyFactory<TInterface>() where TInterface : class
        {
            return MakeProxyFactory(typeof(TInterface)) as ProxyFactory<TInterface>;
        }

        public static ProxyFactoryBase MakeProxyFactory(Type interfaceType) 
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException("A proxy factory can only be generated for an interface.", "interfaceType");
            }
            
            _lock.AcquireReaderLock(-1);

            try
            {
                if (_proxyFactories.ContainsKey(interfaceType))
                {
                    return _proxyFactories[interfaceType];
                }
                else
                {
                    _lock.UpgradeToWriterLock(-1);

                    ProxyFactoryBase proxyFactory = _generator.GenerateProxy(interfaceType);
                    _proxyFactories[interfaceType] = proxyFactory;

                    return proxyFactory;
                }
            }
            finally
            {
                _lock.ReleaseLock();
            }
        }
    }
}
