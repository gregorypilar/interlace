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
    public delegate IEnumerable<T> EdgeGetter<T>(T forObject);

    public class TopologicalSort
    {
        enum VertexState
        {
            Unexplored = 0,
            Discovered,
            Finished
        }

        public static ICollection<T> Sort<T>(ICollection<T> objects, EdgeGetter<T> edgeGetter)
        {
            List<T> finishedObjects = new List<T>();
            Dictionary<T, VertexState> states = new Dictionary<T, VertexState>(objects.Count);

            foreach (T o in objects)
            {
                if (states.ContainsKey(o)) 
                {
                    throw new TopologicalSortException("An object in a list to be topologically " +
                        "sorted is equal to another object in the list, which is not valid.");
                }

                states[o] = VertexState.Unexplored;
            }

            foreach (T visiting in objects) 
            {
                if (states[visiting] == VertexState.Unexplored)
                {
                    DepthFirstSearchVisit(objects, edgeGetter, states, visiting, finishedObjects);
                }
            }

            finishedObjects.Reverse();

            return finishedObjects;
        }

        static void DepthFirstSearchVisit<T>(ICollection<T> objects, EdgeGetter<T> edgeGetter, Dictionary<T, VertexState> states, T visiting, List<T> finishedObjects)
        {
            states[visiting] = VertexState.Discovered;

            foreach (T edge in edgeGetter(visiting))
            {
                if (states[edge] == VertexState.Unexplored)
                {
                    DepthFirstSearchVisit(objects, edgeGetter, states, edge, finishedObjects);
                } 
                else if (states[edge] == VertexState.Discovered)
                {
                    // The edge is a back-edge; the graph is cyclic:

                    throw new TopologicalSortException("The supplied graph is cyclic, and therefore has " +
                        "no topological sort.");
                }
            }
            
            states[visiting] = VertexState.Finished;

            finishedObjects.Add(visiting);
        }
    }
}
