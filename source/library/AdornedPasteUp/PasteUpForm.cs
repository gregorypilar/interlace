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

using DevExpress.XtraBars;
using DevExpress.XtraEditors;

using Interlace.AdornedPasteUp.Editing;
using Interlace.Binding;
using Interlace.Binding.Views;

#endregion

namespace AdornedPasteUp
{
    public partial class PasteUpForm : DevExpress.XtraEditors.XtraForm
    {
        PasteUpControl _pasteUp;

        public PasteUpForm()
        {
            InitializeComponent();

            _pasteUp = new PasteUpControl();
            _pasteUp.Parent = this;
            _pasteUp.Dock = DockStyle.Fill;
            _pasteUp.MouseDown += new MouseEventHandler(_pasteUp_MouseDown);

            Binder formBinder = new Binder(this);
            Binder controlBinder = new Binder(formBinder, "PasteUpControl");
            controlBinder.AddBinding("Text", new PropertyView(this, "Text", "Unknown"));
        }

        void _pasteUp_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _popupMenu.ShowPopup(_pasteUp.PointToScreen(e.Location));
            }
        }

        private void PasteUpForm_Load(object sender, EventArgs e)
        {
        }

        public PasteUpControl PasteUpControl
        {
            get { return _pasteUp; }
        }

        private void _deleteItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_pasteUp.SelectedFrame == null) return;

            _pasteUp.DeleteFrame(_pasteUp.SelectedFrame);
        }

        private void _bringToFrontItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_pasteUp.SelectedFrame == null) return;

            _pasteUp.BringFrameToFront(_pasteUp.SelectedFrame);
        }

        private void _sendToBackItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_pasteUp.SelectedFrame == null) return;

            _pasteUp.SendFrameToBack(_pasteUp.SelectedFrame);
        }

        public static void New(Form parent)
        {
            PasteUpForm form = new PasteUpForm();
            form.MdiParent = parent;
            form.Show();
        }

        public static void Open(Form parent)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Adorned Image (*.ati)|*.ati";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    PasteUpForm form = new PasteUpForm();
                    form.PasteUpControl.Open(openFileDialog.FileName);
                    form.MdiParent = parent;
                    form.Show();
                }
            }
        }
    }
}
