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
using System.Text;

using Interlace.PropertyLists;

#endregion

namespace Interlace.AdornedPasteUp.Documents
{
    public class ImageLinkManager
    {
        Dictionary<ImageLink, int> _referenceCountByLink = new Dictionary<ImageLink, int>();

        public ImageLinkManager()
        {
        }

        internal ImageLinkManager(DocumentDeserializationContext context, PropertyDictionary managerDictionary)
        {
            PropertyDictionary imageLinks = managerDictionary.DictionaryFor("imageLinks");

            if (imageLinks == null) throw new InvalidOperationException("The image manager dictionary is missing the \"imageLinks\" key.");

            foreach (object key in imageLinks.Keys)
            {
                if (!(key is int)) throw new InvalidOperationException("The image links dictionary contains a non-integer key, which is not permitted.");

                int linkIndex = (int)key;

                PropertyDictionary imageDictionary = imageLinks.DictionaryFor(key);

                ImageLink link = new ImageLink(context, imageDictionary);

                _referenceCountByLink[link] = 0;
                context.ImageLinksByKey[linkIndex] = link;
            }
        }

        internal PropertyDictionary Serialize(DocumentSerializationContext context)
        {
            PropertyDictionary managerDictionary = PropertyDictionary.EmptyDictionary();

            PropertyDictionary imageLinks = PropertyDictionary.EmptyDictionary();
            managerDictionary.SetValueFor("imageLinks", imageLinks);

            int linkIndex = 1;

            foreach (ImageLink link in _referenceCountByLink.Keys)
            {
                context.ImageLinkKeys[link] = linkIndex;

                imageLinks.SetValueFor(linkIndex, link.Serialize(context));

                linkIndex++;
            }

            return managerDictionary;
        }

        public event EventHandler<LinkEventArgs> LinkAttached;
        public event EventHandler<LinkEventArgs> LinkDetached;

        public void Attach(ImageLink link)
        {
            if (_referenceCountByLink.ContainsKey(link))
            {
                _referenceCountByLink[link] = _referenceCountByLink[link] + 1;
            }
            else
            {
                _referenceCountByLink[link] = 1;

                if (LinkAttached != null) LinkAttached(this, new LinkEventArgs(this, link));
            }
        }

        public void Detach(ImageLink link)
        {
            if (!_referenceCountByLink.ContainsKey(link)) throw new InvalidOperationException();

            int newReferenceCount = _referenceCountByLink[link] - 1;

            if (newReferenceCount == 0)
            {
                _referenceCountByLink.Remove(link);

                if (LinkDetached != null) LinkDetached(this, new LinkEventArgs(this, link));
            }
            else
            {
                _referenceCountByLink[link] = newReferenceCount;
            }
        }

        public IEnumerable<ImageLink> Links
        {
            get
            {
                foreach (ImageLink link in _referenceCountByLink.Keys)
                {
                    yield return link;
                }
            }
        }
    }
}
