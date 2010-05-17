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
using System.Drawing;
using System.Text;

using Interlace.AdornedPasteUp.Rendering;
using Interlace.PropertyLists;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    public abstract class DocumentFrame : INotifyPropertyChanged
    {
        protected Point _offsetInDocument;

        Document _document;

        public abstract Rectangle HitBounds { get; }
        public abstract Rectangle PaintBounds { get; }
        public abstract void Paint(Graphics g, DocumentPaintResources resources);

        public abstract DocumentFrameMemento CreateMemento();
        public abstract void RestoreFromMemento(DocumentFrameMemento memento);

        protected abstract IEnumerable<IHandleFlyweight> Handles { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public DocumentFrame()
        {
        }

        public virtual Document Document
        { 	 
            get { return _document; }
            internal set { _document = value; }
        }

        public DocumentFrame(DocumentDeserializationContext context, PropertyDictionary frameProperties)
        {
            if (!frameProperties.HasDictionaryFor("offsetInDocument"))
            {
                throw new DocumentReadingException("A frame is missing the required \"offsetInDocument\" field.");
            }

            _offsetInDocument = PropertyBuilders.ToPoint(frameProperties.DictionaryFor("offsetInDocument"));
        }

        internal virtual PropertyDictionary Serialize(DocumentSerializationContext context)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor("offsetInDocument", PropertyBuilders.FromPoint(_offsetInDocument));

            return dictionary;
        }


        protected void FirePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void PaintHandles(Graphics graphics, DocumentPaintResources resources)
        {
            foreach (IHandleFlyweight handle in Handles)
            {
                handle.Paint(this, graphics, resources);
            }
        }

        internal IHandleFlyweight FindHitHandleOrNull(Point point)
        {
            foreach (IHandleFlyweight handle in Handles)
            {
                if (handle.IsHit(this, point)) return handle;
            }

            return null;
        }

        public Point OffsetInDocument
        { 	 
           get { return _offsetInDocument; }
           set { _offsetInDocument = value; }
        }

        ~DocumentFrame()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
