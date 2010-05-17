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
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this._bars = new DevExpress.XtraBars.BarManager(this.components);
            this.bar1 = new DevExpress.XtraBars.Bar();
            this._openItem = new DevExpress.XtraBars.BarButtonItem();
            this._saveItem = new DevExpress.XtraBars.BarButtonItem();
            this._addScreenShotItem = new DevExpress.XtraBars.BarButtonItem();
            this._addLabelItem = new DevExpress.XtraBars.BarButtonItem();
            this.bar2 = new DevExpress.XtraBars.Bar();
            this.barSubItem1 = new DevExpress.XtraBars.BarSubItem();
            this._newItem = new DevExpress.XtraBars.BarButtonItem();
            this._saveAsItem = new DevExpress.XtraBars.BarButtonItem();
            this._closeItem = new DevExpress.XtraBars.BarButtonItem();
            this._exportImageItem = new DevExpress.XtraBars.BarButtonItem();
            this._exitItem = new DevExpress.XtraBars.BarButtonItem();
            this.bar3 = new DevExpress.XtraBars.Bar();
            this._barAndDockingController = new DevExpress.XtraBars.BarAndDockingController(this.components);
            this.barDockControlTop = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlBottom = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlLeft = new DevExpress.XtraBars.BarDockControl();
            this.barDockControlRight = new DevExpress.XtraBars.BarDockControl();
            this._dockManager = new DevExpress.XtraBars.Docking.DockManager(this.components);
            this._propertiesDockPanel = new DevExpress.XtraBars.Docking.DockPanel();
            this.dockPanel1_Container = new DevExpress.XtraBars.Docking.ControlContainer();
            this._labelPropertiesPanel = new DevExpress.XtraEditors.PanelControl();
            this._labelCaptionLabel = new DevExpress.XtraEditors.LabelControl();
            this._labelLabelLabel = new DevExpress.XtraEditors.LabelControl();
            this._labelCaption = new DevExpress.XtraEditors.MemoEdit();
            this._labelLabel = new DevExpress.XtraEditors.MemoEdit();
            this._noPropertiesPanel = new DevExpress.XtraEditors.PanelControl();
            this._imageCollection = new DevExpress.Utils.ImageCollection(this.components);
            this.barButtonItem2 = new DevExpress.XtraBars.BarButtonItem();
            this._tabs = new DevExpress.XtraTabbedMdi.XtraTabbedMdiManager(this.components);
            this._tabsController = new DevExpress.XtraBars.BarAndDockingController(this.components);
            this._rectangularFramePropertiesPanel = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)(this._bars)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._barAndDockingController)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._dockManager)).BeginInit();
            this._propertiesDockPanel.SuspendLayout();
            this.dockPanel1_Container.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._labelPropertiesPanel)).BeginInit();
            this._labelPropertiesPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._labelCaption.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._labelLabel.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._noPropertiesPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._imageCollection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tabs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._tabsController)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._rectangularFramePropertiesPanel)).BeginInit();
            this.SuspendLayout();
            // 
            // _bars
            // 
            this._bars.Bars.AddRange(new DevExpress.XtraBars.Bar[] {
            this.bar1,
            this.bar2,
            this.bar3});
            this._bars.Controller = this._barAndDockingController;
            this._bars.DockControls.Add(this.barDockControlTop);
            this._bars.DockControls.Add(this.barDockControlBottom);
            this._bars.DockControls.Add(this.barDockControlLeft);
            this._bars.DockControls.Add(this.barDockControlRight);
            this._bars.DockManager = this._dockManager;
            this._bars.Form = this;
            this._bars.Images = this._imageCollection;
            this._bars.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this._addScreenShotItem,
            this.barButtonItem2,
            this.barSubItem1,
            this._addLabelItem,
            this._saveAsItem,
            this._openItem,
            this._exportImageItem,
            this._saveItem,
            this._newItem,
            this._closeItem,
            this._exitItem});
            this._bars.MainMenu = this.bar2;
            this._bars.MaxItemId = 11;
            this._bars.StatusBar = this.bar3;
            // 
            // bar1
            // 
            this.bar1.BarName = "Tools";
            this.bar1.DockCol = 0;
            this.bar1.DockRow = 1;
            this.bar1.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this._openItem),
            new DevExpress.XtraBars.LinkPersistInfo(this._saveItem),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this._addScreenShotItem, "", true, true, true, 0, null, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph),
            new DevExpress.XtraBars.LinkPersistInfo(DevExpress.XtraBars.BarLinkUserDefines.PaintStyle, this._addLabelItem, DevExpress.XtraBars.BarItemPaintStyle.CaptionGlyph)});
            this.bar1.Text = "Tools";
            // 
            // _openItem
            // 
            this._openItem.Caption = "&Open...";
            this._openItem.Id = 5;
            this._openItem.ImageIndex = 0;
            this._openItem.Name = "_openItem";
            this._openItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._openItem_ItemClick);
            // 
            // _saveItem
            // 
            this._saveItem.Caption = "&Save";
            this._saveItem.Id = 7;
            this._saveItem.ImageIndex = 1;
            this._saveItem.Name = "_saveItem";
            this._saveItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._saveItem_ItemClick);
            // 
            // _addScreenShotItem
            // 
            this._addScreenShotItem.Caption = "Add Screen Shot";
            this._addScreenShotItem.Id = 0;
            this._addScreenShotItem.ImageIndex = 4;
            this._addScreenShotItem.Name = "_addScreenShotItem";
            this._addScreenShotItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem1_ItemClick);
            // 
            // _addLabelItem
            // 
            this._addLabelItem.Caption = "Add Label";
            this._addLabelItem.Id = 3;
            this._addLabelItem.ImageIndex = 2;
            this._addLabelItem.Name = "_addLabelItem";
            this._addLabelItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._addLabelItem_ItemClick);
            // 
            // bar2
            // 
            this.bar2.BarName = "Main menu";
            this.bar2.DockCol = 0;
            this.bar2.DockRow = 0;
            this.bar2.DockStyle = DevExpress.XtraBars.BarDockStyle.Top;
            this.bar2.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this.barSubItem1)});
            this.bar2.OptionsBar.AllowQuickCustomization = false;
            this.bar2.OptionsBar.DrawDragBorder = false;
            this.bar2.OptionsBar.MultiLine = true;
            this.bar2.OptionsBar.UseWholeRow = true;
            this.bar2.Text = "Main menu";
            // 
            // barSubItem1
            // 
            this.barSubItem1.Caption = "&File";
            this.barSubItem1.Id = 2;
            this.barSubItem1.LinksPersistInfo.AddRange(new DevExpress.XtraBars.LinkPersistInfo[] {
            new DevExpress.XtraBars.LinkPersistInfo(this._newItem),
            new DevExpress.XtraBars.LinkPersistInfo(this._openItem),
            new DevExpress.XtraBars.LinkPersistInfo(this._saveItem),
            new DevExpress.XtraBars.LinkPersistInfo(this._saveAsItem),
            new DevExpress.XtraBars.LinkPersistInfo(this._closeItem, true),
            new DevExpress.XtraBars.LinkPersistInfo(this._exportImageItem, true),
            new DevExpress.XtraBars.LinkPersistInfo(this._exitItem, true)});
            this.barSubItem1.Name = "barSubItem1";
            // 
            // _newItem
            // 
            this._newItem.Caption = "&New";
            this._newItem.Id = 8;
            this._newItem.Name = "_newItem";
            this._newItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._newItem_ItemClick);
            // 
            // _saveAsItem
            // 
            this._saveAsItem.Caption = "Save &As...";
            this._saveAsItem.Id = 4;
            this._saveAsItem.Name = "_saveAsItem";
            this._saveAsItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._saveAsItem_ItemClick);
            // 
            // _closeItem
            // 
            this._closeItem.Caption = "&Close";
            this._closeItem.Id = 9;
            this._closeItem.Name = "_closeItem";
            this._closeItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._closeItem_ItemClick);
            // 
            // _exportImageItem
            // 
            this._exportImageItem.Caption = "&Export Image...";
            this._exportImageItem.Id = 6;
            this._exportImageItem.Name = "_exportImageItem";
            this._exportImageItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.barButtonItem3_ItemClick);
            // 
            // _exitItem
            // 
            this._exitItem.Caption = "E&xit";
            this._exitItem.Id = 10;
            this._exitItem.Name = "_exitItem";
            this._exitItem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this._exitItem_ItemClick);
            // 
            // bar3
            // 
            this.bar3.BarName = "Status bar";
            this.bar3.CanDockStyle = DevExpress.XtraBars.BarCanDockStyle.Bottom;
            this.bar3.DockCol = 0;
            this.bar3.DockRow = 0;
            this.bar3.DockStyle = DevExpress.XtraBars.BarDockStyle.Bottom;
            this.bar3.OptionsBar.AllowQuickCustomization = false;
            this.bar3.OptionsBar.DrawDragBorder = false;
            this.bar3.OptionsBar.UseWholeRow = true;
            this.bar3.Text = "Status bar";
            // 
            // _barAndDockingController
            // 
            this._barAndDockingController.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
            this._barAndDockingController.LookAndFeel.UseDefaultLookAndFeel = false;
            // 
            // _dockManager
            // 
            this._dockManager.Controller = this._barAndDockingController;
            this._dockManager.Form = this;
            this._dockManager.RootPanels.AddRange(new DevExpress.XtraBars.Docking.DockPanel[] {
            this._propertiesDockPanel});
            this._dockManager.TopZIndexControls.AddRange(new string[] {
            "DevExpress.XtraBars.BarDockControl",
            "System.Windows.Forms.StatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonStatusBar",
            "DevExpress.XtraBars.Ribbon.RibbonControl"});
            // 
            // _propertiesDockPanel
            // 
            this._propertiesDockPanel.Controls.Add(this.dockPanel1_Container);
            this._propertiesDockPanel.Dock = DevExpress.XtraBars.Docking.DockingStyle.Bottom;
            this._propertiesDockPanel.FloatSize = new System.Drawing.Size(200, 166);
            this._propertiesDockPanel.FloatVertical = true;
            this._propertiesDockPanel.ID = new System.Guid("4b4c54af-8b8f-46dd-8d18-a72bf15b2424");
            this._propertiesDockPanel.Location = new System.Drawing.Point(0, 421);
            this._propertiesDockPanel.Name = "_propertiesDockPanel";
            this._propertiesDockPanel.Size = new System.Drawing.Size(1045, 171);
            this._propertiesDockPanel.Text = "Properties";
            // 
            // dockPanel1_Container
            // 
            this.dockPanel1_Container.Controls.Add(this._labelPropertiesPanel);
            this.dockPanel1_Container.Controls.Add(this._rectangularFramePropertiesPanel);
            this.dockPanel1_Container.Controls.Add(this._noPropertiesPanel);
            this.dockPanel1_Container.Location = new System.Drawing.Point(4, 21);
            this.dockPanel1_Container.Name = "dockPanel1_Container";
            this.dockPanel1_Container.Size = new System.Drawing.Size(1037, 146);
            this.dockPanel1_Container.TabIndex = 0;
            // 
            // _labelPropertiesPanel
            // 
            this._labelPropertiesPanel.Appearance.BackColor = System.Drawing.SystemColors.Control;
            this._labelPropertiesPanel.Appearance.Options.UseBackColor = true;
            this._labelPropertiesPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._labelPropertiesPanel.Controls.Add(this._labelCaptionLabel);
            this._labelPropertiesPanel.Controls.Add(this._labelLabelLabel);
            this._labelPropertiesPanel.Controls.Add(this._labelCaption);
            this._labelPropertiesPanel.Controls.Add(this._labelLabel);
            this._labelPropertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._labelPropertiesPanel.Location = new System.Drawing.Point(0, 0);
            this._labelPropertiesPanel.LookAndFeel.UseDefaultLookAndFeel = false;
            this._labelPropertiesPanel.LookAndFeel.UseWindowsXPTheme = true;
            this._labelPropertiesPanel.Name = "_labelPropertiesPanel";
            this._labelPropertiesPanel.Size = new System.Drawing.Size(1037, 146);
            this._labelPropertiesPanel.TabIndex = 6;
            // 
            // _labelCaptionLabel
            // 
            this._labelCaptionLabel.Location = new System.Drawing.Point(288, 7);
            this._labelCaptionLabel.Name = "_labelCaptionLabel";
            this._labelCaptionLabel.Size = new System.Drawing.Size(41, 13);
            this._labelCaptionLabel.TabIndex = 3;
            this._labelCaptionLabel.Text = "Caption:";
            // 
            // _labelLabelLabel
            // 
            this._labelLabelLabel.Location = new System.Drawing.Point(8, 7);
            this._labelLabelLabel.Name = "_labelLabelLabel";
            this._labelLabelLabel.Size = new System.Drawing.Size(29, 13);
            this._labelLabelLabel.TabIndex = 2;
            this._labelLabelLabel.Text = "Label:";
            // 
            // _labelCaption
            // 
            this._labelCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this._labelCaption.Location = new System.Drawing.Point(288, 26);
            this._labelCaption.Name = "_labelCaption";
            this._labelCaption.Size = new System.Drawing.Size(436, 117);
            this._labelCaption.TabIndex = 1;
            // 
            // _labelLabel
            // 
            this._labelLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this._labelLabel.Location = new System.Drawing.Point(8, 26);
            this._labelLabel.Name = "_labelLabel";
            this._labelLabel.Size = new System.Drawing.Size(274, 117);
            this._labelLabel.TabIndex = 0;
            // 
            // _noPropertiesPanel
            // 
            this._noPropertiesPanel.Appearance.BackColor = System.Drawing.SystemColors.Control;
            this._noPropertiesPanel.Appearance.Options.UseBackColor = true;
            this._noPropertiesPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._noPropertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._noPropertiesPanel.Location = new System.Drawing.Point(0, 0);
            this._noPropertiesPanel.LookAndFeel.UseDefaultLookAndFeel = false;
            this._noPropertiesPanel.LookAndFeel.UseWindowsXPTheme = true;
            this._noPropertiesPanel.Name = "_noPropertiesPanel";
            this._noPropertiesPanel.Size = new System.Drawing.Size(1037, 146);
            this._noPropertiesPanel.TabIndex = 0;
            // 
            // _imageCollection
            // 
            this._imageCollection.ImageStream = ((DevExpress.Utils.ImageCollectionStreamer)(resources.GetObject("_imageCollection.ImageStream")));
            // 
            // barButtonItem2
            // 
            this.barButtonItem2.Caption = "barButtonItem2";
            this.barButtonItem2.Id = 1;
            this.barButtonItem2.Name = "barButtonItem2";
            // 
            // _tabs
            // 
            this._tabs.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._tabs.Controller = this._tabsController;
            this._tabs.MdiParent = this;
            // 
            // _tabsController
            // 
            this._tabsController.AppearancesBar.Bar.BackColor = System.Drawing.SystemColors.Control;
            this._tabsController.AppearancesBar.Bar.BackColor2 = System.Drawing.SystemColors.ButtonShadow;
            this._tabsController.AppearancesBar.Bar.BorderColor = System.Drawing.SystemColors.ControlDarkDark;
            this._tabsController.AppearancesBar.Bar.Options.UseBackColor = true;
            this._tabsController.AppearancesBar.Bar.Options.UseBorderColor = true;
            this._tabsController.AppearancesBar.Dock.BackColor = System.Drawing.SystemColors.Control;
            this._tabsController.AppearancesBar.Dock.Options.UseBackColor = true;
            this._tabsController.LookAndFeel.Style = DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
            this._tabsController.LookAndFeel.UseDefaultLookAndFeel = false;
            // 
            // _rectangularFramePropertiesPanel
            // 
            this._rectangularFramePropertiesPanel.Appearance.BackColor = System.Drawing.SystemColors.Control;
            this._rectangularFramePropertiesPanel.Appearance.Options.UseBackColor = true;
            this._rectangularFramePropertiesPanel.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this._rectangularFramePropertiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rectangularFramePropertiesPanel.Location = new System.Drawing.Point(0, 0);
            this._rectangularFramePropertiesPanel.LookAndFeel.UseDefaultLookAndFeel = false;
            this._rectangularFramePropertiesPanel.LookAndFeel.UseWindowsXPTheme = true;
            this._rectangularFramePropertiesPanel.Name = "_rectangularFramePropertiesPanel";
            this._rectangularFramePropertiesPanel.Size = new System.Drawing.Size(1037, 146);
            this._rectangularFramePropertiesPanel.TabIndex = 7;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 614);
            this.Controls.Add(this._propertiesDockPanel);
            this.Controls.Add(this.barDockControlLeft);
            this.Controls.Add(this.barDockControlRight);
            this.Controls.Add(this.barDockControlBottom);
            this.Controls.Add(this.barDockControlTop);
            this.IsMdiContainer = true;
            this.Name = "MainForm";
            this.Text = "Adorned Paste Up";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this._bars)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._barAndDockingController)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._dockManager)).EndInit();
            this._propertiesDockPanel.ResumeLayout(false);
            this.dockPanel1_Container.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._labelPropertiesPanel)).EndInit();
            this._labelPropertiesPanel.ResumeLayout(false);
            this._labelPropertiesPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._labelCaption.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._labelLabel.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._noPropertiesPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._imageCollection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tabs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._tabsController)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._rectangularFramePropertiesPanel)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraBars.BarManager _bars;
        private DevExpress.XtraBars.Bar bar1;
        private DevExpress.XtraBars.Bar bar2;
        private DevExpress.XtraBars.Bar bar3;
        private DevExpress.XtraBars.BarDockControl barDockControlTop;
        private DevExpress.XtraBars.BarDockControl barDockControlBottom;
        private DevExpress.XtraBars.BarDockControl barDockControlLeft;
        private DevExpress.XtraBars.BarDockControl barDockControlRight;
        private DevExpress.XtraBars.BarButtonItem _addScreenShotItem;
        private DevExpress.XtraTabbedMdi.XtraTabbedMdiManager _tabs;
        private DevExpress.XtraBars.BarAndDockingController _tabsController;
        private DevExpress.XtraBars.BarSubItem barSubItem1;
        private DevExpress.XtraBars.BarButtonItem barButtonItem2;
        private DevExpress.XtraBars.BarAndDockingController _barAndDockingController;
        private DevExpress.XtraBars.Docking.DockManager _dockManager;
        private DevExpress.XtraBars.Docking.DockPanel _propertiesDockPanel;
        private DevExpress.XtraBars.Docking.ControlContainer dockPanel1_Container;
        private DevExpress.XtraBars.BarButtonItem _addLabelItem;
        private DevExpress.XtraBars.BarButtonItem _saveAsItem;
        private DevExpress.XtraBars.BarButtonItem _openItem;
        private DevExpress.XtraBars.BarButtonItem _exportImageItem;
        private DevExpress.XtraEditors.PanelControl _noPropertiesPanel;
        private DevExpress.XtraEditors.PanelControl _labelPropertiesPanel;
        private DevExpress.XtraEditors.LabelControl _labelCaptionLabel;
        private DevExpress.XtraEditors.LabelControl _labelLabelLabel;
        private DevExpress.XtraEditors.MemoEdit _labelCaption;
        private DevExpress.XtraEditors.MemoEdit _labelLabel;
        private DevExpress.XtraBars.BarButtonItem _saveItem;
        private DevExpress.Utils.ImageCollection _imageCollection;
        private DevExpress.XtraBars.BarButtonItem _newItem;
        private DevExpress.XtraBars.BarButtonItem _closeItem;
        private DevExpress.XtraBars.BarButtonItem _exitItem;
        private DevExpress.XtraEditors.PanelControl _rectangularFramePropertiesPanel;
    }
}

