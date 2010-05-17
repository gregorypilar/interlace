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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;

#endregion

namespace Interlace.NamedObjects
{
    public partial class NamedObjectSuggestionsForm : DevExpress.XtraEditors.XtraForm
    {
        NamedObjectCollectionController _controller;
        NamedObjectRange _currentRange;

        static string _prompt = "The following {0} was not found or is ambiguous:";

        bool _runningNonModally = false;

        public NamedObjectSuggestionsForm()
        {
            _controller = null;

            InitializeComponent();
        }

        public void AttachController(NamedObjectCollectionController controller)
        {
            _controller = controller;

            SetControlsEnabled(true);

            _textLabel.Text = string.Format(_prompt, _controller.Source.ObjectDescription);

            _controller.Source.InitializeGridView(_suggestionsView);
        }

        NamedObjectRange GetNextUnvaluedRange()
        {
            foreach (NamedObjectRange range in _controller.Ranges)
            {
                if (range.Value == null) return range;
            }

            return null;
        }

        void FillSuggestions()
        {
            // Fill the grid:
            ICollection suggestionCollection = 
                _controller.Source.GetSuggestionsFor(_currentRange);

            _suggestionsGrid.DataSource = suggestionCollection;

            // Select the first one:
            if (suggestionCollection.Count >= 1) 
            {
                _suggestionsView.SelectRow(_suggestionsView.GetRowHandle(0));
            }

            _change.Enabled = suggestionCollection.Count != 0;
        }

        void MoveToNextUnvaluedRange()
        {
            _currentRange = GetNextUnvaluedRange();

            if (_currentRange != null)
            {
                _controller.SelectRange(_currentRange);

                _text.Text = _currentRange.Text;
                _text.Select(_text.Text.Length, 0);

                if ((_suggestionsGrid.DataSource as ICollection).Count == 0)
                {
                    _text.Select();
                }
                else
                {
                    _suggestionsGrid.Select();
                }
            }
            else
            {
                BeginInvoke((MethodInvoker)Close);
            }
        }

        private void _suggestionsView_DoubleClick(object sender, EventArgs e)
        {
            _change_Click(_change, EventArgs.Empty);
        }

        private void _change_Click(object sender, EventArgs e)
        {
            if (_suggestionsView.FocusedRowHandle == GridControl.InvalidRowHandle) return;

            object suggestion = _suggestionsView.GetRow(_suggestionsView.FocusedRowHandle);

            _currentRange.Value = _controller.Source.GetRangeValueFromSuggestion(suggestion);
            _currentRange.Text = _controller.Source.GetCanonicalTextFromValue(_currentRange.Value);

            _controller.UpdateFromRanges();

            MoveToNextUnvaluedRange();
        }

        private void _cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void _text_EditValueChanged(object sender, EventArgs e)
        {
            if (_currentRange != null)
            {
                _currentRange.Text = _text.Text.Trim();

                FillSuggestions();
            }
        }

        private void _suggestionsView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _change_Click(_change, EventArgs.Empty);
            }
        }

        private void _text_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BeginInvoke((MethodInvoker)KeyDownOnEditor);
            }
        }

        void KeyDownOnEditor()
        {
            ICollection suggestions = _suggestionsGrid.DataSource as ICollection;

            if (suggestions.Count == 0)
            {
            }
            else
            {
                _suggestionsGrid.Select();
            }
        }

        void SetControlsEnabled(bool enabled)
        {
            _textLabel.Enabled = enabled;
            _text.Enabled = enabled;
            _suggestionsLabel.Enabled = enabled;
            _suggestionsGrid.Enabled = enabled;
            _cancel.Enabled = enabled;
            _change.Enabled = enabled;
        }

        private void NamedObjectSuggestionsForm_Deactivate(object sender, EventArgs e)
        {
            if (_runningNonModally)
            {
                _text.Text = "";
                _suggestionsGrid.DataSource = null;

                SetControlsEnabled(false);
            }
        }

        private void NamedObjectSuggestionsForm_Load(object sender, EventArgs e)
        {
            MoveToNextUnvaluedRange();
        }
    }
}
