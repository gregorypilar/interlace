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

using Interlace.Utilities;

#endregion

namespace Interlace.Collections
{
    public class SetComparison<TSource, TDestination, THash>
    {
        IEnumerable<TSource> _source;
        IEnumerable<TDestination> _destination;

        List<Pair<TSource, TDestination>> _unmodified;
        List<TSource> _deleted;
        List<TDestination> _created;
            
        public delegate THash SourceHasher(TSource element);
        public delegate THash DestinationHasher(TDestination element);

        // Hash and hasher are misnomers; it isn't a hash.

        public SetComparison(IEnumerable<TSource> source, SourceHasher sourceHasher, IEnumerable<TDestination> destination, DestinationHasher destinationHasher)
        {
            _source = source;
            _destination = destination;

            // Create a dictionary of hashes to destination elements:
            Dictionary<THash, TDestination> destinationDictionary = new Dictionary<THash, TDestination>();

            foreach (TDestination destinationElement in destination)
            {
                THash hash = destinationHasher(destinationElement);

                if (destinationDictionary.ContainsKey(hash))
                {
                    throw new InvalidOperationException(string.Format(
                        "The set comparer found two elements in the destination with the same hash (\"{0}\").", hash));
                }

                destinationDictionary[hash] = destinationElement;
            }

            // Find new or existing elements in the source; destructively remove matched items from the dictionary:
            _deleted = new List<TSource>();
            _unmodified = new List<Pair<TSource, TDestination>>();

            foreach (TSource sourceElement in source)
            {
                THash sourceHash = sourceHasher(sourceElement);

                if (destinationDictionary.ContainsKey(sourceHash))
                {
                    _unmodified.Add(new Pair<TSource, TDestination>(sourceElement, destinationDictionary[sourceHash]));
                }
                else
                {
                    _deleted.Add(sourceElement);
                }

                destinationDictionary.Remove(sourceHash);
            }

            // Anything left in the destination dictionary must be new:
            _created = new List<TDestination>();
            _created.AddRange(destinationDictionary.Values);
        }

        public IEnumerable<TSource> Source
        { 	 
           get { return _source; }
        }

        public IEnumerable<TDestination> Destination
        { 	 
           get { return _destination; }
        }

        public IEnumerable<Pair<TSource, TDestination>> Unmodified
        { 	 
           get { return _unmodified; }
        }

        public IEnumerable<TSource> Deleted
        { 	 
           get { return _deleted; }
        }

        public IEnumerable<TDestination> Created
        { 	 
           get { return _created; }
        }
    }
}
