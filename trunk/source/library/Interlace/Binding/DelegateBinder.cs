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
using System.Reflection;
using System.Text;

using Interlace.Binding.Views;

#endregion

namespace Interlace.Binding
{
    public delegate void DelegateBinderDelegate<TSender, TProperty>(TSender sender, TProperty value);
    public delegate void DelegateBinderDelegate<TProperty>(TProperty value);
    public delegate void DelegateBinderDelegate();

    public class DelegateBinder
    {
        object _boundTo = null;
        Type _boundToType = null;

        Dictionary<string, List<InternalDelegateBinderDelegate>> _bindings;

        public delegate void InternalDelegateBinderDelegate(object sender, object value);

        public DelegateBinder()
        {
            _bindings = new Dictionary<string, List<InternalDelegateBinderDelegate>>();
        }

        public DelegateBinder(object boundTo)
        : this()
        {
            BoundTo = boundTo;
        }

        public DelegateBinder(Interlace.Binding.Binder boundToBinder, string propertyName)
        : this()
        {
            boundToBinder.AddBinding(propertyName, new PropertyView(this, "BoundTo", null));
        }

        public object BoundTo
        {
            get { return _boundTo; }
            set 
            {
                if (_boundTo == value) return;

                if (_boundTo != null) DisconnectEvents();

                _boundTo = value; 

                if (_boundTo != null)
                {
                    _boundToType = _boundTo.GetType();

                    ConnectEvents();

                    FireAllBindings();
                }
                else
                {
                    _boundToType = null;
                }
            }
        }

        void ConnectEvents()
        {
            if (_boundTo is INotifyPropertyChanged)
            {
                INotifyPropertyChanged notifier = _boundTo as INotifyPropertyChanged;

                notifier.PropertyChanged += new PropertyChangedEventHandler(PropertyChangedEventHandler);
            }
            else
            {
                throw new InvalidOperationException(
                    "The DelegateBinder can only be bound to objects implementing INotifyPropertyChanged.");
            }
        }

        void DisconnectEvents()
        {
            if (_boundTo is INotifyPropertyChanged)
            {
                INotifyPropertyChanged notifier = _boundTo as INotifyPropertyChanged;

                notifier.PropertyChanged -= new PropertyChangedEventHandler(PropertyChangedEventHandler);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        void FireAllBindings()
        {
            foreach (string propertyName in _bindings.Keys)
            {
                FireBinding(propertyName);
            }
        }

        void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            FireBinding(e.PropertyName);
        }

        void FireBinding(string propertyName)
        {
            if (_bindings.ContainsKey(propertyName))
            {
                object value = null;

                PropertyInfo propertyInfo = _boundToType.GetProperty(propertyName);
                if (propertyInfo != null) value = propertyInfo.GetValue(_boundTo, null);

                foreach (InternalDelegateBinderDelegate binderDelegate in _bindings[propertyName])
                {
                    binderDelegate(_boundTo, value);
                }
            }
        }

        public void Bind<TSender, TProperty>(string propertyName, DelegateBinderDelegate<TSender, TProperty> bindTo)
        {
            Bind(propertyName, 
                (InternalDelegateBinderDelegate)delegate(object sender, object value) { bindTo((TSender)sender, (TProperty)value); });
        }

        public void Bind<TProperty>(string propertyName, DelegateBinderDelegate<TProperty> bindTo)
        {
            Bind(propertyName, 
                (InternalDelegateBinderDelegate)delegate(object sender, object value) { bindTo((TProperty)value); });
        }

        public void Bind(string propertyName, DelegateBinderDelegate bindTo)
        {
            Bind(propertyName, 
                (InternalDelegateBinderDelegate)delegate(object sender, object value) { bindTo(); });
        }

        void Bind(string propertyName, InternalDelegateBinderDelegate bindTo)
        {
            if (!_bindings.ContainsKey(propertyName))
            {
                _bindings[propertyName] = new List<InternalDelegateBinderDelegate>();
            }

            _bindings[propertyName].Add(bindTo);
        }
    }
}
