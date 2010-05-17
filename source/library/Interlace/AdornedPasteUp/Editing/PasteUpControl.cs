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
using System.Text;
using System.Windows.Forms;

using Interlace.AdornedPasteUp.Documents;
using Interlace.AdornedPasteUp.Rendering;
using Interlace.PropertyLists;

#endregion

namespace Interlace.AdornedPasteUp.Editing
{
    public partial class PasteUpControl : Control
    {
        Document _document = null;

        DocumentFrame _selectedFrame = null;

        DragController _dragController = null;

        ImageLinkCache _cache = null;

        public PasteUpControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            Document = new Document();

            Text = "Untitled";

            this.DocumentChanged += new EventHandler(PasteUpControl_DocumentChanged);
        }

        void PasteUpControl_DocumentChanged(object sender, EventArgs e)
        {
            string value = Document.FilePathOrNull;

            if (value == null)
            {
                Text = "Untitled";
            }
            else
            {
                Text = Path.GetFileName(value);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_cache != null) 
            {
                _cache.Dispose();

                _cache = null;
            }

            base.OnHandleDestroyed(e);
        }

        public event EventHandler DocumentChanged;

        public Document Document
        {
            get { return _document; }
            set
            {
                if (_document == value) return;

                // Detach from the current document, if any:
                if (_document != null)
                {
                    SelectNone();

                    _document.Frames.ListChanged -= new ListChangedEventHandler(_frames_ListChanged);

                    _cache.Dispose();
                    _cache = null;
                }

                // Switch to the new document:
                _document = value;

                _document.Frames.ListChanged += new ListChangedEventHandler(_frames_ListChanged);

                _cache = new ImageLinkCache(_document.ImageLinkManager);

                Invalidate();

                if (DocumentChanged != null) DocumentChanged(this, EventArgs.Empty);
            }
        }

        public DocumentFrame SelectedFrame
        {
            get { return _selectedFrame; }
        }

        public event EventHandler SelectedFrameChanged;

        public void SelectNone()
        {
            _selectedFrame = null;

            if (SelectedFrameChanged != null) SelectedFrameChanged(this, EventArgs.Empty);
        }

        void _frames_ListChanged(object sender, ListChangedEventArgs e)
        {
            Invalidate();
        }

        public void AddNewFrame(DocumentFrame documentFrame)
        {
            _document.Frames.Add(documentFrame);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, ClientRectangle);

            using (DocumentPaintResources resources = new DocumentPaintResources(_cache))
            {
                resources.IsFadedFrame = false;

                foreach (DocumentFrame frame in _document.Frames)
                {
                    resources.IsSelectedFrame = (frame == _selectedFrame);
                    resources.SelectedHandleOrNull = _dragController != null ? _dragController.Handle : null;

                    frame.Paint(e.Graphics, resources);

                    if (frame == _selectedFrame)
                    {
                        frame.PaintHandles(e.Graphics, resources);

                        resources.IsFadedFrame = true;
                    }
                }
            }
        }

        DocumentFrame FindHitFrameOrNull(Point location)
        {
            for (int i = 0; i < _document.Frames.Count; i++)
            {
                DocumentFrame frame = _document.Frames[_document.Frames.Count - i - 1];

                if (frame.HitBounds.Contains(location)) return frame;
            }

            return null;
        }

        void PasteUpControl_MouseDown(object sender, MouseEventArgs e)
        {
            // Test hitting a handle first:
            if (_selectedFrame != null)
            {
                IHandleFlyweight handle = _selectedFrame.FindHitHandleOrNull(e.Location);

                if (handle != null)
                {
                    _dragController = new DragController(e.Location, _selectedFrame, handle);
                    _dragController.BeginDrag();

                    return;
                }
            }

            // Then try hitting a frame:
            DocumentFrame hitFrame = FindHitFrameOrNull(e.Location);

            if (_selectedFrame != hitFrame)
            {
                _selectedFrame = hitFrame;

                if (SelectedFrameChanged != null) SelectedFrameChanged(this, EventArgs.Empty);

                Invalidate();
            }
        }

        private void PasteUpControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (_dragController == null) return;

            _dragController.EndDrag();
            _dragController = null;

            Invalidate();
        }

        private void PasteUpControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragController == null) return;

            _dragController.UpdateDrag(e.Location);

            Invalidate();
        }

        public void Save()
        {
            if (_document.FilePathOrNull == null) 
            {
                SaveAs();
            }
            else
            {
                PropertyDictionary dictionary = _document.Serialize(_document.FilePathOrNull);
                dictionary.PersistToFile(_document.FilePathOrNull);
            }
        }

        public void SaveAs()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Adorned Image (*.ati)|*.ati";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    PropertyDictionary dictionary = _document.Serialize(saveFileDialog.FileName);
                    dictionary.PersistToFile(saveFileDialog.FileName);

                    _document.FilePathOrNull = saveFileDialog.FileName;
                }
            }
        }

        public void SaveImage()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PNG Image (*.png)|*.png";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (PasteUpRenderer renderer = new PasteUpRenderer(_document))
                    {
                        using (Bitmap bitmap = renderer.Render())
                        {
                            bitmap.Save(saveFileDialog.FileName);
                        }
                    }
                }
            }
        }

        public void Open(string fileName)
        {
            PropertyDictionary dictionary = PropertyDictionary.FromFile(fileName);
            Document _newDocument = Document.Deserialize(dictionary, Path.GetDirectoryName(fileName));

            _newDocument.FilePathOrNull = fileName;

            Document = _newDocument;
        }

        public void DeleteFrame(DocumentFrame documentFrame)
        {
            _document.DeleteFrame(documentFrame);
        }

        public void BringFrameToFront(DocumentFrame documentFrame)
        {
            _document.BringFrameToFront(documentFrame);
        }

        public void SendFrameToBack(DocumentFrame documentFrame)
        {
            _document.SendFrameToBack(documentFrame);
        }

        public void AddEncodedImage(byte[] image)
        {
            ImageLink link = new ImageLink(image);

            _document.Frames.Add(new RectangularDocumentFrame(new DocumentImage(link)));
        }

        private void PasteUpControl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                e.Effect = DragDropEffects.Link;
            }
        }

        private void PasteUpControl_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileNames = e.Data.GetData(DataFormats.FileDrop) as string[];

            foreach (string fileName in fileNames)
            {
                ImageLink link = new ImageLink(Path.GetFullPath(fileName));

                _document.Frames.Add(new RectangularDocumentFrame(new DocumentImage(link)));
            }
        }
    }
}
