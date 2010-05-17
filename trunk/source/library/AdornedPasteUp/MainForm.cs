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
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Interlace.AdornedPasteUp;
using Interlace.AdornedPasteUp.Documents;
using Interlace.AdornedPasteUp.Editing;
using Interlace.Binding;
using Interlace.Binding.Views;
using Interlace.ScreenShooting;

#endregion

namespace AdornedPasteUp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Binder _mainFormBinder;
        Binder _selectedFormBinder;
        Binder _selectedPasteUpBinder;

        private void MainForm_Load(object sender, EventArgs e)
        {
            _mainFormBinder = new Binder(this);
            _selectedFormBinder = new Binder(_mainFormBinder, "SelectedForm");
            _selectedPasteUpBinder = new Binder(_selectedFormBinder, "PasteUpControl");
            
            PanelPerTypeView panelPerTypeView = new PanelPerTypeView();
            panelPerTypeView.NullPanel = _noPropertiesPanel;
            panelPerTypeView.AddPanel(typeof(LabelDocumentFrame), _labelPropertiesPanel);
            _selectedPasteUpBinder.AddBinding("SelectedFrame", panelPerTypeView);

            Binder labelPropertiesBinder = new Binder();
            _selectedPasteUpBinder.AddBinding("SelectedFrame", new PropertyView(labelPropertiesBinder, "BoundTo", null, true),
                new TypeFilterConverter<LabelDocumentFrame>());

            labelPropertiesBinder.AddBinding("Label", new BaseEditBinderView(_labelLabel));
            labelPropertiesBinder.AddBinding("Caption", new BaseEditBinderView(_labelCaption));

            MdiChildActivate += new EventHandler(MainForm_MdiChildActivate);
        }

        #region Selection Properties

        public PasteUpForm SelectedForm
        {
            get 
            {
                return ActiveMdiChild as PasteUpForm;
            }
        }

        public event EventHandler SelectedFormChanged;

        void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            if (SelectedFormChanged != null) SelectedFormChanged(this, EventArgs.Empty);
        }

        #endregion

        bool HasSelectedPasteUp
        {
            get { return _selectedPasteUpBinder.BoundTo != null; }
        }

        PasteUpControl SelectedPasteUp
        {
            get { return _selectedPasteUpBinder.BoundTo as PasteUpControl; }
        }

        private void _newItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            PasteUpForm.New(this);
        }

        private void _addLabelItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (HasSelectedPasteUp) SelectedPasteUp.AddNewFrame(new LabelDocumentFrame());
        }

        private void _saveAsItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (HasSelectedPasteUp) SelectedPasteUp.SaveAs();
        }

        private void _saveItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (HasSelectedPasteUp) SelectedPasteUp.Save();
        }

        private void _openItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            PasteUpForm.Open(this);
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (HasSelectedPasteUp) SelectedPasteUp.SaveImage();
        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (!HasSelectedPasteUp) return;

            Hide();
            Application.DoEvents();
            Thread.Sleep(300);

            try
            {
                ScreenShot screenShot = ScreenShot.Capture();

                using (Bitmap captureBitmap = ScreenShotForm.EditScreenShot(screenShot))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        captureBitmap.Save(stream, ImageFormat.Png);
                    
                        SelectedPasteUp.AddEncodedImage(stream.ToArray());
                    }
                }
            }
            finally
            {
                Application.DoEvents();

                this.Show();
            }
        }

        private void _closeItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (SelectedForm != null)
            {
                SelectedForm.Close();
            }
        }

        private void _exitItem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Close();
        }
    }
}
