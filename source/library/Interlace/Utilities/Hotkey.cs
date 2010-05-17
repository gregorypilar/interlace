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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Microsoft;

#endregion

namespace Interlace.Utilities
{
    public class Hotkey : IDisposable
    {
        [DllImport("user32", SetLastError = true)]
        static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);

        [DllImport("user32", SetLastError = true)]
        static extern int UnregisterHotKey(IntPtr hwnd, int id);

        [DllImport("kernel32", SetLastError = true)]
        static extern short GlobalAddAtom(string lpString);

        [DllImport("kernel32", SetLastError = true)]
        static extern short GlobalDeleteAtom(short nAtom);

        const int AltModifier = 1;
        const int ControlModifier = 2;
        const int ShiftModifier = 4;

        int _keyNumber;
        int _keyModifiers;
        bool _enabled = false;
        short _uniqueNumber;
        static int _nextUniqueNumber = 0;

        HotkeyWindow _window;

        public event EventHandler Pressed;

        public Hotkey(HotkeyWindow window, Keys keyWithModifiers)
        {
            _window = window;

            _keyModifiers = 0;

            if ((keyWithModifiers & Keys.Shift) != 0) _keyModifiers |= ShiftModifier;
            if ((keyWithModifiers & Keys.Control) != 0) _keyModifiers |= ControlModifier;
            if ((keyWithModifiers & Keys.Alt) != 0) _keyModifiers |= AltModifier;

            _keyNumber = (int)keyWithModifiers & ~((int)Keys.Shift | (int)Keys.Control | (int)Keys.Alt);
        }

        ~Hotkey()
        {
            Dispose(false);
        }

        internal int KeyNumber
        {
            get { return _keyNumber; }
        }

        internal int KeyModifiers
        {
            get { return _keyModifiers; }
        }

        internal void PressReceivedByHotkeyWindow()
        {
            if (Pressed != null) Pressed(this, EventArgs.Empty);
        }

        public bool Enabled
        {
            get { return _enabled; }
            set 
            {
                if (value && !_enabled) InternalEnable();
                if (!value && _enabled) InternalDisable();
            }
        }

        void InternalEnable()
        {
            if (_enabled) throw new InvalidOperationException();

            _window.AttachHotkey(this);

            string uniqueName = string.Format("_hotkey_{0}_{1}",
                Thread.CurrentThread.ManagedThreadId, _nextUniqueNumber++);

            _uniqueNumber = GlobalAddAtom(uniqueName);

            if (_uniqueNumber == 0)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            try
            {
                if (RegisterHotKey(_window.Handle, _uniqueNumber, _keyModifiers, _keyNumber) == 0)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                _enabled = true;
            }
            catch (Exception)
            {
                GlobalDeleteAtom(_uniqueNumber);

                throw;
            }
        }

        void InternalDisable()
        {
            if (_enabled) throw new InvalidOperationException();

            UnregisterHotKey(_window.Handle, _uniqueNumber);
            GlobalDeleteAtom(_uniqueNumber);

            _window.DetachHotkey(this);

            _enabled = false;
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
                if (_enabled)
                {
                    UnregisterHotKey(_window.Handle, _uniqueNumber);
                    GlobalDeleteAtom(_uniqueNumber);
                }

                _disposed = true;
            }
        }
    }
}
