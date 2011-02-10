#region Using Directives and Copyright Notice

// Copyright (c) 2010, Bit Plantation
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Bit Plantation nor the
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
using System.ComponentModel;

#endregion

namespace Interlace.Collections
{
    public abstract class AdapterCollection<TAdapter, TValue, TKey> : BindingList<TAdapter>
    {
        protected abstract TKey GetKeyFromValue(TValue value);
        protected abstract TKey GetKeyFromAdapter(TAdapter adapter);
        protected abstract TAdapter CreateAdapter(TValue value);
        protected abstract void UpdateAdapter(TValue value, TAdapter adapter);

        public void Merge(IList<TValue> source) 
        {
            Merge(source, true);
        }

        public void Update(IList<TValue> partialSource) 
        {
            Merge(partialSource, false);
        }

        void Merge(IList<TValue> source, bool removeMissingEntities) 
        {
            // Build a dictionary of existing items (that we later mutate in a gross way):
            Dictionary<TKey, int> existingIndicies = new Dictionary<TKey, int>();

            int i = 0;

            foreach (TAdapter item in this)
            {
                existingIndicies[GetKeyFromAdapter(item)] = i;

                i++;
            }

    		// Add new entities, and update existing entities:
    		foreach (TValue sourceItem in source) 
            {
                int foundIndex;

                TKey sourceKey = GetKeyFromValue(sourceItem);

                bool found = existingIndicies.TryGetValue(sourceKey, out foundIndex);

                if (found)
                {
                    UpdateAdapter(sourceItem, this[foundIndex]);

                    existingIndicies.Remove(sourceKey);
                }
                else
                {
                    Add(CreateAdapter(sourceItem));
                }
            }

        	// Remove missing entities:
            if (removeMissingEntities)
            {
        		List<int> indiciesToRemove = new List<int>(existingIndicies.Values);

                indiciesToRemove.Sort();

                int indiciesRemoved = 0;

                foreach (int index in indiciesToRemove)
                {
                    RemoveAt(index - indiciesRemoved);

                    indiciesRemoved++;
                }
            }
        }
    }
}
