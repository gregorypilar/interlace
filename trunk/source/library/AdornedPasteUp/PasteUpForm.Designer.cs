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

namespace AdornedPasteUp
{
    partial class PasteUpForm
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
            this.components = new System.ComponentModel.Container();
            this._defaultLookAndFeel = new DevExpress.LookAndFeel.DefaultLookAndFeel(this.components);
            this._barManager = new DevExpress.XtraBars.BarManager(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this._popupMenu = new DevExpress.XtraBars.PopupMenu(this.components);
            this._deleteItem = new DevExpress.XtraBars.BarButtonItem();
            this._bringToFrontItem = new DevExpress.XtraBars.BarButtonItem();
            this._sendToBackItem = new DevExpress.XtraBars.BarButtonItem();
            ((System.ComponentModel.ISupportInitialize)(this._barManager)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._popupMenu)).BeginInit();
            this.SuspendLayout();
            // 
            // _defaultLookAndFeel
            // 
            this._defaultLookAndFeel.LookAndFeel.UseWindowsXPTheme = true;
            // 
            // _barManager
            // 
            this._barManager.DockControls.Add(this.barDockControlTop);
            this._barManager.DockControls.Add(this.barDockControlBottom);
            this._barManager.DockControls.Add(this.barDockControlLeft);
            this._barManager.DockControls.Add(this.barDockControlRight);
            this._barManager.Form = this;
            this._barManager.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this._deleteItem,
            this._bringToFrontItem,
            this._sendToBackItem});
            this._barManager.MaxItemId = 3;
            // 
            // _popupMenu
            // 
            this._popupMenu.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this._deleteItem),
            new DevExpress.XtraBars.LinkPersistInfo(this._bringToFrontItem, true),
            new DevExpress.XtraBars.LinkPersistInfo(this._sendToBackItem)});
            this._popupMenu.Manager = this._barManager;
            this._popupMenu.Name = "_popupMenu";
            // 
            // _deleteItem
            // 
            this._deleteItem.Caption = "Delete";
            this._deleteItem.Id = 0;
            this._deleteItem.Name = "_deleteItem";
            this._deleteItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._deleteItem_ItemClick);
            // 
            // _bringToFrontItem
            // 
            this._bringToFrontItem.Caption = "Bring to Front";
            this._bringToFrontItem.Id = 1;
            this._bringToFrontItem.Name = "_bringToFrontItem";
            this._bringToFrontItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._bringToFrontItem_ItemClick);
            // 
            // _sendToBackItem
            // 
            this._sendToBackItem.Caption = "Send to Back";
            this._sendToBackItem.Id = 2;
            this._sendToBackItem.Name = "_sendToBackItem";
            this._sendToBackItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._sendToBackItem_ItemClick);
            // 
            // PasteUpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.Name = "PasteUpForm";
            this.Text = "PasteUpForm";
            this.Load += new System.EventHandler(this.PasteUpForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this._barManager)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._popupMenu)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.LookAndFeel.DefaultLookAndFeel _defaultLookAndFeel;
        private DevExpress.XtraBars.BarManager _barManager;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.PopupMenu _popupMenu;
        private DevExpress.XtraBars.BarButtonItem _deleteItem;
        private DevExpress.XtraBars.BarButtonItem _bringToFrontItem;
        private DevExpress.XtraBars.BarButtonItem _sendToBackItem;
    }
}
