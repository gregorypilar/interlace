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
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

#endregion

namespace Interlace.Binding.Views
{
    public class BaseCheckedListBoxControlBinderView : BinderViewBase
    {
        BaseCheckedListBoxControl _boundControl;
        int _itemIndex;

        bool _ignoreChangedEvent;

        public BaseCheckedListBoxControlBinderView(BaseCheckedListBoxControl boundControl, int itemIndex)
        {
            _boundControl = boundControl;
            _itemIndex = itemIndex;

            _boundControl.ItemCheck += new DevExpress.XtraEditors.Controls.ItemCheckEventHandler(_boundControl_ItemCheck);
        }

        void _boundControl_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
        {
            if (_ignoreChangedEvent) return;

            ChangeModel(_boundControl.Items[_itemIndex].CheckState == CheckState.Checked);
        }

        public override int OrderingIndex
        {
            get { return _boundControl.TabIndex; }
        }

        private void SetEditValueAndIgnoreEvents(CheckState value)
        {
            try
            {
                _ignoreChangedEvent = true;

                _boundControl.Items[_itemIndex].CheckState = value;
            }
            finally
            {
                _ignoreChangedEvent = false;
            }
        }

        protected override void OnModelChanged(object value)
        {
            if (value == BinderNotBound.Value || value == BinderMissingProperty.Value)
            {
                SetEditValueAndIgnoreEvents(CheckState.Indeterminate);
                _boundControl.Items[_itemIndex].Enabled = false;
            }
            else
            {
                SetEditValueAndIgnoreEvents((bool)value ? CheckState.Checked : CheckState.Unchecked);
                _boundControl.Items[_itemIndex].Enabled = true;
            }
        }

        protected override void HighlightError()
        {
        }
    }
}
