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
using System.Text;

using Interlace.Collections;

#endregion

namespace Interlace.AdornedText
{
    public class SequenceSpan : Span, ISpanContainer
    {
        TrackedCollection<Span> _spans;

        public SequenceSpan()
        {
            _spans = new TrackedCollection<Span>();
            _spans.Added += new EventHandler<TrackedCollectionEventArgs<Span>>(_spans_Added);
            _spans.Removed += new EventHandler<TrackedCollectionEventArgs<Span>>(_spans_Removed);
        }

        void _spans_Added(object sender, TrackedCollectionEventArgs<Span> e)
        {
            e.Item.Parent = this;
        }

        void _spans_Removed(object sender, TrackedCollectionEventArgs<Span> e)
        {
            e.Item.Parent = null;
        }

        public IEnumerable<Span> ContainedSpans
        {
            get { return _spans; }
        }

        public void ReplaceSpan(Span existingSpan, Span replacementSpan)
        {
            int index = _spans.IndexOf(existingSpan);

            if (index == -1) throw new ArgumentException("The existing span is not contained in this container.", "existingSpan");

            if (replacementSpan != null)
            {
                _spans[index] = replacementSpan;
            }
            else
            {
                _spans.RemoveAt(index);
            }
        }

        public Collection<Span> Spans
        {
            get { return _spans; }
        }

        public Block Block
        {
            get
            {
                return Parent.Block;
            }
        }

        public override void Visit(NodeVisitor visitor)
        {
            visitor.VisitSequenceSpan(this);

            foreach (Span item in _spans) item.Visit(visitor);

            base.Visit(visitor);
        }
    }
}
