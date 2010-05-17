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
using System.Text;

using Interlace.AdornedPasteUp.Documents;

#endregion

namespace Interlace.AdornedPasteUp.Rendering
{
    public class ImageLinkCache : IDisposable
    {
        ImageLinkManager _manager;

        Dictionary<ImageLink, Image> _images;

        public ImageLinkCache(ImageLinkManager manager)
        {
            _manager = manager;

            _images = new Dictionary<ImageLink, Image>();

            _manager.LinkAttached += new EventHandler<LinkEventArgs>(_manager_LinkAttached);
            _manager.LinkDetached += new EventHandler<LinkEventArgs>(_manager_LinkDetached);

            foreach (ImageLink link in manager.Links)
            {
                _manager_LinkAttached(manager, new LinkEventArgs(manager, link));
            }
        }

        public Image GetCachedImage(ImageLink link)
        {
            return _images[link];
        }

        void _manager_LinkAttached(object sender, LinkEventArgs e)
        {
            if (_images.ContainsKey(e.Link)) throw new InvalidOperationException();

            _images[e.Link] = e.Link.CreateBitmap();
        }

        void _manager_LinkDetached(object sender, LinkEventArgs e)
        {
            if (!_images.ContainsKey(e.Link)) throw new InvalidOperationException();
            
            Image image = _images[e.Link];

            image.Dispose();

            _images.Remove(e.Link);
        }
    
        #region IDisposable Members

        public void Dispose()
        {
            if (_images != null)
            {
                foreach (Image image in _images.Values)
                {
                    image.Dispose();
                }

                _images = null;
            }
        }

        #endregion
    }
}
