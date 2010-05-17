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

using Interlace.Collections;

#endregion

namespace Interlace.AdornedText
{
    public class BlockSequence : Block
    {
        TrackedBindingList<Block> _blocks;

        public BlockSequence()
        {
            _blocks = new TrackedBindingList<Block>();
            _blocks.Added += new EventHandler<TrackedBindingListEventArgs<Block>>(_blocks_Added);
            _blocks.Removed += new EventHandler<TrackedBindingListEventArgs<Block>>(_blocks_Removed);
        }

        void _blocks_Added(object sender, TrackedBindingListEventArgs<Block> e)
        {
            e.Item.Parent = this;
        }

        void _blocks_Removed(object sender, TrackedBindingListEventArgs<Block> e)
        {
            e.Item.Parent = null;
        }

        public IList<Block> Blocks
        {
            get { return _blocks; }
        }

        public void ReplaceBlock(Block existingBlock, Block replacementBlockOrNull)
        {
            int index = _blocks.IndexOf(existingBlock);

            if (index == -1) throw new ArgumentException("The existing block does not exist in the block sequence.", "existingBlock");

            if (replacementBlockOrNull == null)
            {
                _blocks.RemoveAt(index);
            }
            else
            {
                _blocks[index] = replacementBlockOrNull;
            }
        }

        public override void Visit(NodeVisitor visitor)
        {
            visitor.VisitBlockSequence(this);

            foreach (Block block in _blocks) block.Visit(visitor);

            base.Visit(visitor);
        }
    }
}
