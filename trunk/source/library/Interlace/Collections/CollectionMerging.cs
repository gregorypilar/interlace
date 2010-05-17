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
    public class CollectionMerging
    {
        /// <summary>
        /// Merges two lists containing (independantly) unique elements. The destination list will contain the same elements
        /// but in an arbitrary ordering.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source and destination list.</typeparam>
        /// <param name="source">The source list.</param>
        /// <param name="destination">The destination list; this list is modified to match the source list.</param>
        public static void MergeUniqueLists<T>(IList<T> source, IList<T> destination) 
        {
            // Build a dictionary of existing items (that we later mutate in a gross way):
            Dictionary<T, int> existingIndicies = new Dictionary<T, int>();

            int i = 0;

            foreach (T item in destination)
            {
                existingIndicies[item] = i;

                i++;
            }

    		// Add new entities, and update existing entities:
    		foreach (T sourceItem in source) 
            {
                int foundIndex;

                bool found = existingIndicies.TryGetValue(sourceItem, out foundIndex);

                if (found)
                {
                    destination[foundIndex] = sourceItem;

                    existingIndicies.Remove(sourceItem);
                }
                else
                {
                    destination.Add(sourceItem);
                }
            }

        	// Remove missing entities:
    		List<int> indiciesToRemove = new List<int>(existingIndicies.Values);

            indiciesToRemove.Sort();

            int indiciesRemoved = 0;

            foreach (int index in indiciesToRemove)
            {
                destination.RemoveAt(index - indiciesRemoved);

                indiciesRemoved++;
            }
        }
    }
}
