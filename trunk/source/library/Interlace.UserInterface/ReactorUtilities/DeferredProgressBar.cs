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
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraEditors;

#endregion

namespace Interlace.ReactorUtilities
{
    public enum OperationType
    {
        StandardProgressBar,
        MarqueeProgressBar
    }

    public partial class DeferredProgressBar : DevExpress.XtraEditors.XtraForm, IProgressIndicator
    {
        OperationType _operationType;

        DeferredObject _deferred;

        DeferredFailure _failureObject;
        object _successObject;

        IMonitoredOperation _monitoredOperation;

        public DeferredProgressBar()
        {
            InitializeComponent();
        }

        public static Deferred<object> ChainIndefinite(DeferredObject deferred, string statusMessage)
        {
            using (DeferredProgressBar form = new DeferredProgressBar())
            {
                form.OperationType = OperationType.MarqueeProgressBar;
                form.StatusMessage = statusMessage;
                form.DeferredObject = deferred;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return Deferred.Success(form.SuccessObject);
                }
                else
                {
                    return Deferred.Failure<object>(form.FailureObject);
                }
            }
        }

        public static object ShowIndefinite(DeferredObject deferred, string statusMessage)
        {
            return ShowIndefinite(deferred, statusMessage, null);
        }

        public static object ShowIndefinite(DeferredObject deferred, string statusMessage, string successMessage)
        {
            return ShowIndefinite(deferred, statusMessage, successMessage, RemoteExceptionForm.ShowFailureModal);
        }

        public static object ShowIndefinite(DeferredObject deferred, string statusMessage, string successMessage, Interlace.ReactorUtilities.VoidDeferred.Failback failback)
        {
            using (DeferredProgressBar form = new DeferredProgressBar())
            {
                form.OperationType = OperationType.MarqueeProgressBar;
                form.StatusMessage = statusMessage;
                form.DeferredObject = deferred;
                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(successMessage)) MessageBox.Show(successMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return form.SuccessObject;
                }
                else
                {
                    failback(form.FailureObject);
                    return form.FailureObject;
                }
            }
        }

        public static object ShowFinite(IMonitoredOperation monitoredOperation)
        {
            return ShowFinite(monitoredOperation, "");
        }

        public static object ShowFinite(IMonitoredOperation monitoredOperation, string successMessage)
        {
            using (DeferredProgressBar form = new DeferredProgressBar())
            {
                form.OperationType = OperationType.StandardProgressBar;
                form.MonitoredOperation = monitoredOperation;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    if (!string.IsNullOrEmpty(successMessage)) MessageBox.Show(successMessage, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return form.SuccessObject;
                }
                else
                {
                    RemoteExceptionForm.ShowFailureModal(form.FailureObject);
                    return form.FailureObject;
                }
            }
        }

        public IMonitoredOperation MonitoredOperation
        {
            get { return _monitoredOperation; }
            set
            {
                _monitoredOperation = value;
                _monitoredOperation.ProgressIndicator = this;
            }
        }

        public DeferredObject DeferredObject
        {
            get { return _deferred; }
            set
            {
                _deferred = value;

                _deferred.ObjectCompletion(delegate(object successObject)
                {
                    _successObject = successObject;
                    _failureObject = null;
                    DialogResult = DialogResult.OK;

                    return null;
                },
                delegate(DeferredFailure failure)
                {
                    _successObject = null;
                    _failureObject = failure;
                    DialogResult = DialogResult.Cancel;

                    return null;
                }, null);
            }
        }

        public object SuccessObject
        {
            get { return _successObject; }
        }

        public DeferredFailure FailureObject
        {
            get { return _failureObject; }
        }

        public OperationType OperationType
        {
            get { return _operationType; }
            set
            {
                _operationType = value;

                _marqueeProgressBar.Visible = _operationType == OperationType.MarqueeProgressBar;
                _progressBar.Visible = _operationType == OperationType.StandardProgressBar;
            }
        }

        public int Maximum
        {
            get { return _progressBar.Properties.Maximum; }
            set { _progressBar.Properties.Maximum = value; }
        }

        public int Current
        {
            get { return (int)_progressBar.EditValue; }
            set { _progressBar.EditValue = value; }
        }

        public string StatusMessage
        {
            get { return _statusLabel.Text; }
            set { _statusLabel.Text = value; }
        }

        private void DeferredProgressBar_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        private void DeferredProgressBar_Load(object sender, EventArgs e)
        {
            if (_operationType == OperationType.StandardProgressBar)
            {
                _monitoredOperation.Start();
            }
        }

        #region IProgressIndicator Members

        public void Success()
        {
            Success(null);
        }

        public void Success(object successObject)
        {
            _successObject = successObject;
            _failureObject = null;
            DialogResult = DialogResult.OK;
        }

        public void Failure(DeferredFailure failure)
        {
            _failureObject = failure;
            _successObject = null;
            DialogResult = DialogResult.Cancel;
        }

        public void Cancel()
        {
            _failureObject = null;
            _successObject = null;

            DialogResult = DialogResult.Cancel;
        }

        #endregion

    }
}
