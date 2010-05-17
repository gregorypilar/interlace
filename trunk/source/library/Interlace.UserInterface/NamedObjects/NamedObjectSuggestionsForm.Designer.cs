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

#endregion

namespace Interlace.NamedObjects
{
    partial class NamedObjectSuggestionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._textLabel = new DevExpress.XtraEditors.LabelControl();
            this._text = new DevExpress.XtraEditors.TextEdit();
            this._suggestionsLabel = new DevExpress.XtraEditors.LabelControl();
            this._cancel = new DevExpress.XtraEditors.SimpleButton();
            this._change = new DevExpress.XtraEditors.SimpleButton();
            this._suggestionsGrid = new DevExpress.XtraGrid.GridControl();
            this._suggestionsView = new DevExpress.XtraGrid.Views.Grid.GridView();
            this._checkNames = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this._text.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._suggestionsGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._suggestionsView)).BeginInit();
            this.SuspendLayout();
            // 
            // _textLabel
            // 
            this._textLabel.Location = new System.Drawing.Point(12, 12);
            this._textLabel.Name = "_textLabel";
            this._textLabel.Size = new System.Drawing.Size(249, 13);
            this._textLabel.TabIndex = 0;
            this._textLabel.Text = "The following object was not found or is ambiguous:";
            // 
            // _text
            // 
            this._text.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._text.Location = new System.Drawing.Point(12, 31);
            this._text.MinimumSize = new System.Drawing.Size(372, 20);
            this._text.Name = "_text";
            this._text.Size = new System.Drawing.Size(372, 20);
            this._text.TabIndex = 1;
            this._text.EditValueChanged += new System.EventHandler(this._text_EditValueChanged);
            this._text.KeyDown += new System.Windows.Forms.KeyEventHandler(this._text_KeyDown);
            // 
            // _suggestionsLabel
            // 
            this._suggestionsLabel.Location = new System.Drawing.Point(12, 57);
            this._suggestionsLabel.Name = "_suggestionsLabel";
            this._suggestionsLabel.Size = new System.Drawing.Size(62, 13);
            this._suggestionsLabel.TabIndex = 2;
            this._suggestionsLabel.Text = "Suggestions:";
            // 
            // _cancel
            // 
            this._cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancel.Location = new System.Drawing.Point(12, 248);
            this._cancel.Name = "_cancel";
            this._cancel.Size = new System.Drawing.Size(85, 23);
            this._cancel.TabIndex = 3;
            this._cancel.Text = "&Cancel";
            this._cancel.Click += new System.EventHandler(this._cancel_Click);
            // 
            // _change
            // 
            this._change.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._change.Location = new System.Drawing.Point(299, 248);
            this._change.Name = "_change";
            this._change.Size = new System.Drawing.Size(85, 23);
            this._change.TabIndex = 4;
            this._change.Text = "&Change";
            this._change.Click += new System.EventHandler(this._change_Click);
            // 
            // _suggestionsGrid
            // 
            this._suggestionsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this._suggestionsGrid.EmbeddedNavigator.Name = "";
            this._suggestionsGrid.Location = new System.Drawing.Point(12, 76);
            this._suggestionsGrid.MainView = this._suggestionsView;
            this._suggestionsGrid.Name = "_suggestionsGrid";
            this._suggestionsGrid.Size = new System.Drawing.Size(372, 166);
            this._suggestionsGrid.TabIndex = 2;
            this._suggestionsGrid.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this._suggestionsView});
            // 
            // _suggestionsView
            // 
            this._suggestionsView.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.None;
            this._suggestionsView.GridControl = this._suggestionsGrid;
            this._suggestionsView.Name = "_suggestionsView";
            this._suggestionsView.OptionsBehavior.Editable = false;
            this._suggestionsView.OptionsCustomization.AllowColumnMoving = false;
            this._suggestionsView.OptionsCustomization.AllowColumnResizing = false;
            this._suggestionsView.OptionsCustomization.AllowFilter = false;
            this._suggestionsView.OptionsCustomization.AllowGroup = false;
            this._suggestionsView.OptionsCustomization.AllowSort = false;
            this._suggestionsView.OptionsDetail.EnableMasterViewMode = false;
            this._suggestionsView.OptionsSelection.EnableAppearanceFocusedCell = false;
            this._suggestionsView.OptionsView.ShowColumnHeaders = false;
            this._suggestionsView.OptionsView.ShowGroupPanel = false;
            this._suggestionsView.OptionsView.ShowHorzLines = false;
            this._suggestionsView.OptionsView.ShowIndicator = false;
            this._suggestionsView.OptionsView.ShowPreviewLines = false;
            this._suggestionsView.OptionsView.ShowVertLines = false;
            this._suggestionsView.KeyDown += new System.Windows.Forms.KeyEventHandler(this._suggestionsView_KeyDown);
            this._suggestionsView.DoubleClick += new System.EventHandler(this._suggestionsView_DoubleClick);
            // 
            // _checkNames
            // 
            this._checkNames.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._checkNames.Location = new System.Drawing.Point(282, 248);
            this._checkNames.Name = "_checkNames";
            this._checkNames.Size = new System.Drawing.Size(102, 23);
            this._checkNames.TabIndex = 5;
            this._checkNames.Text = "Check Names";
            this._checkNames.Visible = false;
            // 
            // NamedObjectSuggestionsForm
            // 
            this.CancelButton = this._cancel;
            this.ClientSize = new System.Drawing.Size(396, 283);
            this.Controls.Add(this._checkNames);
            this.Controls.Add(this._suggestionsGrid);
            this.Controls.Add(this._change);
            this.Controls.Add(this._cancel);
            this.Controls.Add(this._suggestionsLabel);
            this.Controls.Add(this._text);
            this.Controls.Add(this._textLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "NamedObjectSuggestionsForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Check Names";
            this.Deactivate += new System.EventHandler(this.NamedObjectSuggestionsForm_Deactivate);
            this.Load += new System.EventHandler(this.NamedObjectSuggestionsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this._text.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._suggestionsGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._suggestionsView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl _textLabel;
        private DevExpress.XtraEditors.TextEdit _text;
        private DevExpress.XtraEditors.LabelControl _suggestionsLabel;
        private DevExpress.XtraEditors.SimpleButton _cancel;
        private DevExpress.XtraEditors.SimpleButton _change;
        private DevExpress.XtraGrid.GridControl _suggestionsGrid;
        private DevExpress.XtraGrid.Views.Grid.GridView _suggestionsView;
        private DevExpress.XtraEditors.SimpleButton _checkNames;
    }
}
