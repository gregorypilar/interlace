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

#endregion

namespace Interlace.ReactorUtilities
{
    public delegate object ObjectFailback(DeferredFailure failure);
    public delegate object ObjectCallback(object value);


    public class DeferredObject
    {
        object _result = null;

        bool _completed = false;

        ObjectCallback _callback = null;
        ObjectFailback _failback = null;

        DeferredObject _next = null;

        public DeferredObject()
        {
        }

        public void SucceedObject(object value)
        {
            if (_completed) throw new InvalidOperationException();

            _result = value;
            _completed = true;

            RunBacks();
        }

        public void Fail(DeferredFailure failure)
        {
            if (_completed) throw new InvalidOperationException();

            _result = failure;
            _completed = true;

            RunBacks();
        }

        public void Fail(Exception ex)
        {
            Fail(DeferredFailure.FromException(ex));
        }

        void RunBacks()
        {
            if (_callback == null) return;

            try
            {
                if (!(_result is DeferredFailure))
                {
                    _result = _callback(_result);
                }
                else
                {
                    _result = _failback(_result as DeferredFailure);
                }
            }
            catch (Exception e)
            {
                _result = DeferredFailure.FromException(e);
            }

            if (_next != null)
            {
                if (!(_result is DeferredFailure))
                {
                    _next.SucceedObject(_result);
                }
                else
                {
                    _next.Fail(_result as DeferredFailure);
                }
            }
        }

        public void ObjectCompletion(ObjectCallback callback, ObjectFailback failback, DeferredObject nextOrNull)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            if (failback == null) throw new ArgumentNullException("failback");

            if (_callback != null) throw new InvalidOperationException(
                "A completion callback has already been set on this deferred.");

            _callback = callback;
            _failback = failback;
            _next = nextOrNull;

            if (_completed)
            {
                RunBacks();
            }
        }

        public void ObjectCompletion(DeferredObject nextOrNull)
        {
            ObjectCompletion(IdentityCallback, IdentityFailback, nextOrNull);
        }

        public static object IdentityCallback(object result)
        {
            return result;
        }

        public static object IdentityFailback(DeferredFailure failback)
        {
            return failback;
        }
    }
}
