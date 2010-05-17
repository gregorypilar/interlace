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

using Interlace.Collections;

#endregion

namespace Interlace.ReactorCore
{
    public class TimerQueue
    {
        PriorityQueue<TimerQueueEntry> _queue = new PriorityQueue<TimerQueueEntry>();

        public TimerQueue()
        {
        }

        public TimerHandle Add(DateTime fireAt, TimerCallback callback, object state)
        {
            TimerQueueEntry entry = new TimerQueueEntry(fireAt, callback, state);

            _queue.Enqueue(entry);

            return new TimerHandle(entry);
        }

        public bool IsEmpty
        {
            get { return _queue.Count == 0; }
        }

        readonly TimeSpan _zeroTimeSpan = new TimeSpan(0);

        public TimeSpan GetTimeUntilNextFireable(DateTime now)
        {
            if (IsEmpty) throw new InvalidOperationException("The time until the next timer " +
                "is not available on an empty queue.");

            TimerQueueEntry entry = _queue.Peek();

            TimeSpan time = entry.GetTimeUntilFireable(now);

            if (time < _zeroTimeSpan) return _zeroTimeSpan;

            return time;
        }

        public void FireAllFireable(DateTime now)
        {
            while (_queue.Count > 0)
            {
                TimerQueueEntry entry = _queue.Peek();

                if (!entry.IsFireable(now)) break;

                _queue.Dequeue();

                entry.Fire();
            }
        }

        public void CancelAll()
        {
            while (_queue.Count > 0)
            {
                TimerQueueEntry entry = _queue.Dequeue();
                entry.Cancel();
            }
        }
    }
}
