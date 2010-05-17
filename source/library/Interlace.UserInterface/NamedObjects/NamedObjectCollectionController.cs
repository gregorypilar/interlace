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
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using DevExpress.XtraEditors;

#endregion

namespace Interlace.NamedObjects
{
    public class NamedObjectCollectionController
    {
        TextEdit _editor;
        Form _parentForm;
        NamedObjectRangeSet _ranges = new NamedObjectRangeSet();
        INamedObjectSource _source;

        /// <summary>
        /// Keep a single form (value) for each parent form (key).
        /// </summary>
        static Dictionary<Form, NamedObjectSuggestionsForm> _checkNamesForms = 
            new Dictionary<Form, NamedObjectSuggestionsForm>();

        public NamedObjectCollectionController(TextEdit editor, Form parentForm, INamedObjectSource source)
        {
            _editor = editor;
            _parentForm = parentForm;
            _source = source;

            _parentForm.FormClosing += new FormClosingEventHandler(_parentForm_FormClosing);

            _editor.LostFocus += new EventHandler(_editor_LostFocus);
        }

        void _parentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _editor.LostFocus -= new EventHandler(_editor_LostFocus);

            if (_checkNamesForms.ContainsKey(_parentForm))
            {
                _checkNamesForms[_parentForm].Close();
            }

            _parentForm.FormClosing -= new FormClosingEventHandler(_parentForm_FormClosing);
        }

        internal INamedObjectSource Source
        {
            get { return _source; }
        }

        internal NamedObjectRangeSet Ranges
        {
            get { return _ranges; }
        }

        void FindValuesForUnambiguousRanges()
        {
            foreach (NamedObjectRange range in _ranges)
            {
                ICollection suggestions = _source.GetSuggestionsFor(range);

                if (suggestions.Count == 1)
                {
                    IEnumerator enumerator = suggestions.GetEnumerator();
                    enumerator.MoveNext();
                    object suggestion = enumerator.Current;

                    range.Value = _source.GetRangeValueFromSuggestion(suggestion);
                    range.Text = _source.GetCanonicalTextFromValue(range.Value);
                }
            }
        }

        bool _checkingNames = false;

        public bool CheckNamesModal()
        {
            _checkingNames = true;

            try
            {
                _ranges.ReplaceWith(_editor.EditValue as string ?? "");

                FindValuesForUnambiguousRanges();

                UpdateFromRanges();

                if (_ranges.HasUnvaluedRange)
                {
                    _editor.Properties.HideSelection = false;

                    NamedObjectSuggestionsForm form = new NamedObjectSuggestionsForm();

                    form.AttachController(this);

                    Rectangle editorScreenBounds = _editor.Parent.RectangleToScreen(_editor.Bounds);
                    Point formLocation = new Point(editorScreenBounds.Left + 40, editorScreenBounds.Bottom + 10);

                    form.Location = formLocation;

                    form.ShowDialog(_parentForm);
                }

                return !_ranges.HasUnvaluedRange;
            }
            finally
            {
                _checkingNames = false;
            }
        }

        public bool ValuesAvailable
        {
            get 
            { 
                _ranges.ReplaceWith(_editor.EditValue as string ?? "");
                FindValuesForUnambiguousRanges();

                return !_ranges.HasUnvaluedRange; 
            }
        }

        public void SetValues<T>(ICollection<T> collection)
        {
            List<string> components = new List<string>();

            foreach (T value in collection)
            {
                components.Add(_source.GetCanonicalTextFromValue(value));
            }

            _editor.EditValue = string.Join("; ", components.ToArray());
        }

        public void GetValues<T>(ICollection<T> collection) where T : class
        {
            _ranges.ReplaceWith(_editor.EditValue as string ?? "");
            FindValuesForUnambiguousRanges();

            collection.Clear();

            foreach (NamedObjectRange range in _ranges)
            {
                T value = range.Value as T;

                if (value != null) collection.Add(value);
            }
        }

        void _editor_LostFocus(object sender, EventArgs e)
        {
            if (!_checkingNames)
            {
                _ranges.ReplaceWith(_editor.EditValue as string ?? "");

                FindValuesForUnambiguousRanges();

                UpdateFromRanges();
            }
        }

        void form_FormClosed(object sender, FormClosedEventArgs e)
        {
            NamedObjectSuggestionsForm form = sender as NamedObjectSuggestionsForm;

            if (form == null) return;

            form.FormClosed -= new FormClosedEventHandler(form_FormClosed);

            _editor.Select(_editor.Text.Length, 0);

            _checkNamesForms.Remove(_parentForm);
        }

        internal void SelectRange(NamedObjectRange range)
        {
            int offset, length;

            _ranges.GetSelectionFor(range, out offset, out length);
            _editor.Select(offset, length);
        }

        internal void UpdateFromRanges()
        {
            _editor.Text = _ranges.Text;
            _editor.Select(_editor.Text.Length, 0);
        }
    }
}
