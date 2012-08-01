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
using System.Reflection;

#endregion

namespace Interlace.Binding.Views
{
    public abstract class BinderViewBase
    {
        private BinderController _controller;
        private ViewConverterBase _converter;

        private bool _errorOccurred;
        private Exception _errorException;

        bool _viewToModelDisabled = false;
        bool _modelToViewDisabled = false;

        public BinderViewBase()
        {
            _converter = ViewConverterBase.Null;
        }

        public bool ViewToModelDisabled
        { 	 
            get { return _viewToModelDisabled; }
            set { _viewToModelDisabled = value; }
        }

        public bool ModelToViewDisabled
        { 	 
            get { return _modelToViewDisabled; }
            set { _modelToViewDisabled = value; }
        }

        public virtual int OrderingIndex
        {
            get { return int.MaxValue; }
        }

        public ViewConverterBase Converter
        {
            get { return _converter; }
            set 
            { 
                if (value == null) 
                    throw new ArgumentException("A non-null converter must be supplied.", "value");

                _converter = value; 
            }
        }

        protected internal BinderController Controller
        {
            set { _controller = value; }
        }

        protected internal void SetValue(object value)
        {
            if (!_modelToViewDisabled)
            {
                object convertedValue;

                if (value == BinderNotBound.Value)
                {
                    convertedValue = _converter.BinderNotBoundValue;
                }
                else if (value == BinderMissingProperty.Value)
                {
                    convertedValue = _converter.BinderMissingValue;
                }
                else
                {
                    convertedValue = _converter.ModelToView(value);
                }

                ClearError();

                OnModelChanged(convertedValue);
            }
        }

        protected abstract void OnModelChanged(object value);

        protected void ChangeModel(object value)
        {
            object valueForModel;

            try
            {
                if (_controller != null && !_viewToModelDisabled)
                {
                    valueForModel = _converter.ViewToModel(value);

                    _controller.OnViewModified(this, valueForModel);
                }
            }
            catch (TargetInvocationException ex)
            {
                SetError(ex.InnerException);

                return;
            }
            catch (FormatException ex)
            {
                SetError(ex);

                return;
            }
        }

        void SetError(Exception ex)
        {
            _errorOccurred = true;
            _errorException = ex;

            ShowError(ex.Message);
        }

        void ClearError()
        {
            _errorOccurred = false;
            _errorException = null;

            HideError();
        }

        protected internal virtual void ShowError(string message)
        {
        }

        protected internal virtual void HideError()
        {
        }

        protected internal virtual void HighlightError()
        {
        }

        internal BinderViewError FindFirstError()
        {
            if (_errorOccurred)
            {
                return new BinderViewError(_errorException.Message, this);
            }
            else
            {
                return null;
            }
        }
    }
}
