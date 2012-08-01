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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

#endregion

namespace Interlace.Binding
{
    public class CountBinderModel : IBinderModel
    {
        ICollection _boundTo;
        Type _boundToType;

        private BinderController _controller;

        public CountBinderModel()
        {
            _boundTo = null;
        }

        public BinderController Controller
        {
            set { _controller = value; }
        }

        public string GetDescriptionForTracing()
        {
            if (_boundTo == null) return "Count of null";

            return string.Format("Count of \"{0}\", type \"{1}\"",
                _boundTo, _boundToType.Name);
        }

        private void ConnectEvents()
        {
            EventInfo changeEvent = _boundToType.GetEvent("ListChanged");

            if (changeEvent != null)
            {
                changeEvent.AddEventHandler(_boundTo, new ListChangedEventHandler(ListChangedEventHandler));
            }
        }

        private void DisconnectEvents()
        {
            EventInfo changeEvent = _boundToType.GetEvent("ListChanged");

            if (changeEvent != null)
            {
                changeEvent.RemoveEventHandler(_boundTo, new ListChangedEventHandler(ListChangedEventHandler));
            }
        }

        public void ConnectBoundToObject(object boundTo)
        {
            if (_boundTo != null) DisconnectEvents();

            _boundTo = boundTo as ICollection;

            if (_boundTo != null)
            {
                _boundToType = _boundTo.GetType();
                ConnectEvents();
            }

            _controller.OnModelModified();
        }

        public void DisconnectBoundToObject()
        {
            if (_boundTo != null)
            {
                DisconnectEvents();
            }

            _boundTo = null;
            _boundToType = null;

            _controller.OnModelModified();
        }

        private void ListChangedEventHandler(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemChanged:
                case ListChangedType.ItemMoved:
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted:
                    // Ignore these events for the count.
                    return;

                default:
                    _controller.OnModelModified();
                    break;
            }
        }

        public object GetValue()
        {
            if (_boundTo == null) return BinderNotBound.Value;

            return _boundTo.Count;
        }

        public void SetValue(object value)
        {
            return;
        }
    }
}
