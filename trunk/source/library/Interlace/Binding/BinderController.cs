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

namespace Interlace.Binding
{
    using Views;

    public class BinderController
    {
        List<BinderViewBase> _views;
        IBinderModel _model;
        bool _tracingEnabled;

        internal BinderController(IBinderModel model)
        {
            _views = new List<BinderViewBase>();
            _model = model;
            _model.Controller = this;
        }

        public void OnModelModified()
        {
            object newValue = _model.GetValue();

#if DEBUG
            if (_tracingEnabled)
            {
                Console.WriteLine(string.Format("Binder: Model ({0}) -> Views ({1}), Value \"{2}\".",
                    _model.GetDescriptionForTracing(), _views.Count, newValue));
            }
#endif

            foreach (BinderViewBase view in _views)
            {
                view.SetValue(newValue);
            }
        }

        public void OnViewModified(BinderViewBase view, object newValue)
        {
            _model.SetValue(newValue);

#if DEBUG
            if (_tracingEnabled)
            {
                Console.WriteLine(string.Format("Binder, View -> Model ({0}), Value \"{1}\".",
                    _model.GetDescriptionForTracing(), newValue));
            }
#endif

            OnModelModified();
        }

        public void ConnectBoundToObject(object boundTo)
        {
            _model.ConnectBoundToObject(boundTo);
        }

        public void DisconnectBoundToObject()
        {
            _model.DisconnectBoundToObject();
        }

        public void AddView(BinderViewBase view)
        {
            view.Controller = this;
            _views.Add(view);

            OnModelModified();
        }

        internal List<BinderViewBase> Views
        {
            get { return _views; }
        }

        internal bool TracingEnabled
        {
            get { return _tracingEnabled; }
            set
            {
                _tracingEnabled = value;
            }
        }
    }
}
