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

namespace Interlace.Collections
{
    public class Partitioner<T>
    {
        public delegate bool IsInEquivalenceRelationDelegate(T a, T b);

        IsInEquivalenceRelationDelegate _relation;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Partitioner"/> class.
        /// </summary>
        /// <param name="relation">A function returning true if the two ojbects are
        /// in the relation used to generate the partition. To generate a
        /// correct partition, the relation must be an equivalence relation; i.e., reflexive, 
        /// symmetric and transistive.</param>
        public Partitioner(IsInEquivalenceRelationDelegate relation)
        {
            _relation = relation;
        }

        public Set<Set<T>> Partition(IEnumerable<T> objects)
        {
            Set<Set<T>> partition = new Set<Set<T>>();

            foreach (T candidate in objects)
            {
                Set<T> setToAddTo = null;

                // First see if an existing set in the partition is suitable:
                foreach (Set<T> set in partition)
                {
                    Debug.Assert(set.Count >= 1);

                    if (_relation(candidate, set.AnyItem))
                    {
                        setToAddTo = set;

                        break;
                    }
                }

                // If not, add a new set to the partition:
                if (setToAddTo == null)
                {
                    setToAddTo = new Set<T>();

                    partition.UnionUpdate(setToAddTo);
                }

                setToAddTo.UnionUpdate(candidate);
            }

            return partition;
        }
    }
}
