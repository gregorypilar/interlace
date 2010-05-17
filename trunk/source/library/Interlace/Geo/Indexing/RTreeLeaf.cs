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
using System.Diagnostics;
using System.Text;

#endregion

namespace Interlace.Geo.Indexing
{
    [Serializable]
    class RTreeLeaf : RTreeNode
    {
        List<RTreeObject> _children;

        public RTreeLeaf(RTree tree)
        : base(tree)
        {
            _children = new List<RTreeObject>(Tree.NodeCapacity + 1);
        }

        internal override void Insert(RTreeObject obj)
        {
            _children.Add(obj);

            if (_children.Count > Tree.NodeCapacity)
            {
                SplitNode();
            }
            else
            {
                AdjustTree();
            }
        }

        /// <summary>
        /// Splits the node in to two new notes with capacity for insertion.
        /// </summary>
        /// <remarks>After calling this method, the node is invalidated.</remarks>
        void SplitNode()
        {
            // Split the children in to two buckets of elements:
            RTreeBoundedsBucket<RTreeObject> lhsBucket;
            RTreeBoundedsBucket<RTreeObject> rhsBucket;

            RTreeBoundedsBucket<RTreeObject>.Split(_children, Tree.NodeMinimum, out lhsBucket, out rhsBucket);

            // Copy the new elements in to this and the other leaf:
            RTreeLeaf lhs = new RTreeLeaf(Tree);
            lhs._children.AddRange(lhsBucket);
            lhs.AdjustNode();

            RTreeLeaf rhs = new RTreeLeaf(Tree);
            rhs._children.AddRange(rhsBucket);
            rhs.AdjustNode();

            // Swap the old element with the two new elements:
            ReplaceNodeWithSplitResultInParent(this, lhs, rhs);

            // Destroy this node to avoid hard to diagnose problems with hanging links:
            _children.Clear();
        }

        internal void AdjustNode()
        {
            Box newBounds = Box.EmptyBox;

            foreach (RTreeObject child in _children)
            {
                newBounds.ExpandToInclude(child.Bounds);
            }

            Bounds = newBounds;
        }

        internal void AdjustTree()
        {
            AdjustNode();

            if (Parent != null) Parent.AdjustTree();
        }

        internal override void Find<T>(RTreeVisitorDelegate<T> visitor, Box bounds) 
        {
            foreach (RTreeObject child in _children)
            {
                if (child.Bounds.Intersects(bounds))
                {
                    visitor((T)child.Pointer);
                }
            }
        }

        internal override void ThrowOnInvariantsViolated(RTreeNode callingNode)
        {
            if (Parent != callingNode) throw new InvalidOperationException();

            if (_children.Count < Tree.NodeMinimum && callingNode != null) throw new InvalidOperationException();
            if (_children.Count > Tree.NodeCapacity) throw new InvalidOperationException();
        }

        internal override bool DoesBoxExist(Box bounds)
        {
            foreach (RTreeObject child in _children)
            {
                if (child.Bounds.Intersects(bounds)) return true;
            }

            return false;
        }
    }
}
