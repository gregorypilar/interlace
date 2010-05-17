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
using System.IO;
using System.Text;

using Interlace.Utilities;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    public class DocumentSerializationContext
    {
        Dictionary<ImageLink, int> _imageLinkKeys;
        string _absolutePath;
        string _extensionlessName;

        public DocumentSerializationContext(string absolutePathOrNull, string extensionlessName)
        {
            _imageLinkKeys = new Dictionary<ImageLink, int>();
            _absolutePath = absolutePathOrNull;

            if (_absolutePath == null)
            {
                _absolutePath = Environment.CurrentDirectory;
            }

            _extensionlessName = extensionlessName;
        }

        public Dictionary<ImageLink, int> ImageLinkKeys
        { 	 
            get { return _imageLinkKeys; }
        }

        public string AbsolutePath
        { 	 
            get { return _absolutePath; }
        }

        public string ExtensionlessName
        {
            get { return _extensionlessName; }
        }

        internal object GetRelativeFileName(string _fileName)
        {
            return RelativePath.FromAbsolute(_fileName, _absolutePath);
        }
    }
}
