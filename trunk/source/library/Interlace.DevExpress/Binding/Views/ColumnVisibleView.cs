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
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;

#endregion

namespace Interlace.Binding.Views
{
    public class ColumnVisibleView : BinderViewBase
    {
        delegate void EnabledSetDelegate(bool enabled);

        EnabledSetDelegate _setDelegate;
        bool _unboundValue = false;

        public ColumnVisibleView(params GridColumn[] boundColumns)
        {            
            if (boundColumns.Length==0) return;

            ViewToModelDisabled = true;

            //Store initial visible index state
            Dictionary<GridColumn, int> storedVisibleIndices = GetVisibleIndices(boundColumns[0].View);

            // Take a private copy:
            GridColumn[] boundColumnsCopy = new GridColumn[boundColumns.Length];           
            Array.Copy(boundColumns, boundColumnsCopy, boundColumnsCopy.Length);
            

            _setDelegate = delegate(bool visible)
            {
                Dictionary<GridColumn, bool> storedVisibility = GetColumnVisibility(boundColumns[0].View);
                foreach (GridColumn column in boundColumnsCopy)
                {
                    if (column.Visible != visible) column.Visible = visible;
                }

                ResetColumnOrder(boundColumnsCopy[0].View, storedVisibility, storedVisibleIndices);                
            };
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

        private Dictionary<GridColumn, bool> GetColumnVisibility(ColumnView columnView)
        {
            Dictionary<GridColumn, bool> storedVisiblity = new Dictionary<GridColumn, bool>();

            foreach (GridColumn column in columnView.Columns)
            {
                storedVisiblity[column] = column.Visible;
            }

            return storedVisiblity;
        }

        private Dictionary<GridColumn, int> GetVisibleIndices(ColumnView columnView)
        {
            Dictionary<GridColumn, int> storedVisibleIndices = new Dictionary<GridColumn, int>();

            foreach (GridColumn column in columnView.Columns)
            {
                if (column.VisibleIndex > -1)
                    storedVisibleIndices[column] = column.VisibleIndex;
            }

            return storedVisibleIndices;
        }

        private void ResetColumnOrder(ColumnView columnView, Dictionary<GridColumn, bool> storedVisiblity, Dictionary<GridColumn, int> storedVisibleIndices)
        {
            int i = 100;

            foreach (GridColumn column in columnView.Columns)
            {
                if (!storedVisiblity[column]) continue;
                if (storedVisibleIndices.ContainsKey(column))
                {
                    column.Visible = true;
                    column.VisibleIndex = storedVisibleIndices[column];
                }
                else
                {
                    column.VisibleIndex = i++;
                }
            }
        }
    }
}
