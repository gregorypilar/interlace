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
using System.Text;

#endregion

namespace Interlace.Collections
{
    public class EqualityPartitioner<T>
    {
        public delegate object GetPropertyForEqualityRelation(T o);

        GetPropertyForEqualityRelation _propertyGetter;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:EqualityPartitioner"/> class.
        /// </summary>
        /// <param name="relation">A function returning an object derived in some way from the
        /// objects being partitioned. The objects are partitioned based on equality of
        /// the derived object.</param>
        public EqualityPartitioner(GetPropertyForEqualityRelation propertyGetter)
        {
            _propertyGetter = propertyGetter;
        }

        public Set<Set<T>> Partition(ICollection<T> objects)
        {
            // A dictionary of the sets in the partition, keyed by some object in each set:
            Dictionary<object, Set<T>> partition = new Dictionary<object, Set<T>>();

            foreach (T candidate in objects)
            {
                object property = _propertyGetter(candidate);

                // First see if an existing set in the partition is suitable:
                if (!partition.ContainsKey(property))
                {
                    partition[property] = new Set<T>();
                }

                partition[property].UnionUpdate(candidate);
            }

            return new Set<Set<T>>(partition.Values);
        }
    }
}
