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

#endregion

namespace Interlace.AdornedText
{
    public class Paragraph : Block, ISpanContainer
    {
        Span _span;

        public Paragraph(Span span)
        {
            _span = span;
            _span.Parent = this;
        }

        public Span Span
        {
            get { return _span; }
        }

        public IEnumerable<Span> ContainedSpans
        {
            get { if (_span != null) yield return _span; }
        }

        public Block Block
        {
            get { return this; }
        }

        public void ReplaceSpan(Span existingSpan, Span replacementSpan)
        {
            if (_span != existingSpan) throw new ArgumentException("The existing span is not the child of this paragraph.", "existingSpan");

            if (_span != null) _span.Parent = null;

            _span = replacementSpan;

            if (_span != null) _span.Parent = this;
        }

        public override void Visit(NodeVisitor visitor)
        {
            visitor.VisitParagraph(this);

            _span.Visit(visitor);

            base.Visit(visitor);
        }
    }
}
