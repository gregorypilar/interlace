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
using Interlace.Utilities;

#endregion

namespace Interlace.ReactorService
{
    public class BlockingReactorThreadInvoker : IThreadInvoker
    {
        delegate void QueueDelegate();

        ReactorThreadInvokerQueue _queue;

        public BlockingReactorThreadInvoker(ReactorThreadInvokerQueue queue)
        {
            _queue = queue;
        }

        #region IThreadInvoker Members

        public object Invoke(Delegate method)
        {
            return Invoke(method, new object[] { });
        }

        public object Invoke(Delegate method, params object[] args)
        {
            object result = null;
            Exception exception = null;

            using (ManualResetEvent waitHandle = new ManualResetEvent(false))
            {
                _queue._queue.Enqueue(
                    delegate()
                    {
                        try
                        {
                            result = method.DynamicInvoke(args);
                        }
                        catch (Exception e)
                        {
                            exception = e;
                        }

                        waitHandle.Set();
                    });

                waitHandle.WaitOne();
            }

            if (exception != null)
            {
                throw new ReactorInvocationException("An exception occurred invoking a method " +
                    "in a reactor thread; check InnerException for the exception.", exception);
            }
            else
            {
                return result;
            }
        }

        #endregion
    }
}
