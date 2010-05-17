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
    /// <summary>
    /// A visitor for RTree nodes.
    /// </summary>
    public delegate void RTreeVisitorDelegate<T>(T obj);
    
    [Serializable]
    public class RTree
    {
        RTreeNode _root;

        int _nodeCapacity;
        int _nodeMinimum;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RTree"/> class.
        /// </summary>
        /// <param name="nodeCapacity">The node capacity.</param>
        /// <param name="nodeMinimum">The node minimum.</param>
        public RTree(int nodeCapacity, int nodeMinimum)
        {
            if (nodeMinimum > nodeCapacity / 2)
            {
                throw new ArgumentException("The node minimum must be less than or equal to " +
                    "half of the node capacity.", "nodeMinimum");
            }
            
            _nodeCapacity = nodeCapacity;
            _nodeMinimum = nodeMinimum;

            _root = new RTreeLeaf(this);
            _root.Parent = null;
        }

        public int NodeCapacity
        {
            get
            {
                return _nodeCapacity;
            }
        }

        public int NodeMinimum
        {
            get
            {
                return _nodeMinimum;
            }
        }

        /// <summary>
        /// Inserts the specified object.
        /// </summary>
        /// <param name="bounds">The bounding box of the object.</param>
        /// <param name="obj">The object.</param>
        public void Insert<T>(Box bounds, T obj)
        {
            _root.Insert(new RTreeObject(bounds, obj));
        }

        /// <summary>
        /// Finds objects intersecting the supplied bounding box.
        /// </summary>
        public void Find<T>(RTreeVisitorDelegate<T> visitor, Box bounds) 
        {
            _root.Find<T>(visitor, bounds);
        }

        public bool DoesBoxExist(Box bounds)
        {
            return _root.DoesBoxExist(bounds);
        }

        internal void ReplaceRootWithSplitResult(RTreeNode lhsNode, RTreeNode rhsNode)
        {
            RTreeNonLeaf newRoot = new RTreeNonLeaf(this);

            newRoot.ReplaceNodeWithSplitResult(null, lhsNode, rhsNode);

            _root = newRoot;
        }

        public void ThrowOnInvariantsViolated()
        {
            if (_root == null) throw new InvalidOperationException();

            if (_root.Parent != null) throw new InvalidOperationException();

            _root.ThrowOnInvariantsViolated(null);
        }
    }
}
