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
    public class HotkeyWindow : NativeWindow, IDisposable
    {
        Dictionary<int, Hotkey> _hotkeys = new Dictionary<int, Hotkey>();

        public HotkeyWindow()
        {
            CreateParams parameters = new CreateParams();
            CreateHandle(parameters);
        }

        const int WM_HOTKEY = 0x312;

        internal void AttachHotkey(Hotkey hotkey)
        {
            int combinedNumber = hotkey.KeyNumber << 16 | hotkey.KeyModifiers;

            if (_hotkeys.ContainsKey(combinedNumber)) 
            {
                throw new InvalidOperationException("A similar hotkey is already installed.");
            }

            _hotkeys[combinedNumber] = hotkey;
        }

        internal void DetachHotkey(Hotkey hotkey)
        {
            int combinedNumber = hotkey.KeyNumber << 16 | hotkey.KeyModifiers;

            _hotkeys.Remove(combinedNumber);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    int combinedNumber = m.LParam.ToInt32();

                    if (_hotkeys.ContainsKey(combinedNumber))
                    {
                        _hotkeys[combinedNumber].PressReceivedByHotkeyWindow();
                    }

                    break;
            }

            base.WndProc(ref m);
        }

        ~HotkeyWindow()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                DestroyHandle();

                _disposed = true;
            }
        }
    }
}
