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
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraBars;

#endregion

namespace Interlace.Binding.Views
{
    public class EnabledView : BinderViewBase
    {
        delegate void EnabledSetDelegate(bool enabled);

        EnabledSetDelegate _setDelegate;
        bool _unboundValue = false;

        public EnabledView(params Control[] boundControls)
        {
            ViewToModelDisabled = true;

            // Take a private copy:
            Control[] boundControlsCopy = new Control[boundControls.Length];
            Array.Copy(boundControls, boundControlsCopy, boundControlsCopy.Length);

            _setDelegate = delegate(bool enabled)
            {
                foreach (Control control in boundControlsCopy)
                {
                    control.Enabled = enabled;
                }
            };
        }

        public EnabledView(params BarItem[] boundBarItems)
        {
            ViewToModelDisabled = true;

            // Take a private copy:
            BarItem[] boundBarItemsCopy = new BarItem[boundBarItems.Length];
            Array.Copy(boundBarItems, boundBarItemsCopy, boundBarItemsCopy.Length);

            _setDelegate = delegate(bool enabled)
            {
                foreach (BarItem barItem in boundBarItemsCopy)
                {
                    if (barItem.Enabled != enabled) barItem.Enabled = enabled;
                }
            };
        }

        public static EnabledView FromReflection<T>(params T[] objects)
        {
            // Take a private copy:
            T[] objectsCopy = new T[objects.Length];
            Array.Copy(objects, objectsCopy, objects.Length);

            Type objectType = typeof(T);
            PropertyInfo property = objectType.GetProperty("Enabled");

            return new EnabledView(delegate(bool enabled)
            {
                foreach (T item in objectsCopy)
                {
                    property.SetValue(item, enabled, null);
                }
            });
        }

        EnabledView(EnabledSetDelegate setDelegate)
        {
            ViewToModelDisabled = true;

            _setDelegate = setDelegate;
        }

        protected override void OnModelChanged(object value)
        {
            bool valueToSet;

            if (value == BinderNotBound.Value || value == BinderMissingProperty.Value)
            {
                valueToSet = _unboundValue;
            }
            else if (value is bool)
            {
                valueToSet = (bool)value;
            }
            else
            {
                valueToSet = value != null;
            }

            _setDelegate(valueToSet);
        }
    }
}
