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
using System.Configuration;
using System.Data;

using DevExpress.Data;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

#endregion

namespace Interlace.Utilities
{
    /// <summary>
    /// Provides properties that give type safe access to the current 
    /// selection of a grid view.
    /// </summary>
    /// <remarks>
    /// The properties of this object can be bound to, and will raise events
    /// when they change. For example, a button that acts on the selection
    /// of a grid can have its Enabled property bound to HasAnySelection or
    /// HasSingleSelection on this object.
    /// </remarks>
    /// <typeparam name="TRow">The type of object contained in the grid.</typeparam>
    public class TypedGridSelection<TRow> : INotifyPropertyChanged where TRow : class
    {
        ColumnView _view;

        int _previousSelectionCount;

        bool _forbidGroupRowSelection;
        bool _lockRowFocusing;

        // TODO: Remove the hack, onnnneeeee day.
        bool _enablePostNewRowHack = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedGridSelection&lt;TRow&gt;"/> class.
        /// </summary>
        /// <param name="view">The grid view that the selection of is to be tracked.</param>
        public TypedGridSelection(ColumnView view)
        {
            _view = view;
            _view.SelectionChanged += new SelectionChangedEventHandler(_view_SelectionChanged);

            // When a grid is not in multi-select mode, the selection changed events aren't fired.
            // (See Developer Express Knowledge Base Article A1882)
            // As a work-around, we watch the focus events too:
            _view.FocusedRowChanged += new FocusedRowChangedEventHandler(_view_FocusedRowChanged);
            _view.GridControl.DataSourceChanged += new EventHandler(_view_DataSourceChanged);
            _view.ColumnFilterChanged += new EventHandler(_view_ColumnFilterChanged);

            _previousSelectionCount = _view.SelectedRowsCount;

            _forbidGroupRowSelection = false;
            _lockRowFocusing = false;
        }

        void _view_ColumnFilterChanged(object sender, EventArgs e)
        {
            HandleRowFocusOrSelectionChange();
        }

        public bool EnablePostNewRowHack
        { 	 
           get { return _enablePostNewRowHack; }
           set { _enablePostNewRowHack = value; }
        }

        void _view_DataSourceChanged(object sender, EventArgs e)
        {
            if (PropertyChanged != null)
			{
                PropertyChanged(this, new PropertyChangedEventArgs("HasDataSource"));
			}

        	HandleRowFocusOrSelectionChange();
        }

        void HandleRowFocusOrSelectionChange()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("SingleSelection"));

                if (Math.Sign(_previousSelectionCount) != Math.Sign(_view.SelectedRowsCount))
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("HasAnySelection"));
                }

                if ((_previousSelectionCount == 1) != (_view.SelectedRowsCount == 1))
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("HasSingleSelection"));
                }
            }

            _previousSelectionCount = _view.SelectedRowsCount;
        }

        void _view_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (_forbidGroupRowSelection)
                CancelSelectionOfGroupRows(_view, e);

            if (!_lockRowFocusing)
                HandleRowFocusOrSelectionChange();
        }

        void _view_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            HandleRowFocusOrSelectionChange();
        }

        /// <summary>
        /// Occurs when one of the properties in this class change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets a value indicating whether the grid view has any selection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the grid view has any selection; otherwise, <c>false</c>.
        /// </value>
        public bool HasAnySelection
        {
            get
            {
                return _view.SelectedRowsCount > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the grid view has a single selection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the grid view has single selection; otherwise, <c>false</c>.
        /// </value>
        public bool HasSingleSelection
        {
            get
            {
                return _view.SelectedRowsCount == 1;
            }
        }

        /// <summary>
        /// Indicates if the grid is attached to a non-null datasource.  Useful for determining
        /// the difference between a null datasource and an empty one.
        /// </summary>
        /// <value>True if there is a valid datasource, else false if datasource is null.</value>
        public bool HasDataSource
        {
            get { return _view.GridControl.DataSource != null; }
        }

        /// <summary>
        /// Gets the single selection. 
        /// </summary>
        /// <value>The single selection if there is only one object
        /// selected, otherwise <c>null</c>.</value>
        public TRow SingleSelection
        {
            get
            {
                PostNewRows();
                
                int[] selectedRowHandles = _view.GetSelectedRows();

                if (selectedRowHandles.Length != 1) return null;
                
                return _view.GetRow(selectedRowHandles[0]) as TRow;
            }
        }

        /// <summary>
        /// When DevExpress grids create a row, it's not a 'real' row until there's either data
        /// in it, or it gets specifically asked to create the row.  If you call GetRow with the
        /// virtual row's handle (-999998), then it throws up because the row doesn't really exist.
        /// This call forces the grid to create the row for real.
        /// </summary>
        private void PostNewRows()
        {
            if (_enablePostNewRowHack) _view.UpdateCurrentRow();
        }

        /// <summary>
        /// Determines if the single selected row is visible.
        /// </summary>
        /// <value>True if the single selected row is visible.  False if
        /// there are multiple selected rows, or the single selected row
        /// is not visible.  (In other words if it's scrolled out of view.)</value>
        public bool SingleSelectionVisible
        {
            get
            {
                int[] selectedRowHandles = _view.GetSelectedRows();

                if (selectedRowHandles.Length != 1) return false;

                return _view.GetVisibleIndex(selectedRowHandles[0]) >= 0;
            }
        }
        

        /// <summary>
        /// Gets a list of selected items.  If there is no selection, an empty
        /// list is returned.
        /// </summary>
        /// <value>A list of selected items.  If there is no selection, an empty
        /// list is returned.</value>
        public List<TRow> MultipleSelection
        {
            get
            {
                List<TRow> returnValue = new List<TRow>();

                PostNewRows();

                int[] selectedRowHandles = _view.GetSelectedRows();

                foreach (int selectedRowHandle in selectedRowHandles)
                {
                    returnValue.Add(_view.GetRow(selectedRowHandle) as TRow);
                }

                return returnValue;
            }
        }

        /// <summary>
        /// Returns the number of selected items in the grid.  Mainly here for convenience.
        /// </summary>
        /// <value>An int representing the number of currently selected items in the grid.</value>
        public int SelectionCount
        {
            get { return _view.GetSelectedRows().Length; }
        }

        /// <summary>
        /// When set to true, ensures that the user can't select group rows in the grid.
        /// </summary>
        public bool ForbidGroupRowSelection
        {
            get { return _forbidGroupRowSelection; }
            set
            {
                _forbidGroupRowSelection = value;

                if (value)
                {
                    // Make sure we aren't already on a group row.
                    if (_view is GridView)
                    {
                        GridView gridView = _view as GridView;

                        if (gridView.IsGroupRow(gridView.FocusedRowHandle))
                            gridView.MoveNext();
                    }
                }
            }
        }

        private void CancelSelectionOfGroupRows(ColumnView columnView, FocusedRowChangedEventArgs e)
        {
            if (_lockRowFocusing) return;

            GridView gridView;

            if (columnView is GridView)
                gridView = columnView as GridView;
            else
                return;

            try
            {
                _lockRowFocusing = true;

                if (gridView.IsGroupRow(e.FocusedRowHandle) & !gridView.IsGroupRow(e.PrevFocusedRowHandle))
                {
                    if (gridView.GetParentRowHandle(e.PrevFocusedRowHandle) == e.FocusedRowHandle)
                    {
                        if (gridView.GetVisibleIndex(e.FocusedRowHandle) > 0)
                        {
                            int rowHandle = gridView.GetVisibleRowHandle(
                                gridView.GetVisibleIndex(e.FocusedRowHandle) - 1);

                            if (gridView.IsGroupRow(rowHandle))
                            {
                                rowHandle = gridView.GetChildRowHandle(rowHandle,
                                    gridView.GetChildRowCount(rowHandle) - 1);
                            }

                            gridView.FocusedRowHandle = rowHandle;
                        }
                        else
                        {
                            gridView.FocusedRowHandle = e.PrevFocusedRowHandle;
                        }
                    }
                    else
                    {
                        if (gridView.IsGroupRow(e.FocusedRowHandle))
                            gridView.ExpandGroupRow(e.FocusedRowHandle, false);

                        gridView.MoveNext();
                    }
                }
            }
            finally
            {
                _lockRowFocusing = false;
            }
        }
    }
}
