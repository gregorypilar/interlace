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

namespace Interlace.ReactorCore
{
    public class TimerQueueEntry : IComparable
    {
        DateTime _fireAt;
        TimerCallback _callback;
        object _state;
        bool _fired;
        bool _cancelled;

        public TimerQueueEntry(DateTime fireAt, TimerCallback callback, object state)
        {
            _callback = callback;
            _state = state;
            _fireAt = fireAt;
            _fired = false;
            _cancelled = false;
        }

        public bool IsFireable(DateTime atTime)
        {
            return _fireAt <= atTime;
        }

        public TimeSpan GetTimeUntilFireable(DateTime atTime)
        {
            return _fireAt - atTime;
        }

        public void Cancel()
        {
            _cancelled = true;
        }

        public void Fire()
        {
            if (!_cancelled)
            {
                if (_fired) throw new InvalidOperationException("The timer has already been fired.");

                try
                {
                    _callback(_fireAt, _state);
                }
                finally
                {
                    _fired = true;
                }
            }
        }

        public int CompareTo(object obj)
        {
            TimerQueueEntry rhs = obj as TimerQueueEntry;

            if (rhs == null) throw new InvalidOperationException("A timer queue entry can not " +
                "be compared with any other object type or null references.");

            return _fireAt.CompareTo(rhs._fireAt);
        }
    }
}
