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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Interlace.AdornedText;

#endregion

namespace Interlace.AdornedRendering
{
    class AdornedBrowserRendering : IDisposable, ITemporaryFileManager
    {
        string _temporaryFileDirectory = null;
        List<string> _temporaryFilePaths = new List<string>();

        string _documentfileName = null;

        int _nextAnonymousFileNameNumber = 1;

        public AdornedBrowserRendering()
        {
        }

        public void EnsureTemporaryDirectoryCreated()
        {
            if (_temporaryFileDirectory != null) return;

            Guid guid = Guid.NewGuid();

            string directory = "adorned-" + guid.ToString("D");

            _temporaryFileDirectory = Path.Combine(Path.GetTempPath(), directory);

            Directory.CreateDirectory(_temporaryFileDirectory);
        }

        public string CreateTemporaryFilePath(string filename)
        {
            EnsureTemporaryDirectoryCreated();

            string path = Path.Combine(_temporaryFileDirectory, filename);

            _temporaryFilePaths.Add(path);

            return path;
        }

        public string CreateAnonymousFilePath(params string[] extensions)
        {
            EnsureTemporaryDirectoryCreated();

            int baseNumber = _nextAnonymousFileNameNumber;
            _nextAnonymousFileNameNumber++;

            string firstPath = null;

            foreach (string extension in extensions)
            {
                string path = Path.Combine(_temporaryFileDirectory, string.Format("anon{0}{1}",
                    baseNumber, extension));

                if (firstPath == null) firstPath = path;

                _temporaryFilePaths.Add(path);
            }

            return firstPath;
        }

        void DeleteFiles()
        {
            if (_temporaryFileDirectory != null)
            {
                foreach (string temporaryFilePath in _temporaryFilePaths)
                {
                    try
                    {
                        File.Delete(temporaryFilePath);
                    }
                    catch (IOException)
                    {
                        // Do nothing.
                    }
                }

                try
                {
                    Directory.Delete(_temporaryFileDirectory);
                }
                catch (IOException)
                {
                    // Do nothing;
                }

                _temporaryFileDirectory = null;
                _temporaryFilePaths.Clear();
            }
        }

        public string DocumentFileName
        {
            get { return _documentfileName; }
        }

        internal void RenderWith(TextReader reader, IAdornedReferenceResolver resolver, AdornedRenderer renderer)
        {
            _documentfileName = CreateTemporaryFilePath("document.html");

            using (StreamWriter writer = new StreamWriter(_documentfileName))
            {
                renderer.RenderDocument(reader, resolver, writer);
            }
        }

        internal void RenderWith(Section document, IAdornedReferenceResolver resolver, AdornedRenderer renderer)
        {
            _documentfileName = CreateTemporaryFilePath("document.html");

            using (StreamWriter writer = new StreamWriter(_documentfileName))
            {
                renderer.RenderDocument(document, resolver, writer);
            }
        }

        ~AdornedBrowserRendering()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                DeleteFiles();

                _disposed = true;
            }
        }
    }
}
