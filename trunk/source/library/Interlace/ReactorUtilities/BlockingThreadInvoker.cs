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
using System.Windows.Forms;

using Interlace.ReactorCore;
using Interlace.Utilities;

#endregion

namespace Interlace.ReactorUtilities
{
    /// <summary>
    /// An <see cref="IThreadInvoker"/> implementation that blocks. Great care should be taken
    /// when using this class; see the remarks.
    /// </summary>
    /// <remarks>
    /// This class should only be used for testing. There are a number of traps when using it.
    /// First, if the called method needs a running UI thread or reactor thread, a deadlock will 
    /// most likely occur. Also, the callbacks will fail as soon as this object is disposed, which
    /// may be too soon.
    /// </remarks>
    public class BlockingThreadInvoker : IDisposable, IThreadInvoker
    {
        AutoResetEvent _requestSignal;
        AutoResetEvent _responseSignal;

        object _requestLock = new object();
        Delegate _requestDelegate = null;
        object[] _requestArguments = null;

        object _responseResult = null;

        bool _disposed = false;

        public BlockingThreadInvoker()
        {
            _requestSignal = new AutoResetEvent(false);
            _responseSignal = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            lock (_requestLock)
            {
                _disposed = true;
            }

            if (_requestSignal != null)
            {
                _requestSignal.Close();
                _requestSignal = null;
            }

            if (_responseSignal != null)
            {
                _responseSignal.Close();
                _responseSignal = null;
            }
        }

        object WaitOnInternal(DeferredObject deferred)
        {
            object result = null;
            Exception exception = null;
            bool completed = false;

            deferred.ObjectCompletion(
                delegate(object deferredResult)
                {
                    result = deferredResult;
                    completed = true;

                    return null;
                },
                delegate(DeferredFailure failure)
                {
                    exception = failure.Exception;
                    completed = true;

                    return null;
                }, null);

            while (!completed)
            {
                _requestSignal.WaitOne();

                _responseResult = _requestDelegate.DynamicInvoke(_requestArguments);

                _requestDelegate = null;
                _requestArguments = null;

                _responseSignal.Set();
            }

            if (exception != null)
            {
                throw new ApplicationException(
                    "An exception was thrown while running callbacks. Check InnerException for details.", exception);
            }

            return result;
        }

        public void WaitOn(VoidDeferred deferred)
        {
            WaitOnInternal(deferred);
        }

        public T WaitOn<T>(Deferred<T> deferred)
        {
            return (T)WaitOnInternal(deferred);
        }

        #region IThreadInvoker Members

        public object Invoke(Delegate method)
        {
            return Invoke(method, new object[] { });
        }

        public object Invoke(Delegate method, params object[] args)
        {
            lock (_requestLock)
            {
                if (_disposed) throw new InvalidOperationException(
                    "The blocking thread invoker has been disposed; invokes should not be made.");

                _requestDelegate = method;
                _requestArguments = args;

                _requestSignal.Set();

                _responseSignal.WaitOne();

                object result = _responseResult;
                _responseResult = null;

                return result;
            }
        }

        #endregion
    }
}
