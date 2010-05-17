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
using System.Text;
using System.Windows.Forms;

using Interlace.Utilities;

#endregion

namespace Interlace.ScreenShooting
{
    public class ScreenShotWindow
    {
        Rectangle _bounds;
        SystemWindow _systemWindow;
        List<ScreenShotWindow> _children;
        string _className;

        public static ScreenShotWindow FindWindowByPoint(ScreenShotWindow topWindow, Point point)
        {
            if (!topWindow.Bounds.Contains(point)) return null;

            ScreenShotWindow containingChild = null;

            foreach (ScreenShotWindow child in topWindow.PaintOrderChildren)
            {
                if (child.Bounds.Contains(point)) containingChild = child;
            }

            if (containingChild != null)
            {
                return FindWindowByPoint(containingChild, point);
            }
            else
            {
                return topWindow;
            }
        }

        public static Region FindWindowVisibleRegion(ScreenShotWindow topWindow, ScreenShotWindow window, bool includeChildWindows)
        {
            Stack<ScreenShotWindow> stack = new Stack<ScreenShotWindow>();

            Region region = new Region();
            region.MakeEmpty();

            try
            {
                stack.Push(topWindow);

                bool windowHasBeenDrawn = false;

                while (stack.Count > 0)
                {
                    ScreenShotWindow currentWindow = stack.Pop();

                    if (currentWindow == window)
                    {
                        region.Union(currentWindow.Bounds);
                        windowHasBeenDrawn = true;
                    }
                    else
                    {
                        if (windowHasBeenDrawn)
                        {
                            region.Exclude(currentWindow.Bounds);
                        }
                    }

                    if (currentWindow != window || !includeChildWindows)
                    {
                        foreach (ScreenShotWindow childWindow in currentWindow.Children)
                        {
                            stack.Push(childWindow);
                        }
                    }
                }

                return region;
            }
            catch (Exception)
            {
                region.Dispose();

                throw;
            }
        }

        public string ClassName
        { 	 
           get { return _className; }
        }

        public List<ScreenShotWindow> Children
        { 	 
           get { return _children; }
        }

        public IEnumerable<ScreenShotWindow> PaintOrderChildren
        {
            get
            {
                for (int i = 0; i < _children.Count; i++)
                {
                    yield return _children[_children.Count - 1 - i];
                }
            }
        }

        public static ScreenShotWindow ScreenWindow
        {
            get 
            {
                ScreenShotWindow window = new ScreenShotWindow(Screen.PrimaryScreen);

                window.PopulateAncestors();

                return window;
            }
        }

        protected ScreenShotWindow(Screen screen)
        {
            _systemWindow = null;
            _bounds = screen.Bounds;
            _className = "(Screen)";
        }

        protected ScreenShotWindow(SystemWindow systemWindow)
        {
            _systemWindow = systemWindow;
            _bounds = systemWindow.Bounds;
            _className = systemWindow.ClassName;
        }

        public Rectangle Bounds
        { 	 
           get { return _bounds; }
        }

        public void CullObscuredAncestors()
        {
            CullObscuredWindows();

            foreach (ScreenShotWindow window in _children)
            {
                window.CullObscuredAncestors();
            }
        }

        public void CullObscuredWindows()
        {
            List<ScreenShotWindow> newChildren = new List<ScreenShotWindow>();

            for (int i = 0; i < _children.Count; i++)
            {
                ScreenShotWindow child = _children[i];
                Rectangle bounds = child.Bounds;

                bool obscured = false;

                for (int j = 0; j < i; j++)
                {
                    if (_children[j].Bounds.Contains(bounds)) obscured = true;
                }

                if (!obscured) newChildren.Add(child);
            }

            _children = newChildren;
        }

        protected void PopulateAncestors()
        {
            _children = new List<ScreenShotWindow>();

            IList<SystemWindow> childWindows;

            if (_systemWindow == null)
            {
                childWindows = SystemWindow.TopLevelWindows;
            }
            else
            {
                childWindows = _systemWindow.ChildWindows;
            }

            foreach (SystemWindow childSystemWindow in childWindows)
            {
                if (!childSystemWindow.IsVisible) continue;

                ScreenShotWindow window = new ScreenShotWindow(childSystemWindow);

                _children.Add(window);

                window.PopulateAncestors();
            }
        }
    }
}
