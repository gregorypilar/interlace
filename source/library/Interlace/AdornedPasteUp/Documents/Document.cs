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
using System.IO;
using System.Text;

using Interlace.Collections;
using Interlace.PropertyLists;
using Interlace.Utilities;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    public class Document : IDisposable, INotifyPropertyChanged
    {
        TrackedBindingList<DocumentFrame> _frames = new TrackedBindingList<DocumentFrame>();
        bool _wasDownConverted = false;

        ImageLinkManager _imageLinkManager;

        string _filePathOrNull;

        public Document()
        {
            _imageLinkManager = new ImageLinkManager();

            _frames.Added += new EventHandler<TrackedBindingListEventArgs<DocumentFrame>>(_frames_Added);
            _frames.Removed += new EventHandler<TrackedBindingListEventArgs<DocumentFrame>>(_frames_Removed);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool WasDownConverted
        { 	 
            get { return _wasDownConverted; }
        }

        public string FilePathOrNull
        { 	 
            get { return _filePathOrNull; }
            set 
            { 
                _filePathOrNull = value;

                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("FilePathOrNull"));
            }
        }

        void _frames_Added(object sender, TrackedBindingListEventArgs<DocumentFrame> e)
        {
            if (e.Item.Document != null) throw new InvalidOperationException();

            e.Item.Document = this;
        }

        void _frames_Removed(object sender, TrackedBindingListEventArgs<DocumentFrame> e)
        {
            e.Item.Document = null;
        }

        public ImageLinkManager ImageLinkManager
        { 	 
            get { return _imageLinkManager; }
        }

        public BindingList<DocumentFrame> Frames
        {
            get { return _frames; }
        }

        public static int MajorVersion
        {
            get { return 1; }
        }

        public static int MinorVersion
        {
            get { return 0; }
        }

        public PropertyDictionary Serialize(string desiredFilePath)
        {
            PropertyDictionary container = PropertyDictionary.EmptyDictionary();
            PropertyDictionary document = PropertyDictionary.EmptyDictionary();

            container.SetValueFor("pasteUp", document);

            // Later major versions can not be read:
            document.SetValueFor("majorVersion", 1); 

            // Later minor versions can be read, but with potential loss of information; 
            // they should not be saved over:
            document.SetValueFor("minorVersion", 0);

            DocumentSerializationContext context = new DocumentSerializationContext(
                Path.GetDirectoryName(desiredFilePath), Path.GetFileNameWithoutExtension(desiredFilePath));

            PropertyDictionary imageLinkManagerDictionary = _imageLinkManager.Serialize(context);
            document.SetValueFor("imageLinkManager", imageLinkManagerDictionary);

            PropertyArray frames = PropertyArray.EmptyArray();
            document.SetValueFor("frames", frames);

            foreach (DocumentFrame frame in _frames)
            {
                frames.AppendValue(frame.Serialize(context));
            }

            return container;
        }

        public static Document Deserialize(PropertyDictionary container, string absolutePath)
        {
            PropertyDictionary documentProperties = container.DictionaryFor("pasteUp");

            if (documentProperties == null) return null;

            if (!documentProperties.HasIntegerFor("majorVersion", "majorVersion")) 
            {
                throw new DocumentReadingException("The document lacks the required major and minor version numbers.");
            }

            if (documentProperties.IntegerFor("majorVersion") > MajorVersion)
            {
                throw new DocumentReadingException("The document version is not supported by this reader.");
            }

            Document document = new Document();

            if (documentProperties.IntegerFor("majorVersion") == MajorVersion &&
                documentProperties.IntegerFor("minorVersion") > MinorVersion)
            {
                document._wasDownConverted = true;
            }

            DocumentDeserializationContext context = new DocumentDeserializationContext(
                documentProperties.IntegerFor("majorVersion").Value,
                documentProperties.IntegerFor("minorVersion").Value,
                absolutePath);

            PropertyDictionary imageLinkManagerProperties = documentProperties.DictionaryFor("imageLinkManager");

            if (imageLinkManagerProperties == null) throw new DocumentReadingException(
                "The image link manager dictionary is missing from the document.");

            document._imageLinkManager = new ImageLinkManager(context, imageLinkManagerProperties);

            PropertyArray framesArray = documentProperties.ArrayFor("frames");

            if (framesArray == null) throw new DocumentReadingException("The frames array is missing from the document.");

            for (int i = 0; i < framesArray.Count; i++)
            {
                PropertyDictionary frameProperties = framesArray.DictionaryAt(i);

                if (frameProperties == null) throw new DocumentReadingException(
                    "The document frame array contains an invalid non-dictionary object.");

                document._frames.Add(DeserializeFrame(context, frameProperties));
            }

            return document;
        }

        static DocumentFrame DeserializeFrame(DocumentDeserializationContext context, PropertyDictionary frameProperties)
        {
            string type = frameProperties.StringFor("type", "missing");

            switch (type)
            {
                case "label":
                    return new LabelDocumentFrame(context, frameProperties);

                case "rectangular":
                    return new RectangularDocumentFrame(context, frameProperties);

                default:
                    throw new DocumentReadingException(string.Format("The frame type \"{0}\" is not recognised by this version.",
                        type));
            }
        }

        public void Dispose()
        {
            foreach (DocumentFrame frame in _frames)
            {
                frame.Dispose();
            }
        }

        void ThrowOnInvalidDocumentFrame(DocumentFrame documentFrame)
        {
            if (documentFrame == null) 
            {
                throw new ArgumentNullException("documentFrame");
            }

            if (!_frames.Contains(documentFrame))
            {
                throw new ArgumentException(
                    "The document frame supplied to a document method is not actually within the document.", "documentFrame");
            }
        }

        public void DeleteFrame(DocumentFrame documentFrame)
        {
            ThrowOnInvalidDocumentFrame(documentFrame);

            _frames.Remove(documentFrame);
            documentFrame.Dispose();
        }

        public void BringFrameToFront(DocumentFrame documentFrame)
        {
            ThrowOnInvalidDocumentFrame(documentFrame);

            _frames.Remove(documentFrame);
            _frames.Add(documentFrame);
        }

        public void SendFrameToBack(DocumentFrame documentFrame)
        {
            ThrowOnInvalidDocumentFrame(documentFrame);

            _frames.Remove(documentFrame);
            _frames.Insert(0, documentFrame);
        }
    }
}
