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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

#endregion

namespace Interlace.Collections
{
    /// <summary>
    /// A binding list that guarantees a call to an event any time an item is added and removed from a list.
    /// </summary>
    /// <typeparam name="T">The type of item in the list.</typeparam>
    /// <remarks>The guarantee is based on the reflected source code; in the current version of the framework,
    /// nothing other than the overridable methods of Collection&lt;T&gt; modifies the list.</remarks>
    public class TrackedCollection<T> : Collection<T>
    {
        public event EventHandler<TrackedCollectionEventArgs<T>> Added;
        public event EventHandler<TrackedCollectionEventArgs<T>> Removed;

        protected override void ClearItems()
        {
            foreach (T item in Items)
            {
                if (Removed != null) Removed(this, new TrackedCollectionEventArgs<T>(item));
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            if (Added != null) Added(this, new TrackedCollectionEventArgs<T>(item));
        }

        protected override void RemoveItem(int index)
        {
            if (Removed != null) Removed(this, new TrackedCollectionEventArgs<T>(Items[index]));

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            T existingItem = Items[index];

            if (!object.ReferenceEquals(item, existingItem))
            {
                if (Removed != null) Removed(this, new TrackedCollectionEventArgs<T>(existingItem));
            }

            base.SetItem(index, item);

            if (!object.ReferenceEquals(item, existingItem))
            {
                if (Added != null) Added(this, new TrackedCollectionEventArgs<T>(item));
            }
        }
    }
}
