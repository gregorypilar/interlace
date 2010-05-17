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
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Interlace.PropertyLists;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    public class ImageLink 
    {
        string _fileName;
        Size _physicalDimension;
        byte[] _unsavedFileContents;

        public ImageLink(string fileName)
        {
            _fileName = fileName;
            _unsavedFileContents = null;

            using (Bitmap bitmap = CreateBitmap())
            {
                _physicalDimension = bitmap.PhysicalDimension.ToSize();
            }
        }

        public ImageLink(byte[] data)
        {
            _fileName = null;
            _unsavedFileContents = data;

            using (Bitmap bitmap = CreateBitmap())
            {
                _physicalDimension = bitmap.PhysicalDimension.ToSize();
            }
        }

        public static Regex _unsavedPattern = new Regex(@"(.*),\s*Screen\s*Shot\s*(\d{1,6})\s*$");

        void EnsureSaved(DocumentSerializationContext context)
        {
            if (_unsavedFileContents == null) return;

            DirectoryInfo directory = new DirectoryInfo(context.AbsolutePath);

            int nextAvailableNumber = 1;

            foreach (FileInfo file in directory.GetFiles())
            {
                Match match = _unsavedPattern.Match(file.Name);

                if (!match.Success) continue;
                if (string.Compare(match.Groups[1].Value.Trim(), context.ExtensionlessName, true) != 0) continue;

                int thisNumber = int.Parse(match.Groups[2].Value);

                nextAvailableNumber = Math.Max(nextAvailableNumber, thisNumber + 1);
            }

            string extensionlessImageName = string.Format("{0}, Screen Shot {1}", context.ExtensionlessName, nextAvailableNumber);
            string fileName = Path.Combine(context.AbsolutePath, Path.ChangeExtension(extensionlessImageName, ".png"));

            using (Stream fileStream = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
            {
                fileStream.Write(_unsavedFileContents, 0, _unsavedFileContents.Length);
            }

            _fileName = fileName;
            _unsavedFileContents = null;
        }

        public Bitmap CreateBitmap()
        {
            if (_fileName != null)
            {
                return new Bitmap(_fileName);
            }
            else if (_unsavedFileContents != null)
            {
                // Closing the stream causes GDI+ exceptions; avoid disposing the MemoryStream:
                MemoryStream stream = new MemoryStream(_unsavedFileContents);

                return new Bitmap(stream);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public ImageLink(DocumentDeserializationContext context, PropertyDictionary dictionary)
        {
            _fileName = Path.Combine(context.AbsolutePath, dictionary.StringFor("fileName"));
            _physicalDimension = PropertyBuilders.ToSize(dictionary.DictionaryFor("physicalDimension"));
        }

        internal PropertyDictionary Serialize(DocumentSerializationContext context)
        {
            EnsureSaved(context);

            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor("fileName", context.GetRelativeFileName(_fileName));
            dictionary.SetValueFor("physicalDimension", PropertyBuilders.FromSize(_physicalDimension));

            return dictionary;
        }

        public string FileName
        { 	 
            get { return _fileName; }
        }

        public Size PhysicalDimension
        {
            get { return _physicalDimension; }
        }
    }
}
