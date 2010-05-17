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
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Interlace.Utilities
{
    public class SystemWindow
    {
        [ThreadStatic]
        static List<IntPtr> _enumeratingIntoList;

        readonly IntPtr _handle;

        delegate bool EnumWindowsCallbackDelegate(IntPtr handle, int parameter);

        struct SystemRect
        {
            int _left;
            int _top;
            int _right;
            int _bottom;

            public Rectangle Rectangle
            {
                get { return Rectangle.FromLTRB(_left, _top, _right, _bottom); }
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsCallbackDelegate callback, int parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumChildWindows(IntPtr handle, EnumWindowsCallbackDelegate callback, int parameter);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr handle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr handle, out SystemRect rectangle);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr handle, out uint processId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetClassName(IntPtr handle, StringBuilder name, int nameCapacity);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll", SetLastError = false)]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        SystemWindow(IntPtr handle)
        {
            _handle = handle;
        }

        static bool EnumerateWindowsCallback(IntPtr handle, int parameter)
        {
            if (_enumeratingIntoList != null)
            {
                _enumeratingIntoList.Add(handle);
            }

            return true;
        }

        static SystemWindow HandleOrNullToSystemWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return null;

            return new SystemWindow(handle);
        }

        public static SystemWindow Find(string windowName)
        {
            return HandleOrNullToSystemWindow(FindWindow(null, windowName));
        }

        public static SystemWindow Find(string className, string windowName)
        {
            return HandleOrNullToSystemWindow(FindWindow(className, windowName));
        }

        public static IList<SystemWindow> TopLevelWindows
        {
            get
            {
                try
                {
                    _enumeratingIntoList = new List<IntPtr>();

                    EnumWindows(EnumerateWindowsCallback, 0);

                    List<SystemWindow> windows = new List<SystemWindow>();

                    foreach (IntPtr handle in _enumeratingIntoList)
                    {
                        windows.Add(new SystemWindow(handle));
                    }

                    return windows;
                }
                finally
                {
                    _enumeratingIntoList = null;
                }
            }
        }

        public IList<SystemWindow> ChildWindows
        {
            get
            {
                try
                {
                    _enumeratingIntoList = new List<IntPtr>();

                    EnumChildWindows(_handle, EnumerateWindowsCallback, 0);

                    List<SystemWindow> windows = new List<SystemWindow>();

                    foreach (IntPtr handle in _enumeratingIntoList)
                    {
                        windows.Add(new SystemWindow(handle));
                    }

                    return windows;
                }
                finally
                {
                    _enumeratingIntoList = null;
                }
            }
        }

        public int SendMessage(int message, int lParam, int wParam)
        {
            return SendMessage(_handle, message, wParam, lParam);
        }

        public IntPtr Handle
        { 	 
            get { return _handle; }
        }

        public Rectangle Bounds
        {
            get
            {
                SystemRect rect;

                GetWindowRect(_handle, out rect);

                return rect.Rectangle;
            }
        }

        public bool IsVisible
        {
            get { return IsWindowVisible(_handle); }
        }

        public int ProcessId
        {
            get
            {
                uint processId;

                GetWindowThreadProcessId(_handle, out processId);

                return (int)processId;
            }
        }

        public string ClassName
        {
            get
            {
                StringBuilder builder = new StringBuilder(256);

                GetClassName(_handle, builder, builder.Capacity);

                return builder.ToString();
            }
        }

        public SystemWindow DesktopWindow
        {
            get
            {
                return new SystemWindow(GetDesktopWindow());
            }
        }

        public bool IsDesktopWindow
        {
            get { return _handle == GetDesktopWindow(); }
        }
    }
}
