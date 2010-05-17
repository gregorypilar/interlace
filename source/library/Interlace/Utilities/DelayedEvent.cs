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
using System.Windows.Forms;

#endregion

namespace Interlace.Utilities
{
    /// <summary>
    /// Hooks an event of any type and uses it to start a countdown that triggers a second
    /// event. Any event arguments of the first event are not kept.
    /// </summary>
    /// <typeparam name="TEventArgs">The event that triggers the countdown.</typeparam>
    public class DelayedEvent<TEventArgs> where TEventArgs : EventArgs
    {
        public event EventHandler Triggered;
        public event EventHandler<TEventArgs> TriggeredWithArgs;

        Timer _timer;

        TimeSpan _lag;

        bool _isActive;
        DateTime _elapsesAt;

        object _delayedSender;
        TEventArgs _delayedArgs;

        public DelayedEvent(Form owningForm)
        {
            _isActive = false;

            _timer = new Timer();
            _timer.Interval = 100;
            _timer.Start();

            _timer.Tick += new EventHandler(_timer_Tick);

            owningForm.FormClosed += new FormClosedEventHandler(owningForm_FormClosed);
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            if (_isActive && DateTime.Now > _elapsesAt)
            {
                _isActive = false;

                object delayedSender = _delayedSender;
                TEventArgs delayedArgs = _delayedArgs;

                _delayedSender = null;
                _delayedArgs = null;

                if (Triggered != null) Triggered(this, EventArgs.Empty);
                if (TriggeredWithArgs != null) TriggeredWithArgs(delayedSender, delayedArgs);
            }
        }

        void owningForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _timer.Stop();

            _timer.Tick -= new EventHandler(_timer_Tick);
            (sender as Form).FormClosed -= new FormClosedEventHandler(owningForm_FormClosed);
        }

        public void HandleTriggeringEvent(object sender, TEventArgs e)
        {
            _isActive = true;
            _elapsesAt = DateTime.Now + Lag;

            _delayedSender = sender;
            _delayedArgs = e;
        }

        public TimeSpan Lag
        {
            get { return _lag; }
            set { _lag = value; }
        }
    }
}
