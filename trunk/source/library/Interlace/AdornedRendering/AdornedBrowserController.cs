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
    public class AdornedBrowserController : IDisposable
    {
        AdornedBrowserRendering _currentRendering = null;
        AdornedRenderer _defaultRenderer = new AdornedRenderer();
        IAdornedReferenceResolverFactory _defaultReferenceResolverFactory = new AdornedFileResolverFactory();
    
        public string NavigateTo(TextReader reader, string referencesDirectory)
        {
            return NavigateTo(reader, referencesDirectory, _defaultRenderer, _defaultReferenceResolverFactory);
        }

        public string NavigateTo(TextReader reader, string referencesDirectory, AdornedRenderer renderer)
        {
            return NavigateTo(reader, referencesDirectory, renderer, _defaultReferenceResolverFactory);
        }

        public string NavigateTo(TextReader reader, string referencesDirectory, AdornedRenderer renderer, 
            IAdornedReferenceResolverFactory referenceResolverFactory)
        {
            if (_currentRendering != null)
            {
                _currentRendering.Dispose();
            }

            _currentRendering = new AdornedBrowserRendering();

            IAdornedReferenceResolver resolver = 
                referenceResolverFactory.CreateReferenceResolver(_currentRendering, referencesDirectory);

            _currentRendering.RenderWith(reader, resolver, renderer);

            return _currentRendering.DocumentFileName;
        }

        public string NavigateTo(Section document, string referencesDirectory, AdornedRenderer renderer)
        {
            return NavigateTo(document, referencesDirectory, renderer);
        }

        public string NavigateTo(Section document, string referencesDirectory, AdornedRenderer renderer,
            IAdornedReferenceResolverFactory referenceResolverFactory)
        {
            if (_currentRendering != null)
            {
                _currentRendering.Dispose();
            }

            _currentRendering = new AdornedBrowserRendering();

            IAdornedReferenceResolver resolver = 
                referenceResolverFactory.CreateReferenceResolver(_currentRendering, referencesDirectory);

            _currentRendering.RenderWith(document, resolver, renderer);

            return _currentRendering.DocumentFileName;
        }

        public void Dispose()
        {
            if (_currentRendering != null)
            {
                _currentRendering.Dispose();

                _currentRendering = null;
            }
        }
    }
}
