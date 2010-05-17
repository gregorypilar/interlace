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

namespace Interlace.Geo.Indexing
{
    /// <summary>
    /// A container of objects with bounding boxes. A number of special spatial operations intended
    /// for use building and maintaining R-Trees are included in the collection.
    /// </summary>
    /// <typeparam name="T">The objects to be stored in the container.</typeparam>
    internal class RTreeBoundedsBucket<T> : IRTreeBounded, IEnumerable<T> where T : IRTreeBounded
    {
        List<T> _elements;
        Box _cachedBounds;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RTreeBoundedsBucket&lt;T&gt;"/> class.
        /// </summary>
        public RTreeBoundedsBucket()
        {
            _elements = new List<T>();

            _cachedBounds = Box.EmptyBox;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:RTreeBoundedsBucket&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="elements">The initial elements.</param>
        public RTreeBoundedsBucket(ICollection<T> elements)
        {
            _elements = new List<T>(elements);

            UpdateCachedBounds();
        }

        /// <summary>
        /// Gets the bounding box of all contained boundables.
        /// </summary>
        /// <value>The bounding box.</value>
        public Box Bounds
        {
            get
            {
                return _cachedBounds;
            }
        }

        /// <summary>
        /// Updates the cached bounding box.
        /// </summary>
        void UpdateCachedBounds()
        {
            _cachedBounds = Box.EmptyBox;

            foreach (T element in _elements)
            {
                _cachedBounds.ExpandToInclude(element.Bounds);
            }
        }

        /// <summary>
        /// Adds the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Add(T element)
        {
            _elements.Add(element);

            _cachedBounds.ExpandToInclude(element.Bounds);
        }

        /// <summary>
        /// Gets the count of contained objects.
        /// </summary>
        /// <value>The number of contined objects.</value>
        public int Count
        {
            get
            {
                return _elements.Count;
            }
        }

        public void MoveAllElementsTo(RTreeBoundedsBucket<T> destinationBucket)
        {
            destinationBucket._elements.AddRange(_elements);
            _elements.Clear();
        }

        /// <summary>
        /// Picks the two best seeds for a node split, and removes them from this bucket.
        /// </summary>
        /// <param name="children">The child objects.</param>
        /// <param name="lhs">The first seed object.</param>
        /// <param name="rhs">The second seed object.</param>
        public void RemoveSplitSeeds(out T lhs, out T rhs)
        {
            if (_elements.Count < 2)
            {
                throw new InvalidOperationException("Calling the PickSeeds(...) method is only " +
                    "valid when there are two or more objects to be picked.");
            }

            int mostWastefulI = 0;
            int mostWastefulJ = 1;
            double mostWastefulArea = Double.NegativeInfinity;

            for (int i = 0; i < _elements.Count; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    double wastedArea = GetAreaWastedMetric(
                        _elements[i].Bounds, _elements[j].Bounds);

                    if (wastedArea > mostWastefulArea)
                    {
                        mostWastefulI = i;
                        mostWastefulJ = j;
                        mostWastefulArea = wastedArea;
                    }
                }
            }

            // Remove the i'th element first; since j < i, the position of j won't change:
            lhs = _elements[mostWastefulI];
            _elements.RemoveAt(mostWastefulI);

            rhs = _elements[mostWastefulJ];
            _elements.RemoveAt(mostWastefulJ);
        }

        /// <summary>
        /// Picks the next object that should be added to two splitting nodes, and removes it
        /// from the bucket.
        /// </summary>
        /// <param name="lhsBounds">The first node bounds.</param>
        /// <param name="rhsBounds">The second node bounds.</param>
        /// <returns>The most suitable bounded object.</returns>
        public T RemoveSplitNext(Box lhsBounds, Box rhsBounds)
        {
            if (_elements.Count == 0)
            {
                throw new InvalidOperationException("There are no more objects to be removed.");
            }

            int maximumIndex = 0;
            double maximumAreaDelta = 0;

            for (int i = 0; i < _elements.Count; i++)
            {
                double lhsIncrease = GetCombinedArea(lhsBounds, _elements[i].Bounds) - lhsBounds.Area;
                double rhsIncrease = GetCombinedArea(rhsBounds, _elements[i].Bounds) - rhsBounds.Area;

                double delta = Math.Abs(lhsIncrease - rhsIncrease);

                if (delta >= maximumAreaDelta)
                {
                    maximumIndex = i;
                    maximumAreaDelta = delta;
                }
            }

            T toRemove = _elements[maximumIndex];
            _elements.RemoveAt(maximumIndex);

            return toRemove;
        }

        /// <summary>
        /// Gets the best destination, attempting to minimise the increase in bounds required
        /// in the destination to accommodate the new element.
        /// </summary>
        /// <param name="destinationFor">The element to be placed in a destination.</param>
        /// <param name="lhsDestination">The first destination.</param>
        /// <param name="rhsDestination">The second destination.</param>
        /// <returns>The best destination for the new element.</returns>
        public static DT GetBestDestination<DT>(T destinationFor, DT lhsDestination, DT rhsDestination) where DT : IRTreeBounded
        {
            double lhsAreaDelta = GetCombinedArea(destinationFor.Bounds, lhsDestination.Bounds) - 
                lhsDestination.Bounds.Area;

            double rhsAreaDelta = GetCombinedArea(destinationFor.Bounds, rhsDestination.Bounds) - 
                rhsDestination.Bounds.Area;

            if (lhsAreaDelta < rhsAreaDelta ||
                (lhsAreaDelta == rhsAreaDelta && lhsDestination.Bounds.Area < rhsDestination.Bounds.Area))
            {
                return lhsDestination;
            }
            else
            {
                return rhsDestination;
            }
        }

        /// <summary>
        /// Gets the best destination, attempting to minimise the increase in bounds required
        /// in the destination to accommodate the new element.
        /// </summary>
        /// <param name="destinationFor">The element to be placed in a destination.</param>
        /// <param name="destinations">The destinations.</param>
        /// <returns>The best destination for the new element.</returns>
        public static DT GetBestDestination<DT>(T destinationFor, ICollection<DT> destinations) where DT : IRTreeBounded
        {
            double minimumAreaDelta = Double.PositiveInfinity;
            double minimumArea = Double.PositiveInfinity;
            DT minimumDestination = default(DT);

            foreach (DT destination in destinations)
            {
                double areaDelta = GetCombinedArea(destinationFor.Bounds, destination.Bounds) -
                    destination.Bounds.Area;

                if (areaDelta < minimumAreaDelta || 
                    (areaDelta == minimumAreaDelta && destination.Bounds.Area < minimumArea))
                {
                    minimumAreaDelta = areaDelta;
                    minimumArea = destination.Bounds.Area;
                    minimumDestination = destination;
                }
            }

            return minimumDestination;
        }

        /// <summary>
        /// Splits the specified elements in to two buckets, attempting to minimise the
        /// probability that both buckets will overlap any given bounding box.
        /// </summary>
        /// <param name="elements">The elements to split.</param>
        /// <param name="minimum">The minimum number of elements in either bucket.</param>
        /// <param name="lhsResult">The first resulting bucket.</param>
        /// <param name="rhsResult">The second resulting bucket.</param>
        public static void Split(ICollection<T> elements, int minimum, 
            out RTreeBoundedsBucket<T> lhsResult, out RTreeBoundedsBucket<T> rhsResult)
        {
            // Create buckets for the remaining objects, and the two destinations:
            RTreeBoundedsBucket<T> source = new RTreeBoundedsBucket<T>(elements);

            RTreeBoundedsBucket<T> lhs = new RTreeBoundedsBucket<T>();
            RTreeBoundedsBucket<T> rhs = new RTreeBoundedsBucket<T>();

            // Pick the seed nodes:
            T lhsSeed;
            T rhsSeed;

            source.RemoveSplitSeeds(out lhsSeed, out rhsSeed);

            lhs.Add(lhsSeed);
            rhs.Add(rhsSeed);

            while (source.Count > 0)
            {
                // If either side needs the remaining children to make up its minimum,
                // give the node the remaining children:
                int lhsCountUntilMinimum = Math.Max(0, minimum - lhs.Count);
                int rhsCountUntilMinimum = Math.Max(0, minimum - rhs.Count);

                if (source.Count <= lhsCountUntilMinimum)
                {
                    source.MoveAllElementsTo(lhs);

                    break;
                }

                if (source.Count <= rhsCountUntilMinimum)
                {
                    source.MoveAllElementsTo(rhs);

                    break;
                }

                // Otherwise, find the next node and add it:
                T next = source.RemoveSplitNext(lhs.Bounds, rhs.Bounds);
                RTreeBoundedsBucket<T> best = GetBestDestination(next,
                    lhs, rhs);

                best.Add(next);
            }

            lhsResult = lhs;
            rhsResult = rhs;
        }

        /// <summary>
        /// Gets the area that would be wasted by placing both boxes in the same R-Tree group.
        /// </summary>
        /// <param name="lhs">The first box.</param>
        /// <param name="rhs">The second box.</param>
        /// <returns>The area not contained by the two boxes in the combined rectangle. The
        /// result may be negative; see the R-Tree paper's "PickSeeds" algorithm.</returns>
        internal static double GetAreaWastedMetric(Box lhs, Box rhs)
        {
            return GetCombinedArea(lhs, rhs) - lhs.Area - rhs.Area;
        }

        /// <summary>
        /// Gets the combined area of the two bounding boxes.
        /// </summary>
        /// <param name="lhs">The first bounding box.</param>
        /// <param name="rhs">The second bounding box.</param>
        /// <returns></returns>
        internal static double GetCombinedArea(Box lhs, Box rhs)
        {
            return (Math.Max(lhs.X2, rhs.X2) - Math.Min(lhs.X1, lhs.X2)) *
                (Math.Max(lhs.Y2, rhs.Y2) - Math.Min(lhs.Y1, lhs.Y2));
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}
