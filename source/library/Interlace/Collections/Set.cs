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
    [Serializable]
    public class Set<T> : IEnumerable<T>
    {
        Dictionary<T, int> _members;

        public Set()
        {
            _members = new Dictionary<T, int>();
        }

        public Set(params T[] members)
            : this(members as IEnumerable<T>)
        { }

        public Set(IEnumerable<T> members)
            : this()
        {
            foreach(T item in members)
            {
                _members[item] = 1;
            }
        }

        /// <summary>
        /// Deprecated.  Use the IEnumerable Constructor instead.
        /// </summary>
        /// <param name="members">An enumerable collection of items to put in the set.</param>
        /// <returns>A set initialized with the members.</returns>
        public static Set<T> FromEnumerable(IEnumerable<T> members)
        {
            return new Set<T>(members);
        }

        public int Count
        {
            get { return _members.Count; }
        }

        public Set<T> Copy()
        {
            return Set<T>.FromEnumerable(this);
        }

        public override bool Equals(object obj)
        {
            Set<T> rhs = obj as Set<T>;

            if (rhs == null) return false;

            if (Count != rhs.Count) return false;

            // If the lhs is a subset of the rhs:
            foreach (T item in _members.Keys)
            {
                if (!rhs._members.ContainsKey(item)) return false;
            }

            // And if the rhs is a subset of the lhs:
            foreach (T item in rhs._members.Keys)
            {
                if (!_members.ContainsKey(item)) return false;
            }

            // Then by definition, the sets are equal:
            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = _members.Count;

            foreach (T item in _members.Keys)
            {
                hashCode ^= item.GetHashCode();
            }

            return hashCode;
        }

        public override string ToString()
        {
            if (Count == 0) return "{}";

            StringBuilder stringList = new StringBuilder();

            foreach (T item in _members.Keys)
            {
                if (stringList.Length != 0) stringList.Append(", ");

                stringList.Append(item.ToString());
            }

            stringList.Insert(0, "{ ");
            stringList.Append(" }");

            return stringList.ToString();
        }

        public T[] ToArray()
        {
            T[] array = new T[_members.Keys.Count];

            int i = 0;

            foreach (T item in _members.Keys)
            {
                array[i] = item;
                i++;
            }

            return array;
        }

        public bool Contains(T item)
        {
            return _members.ContainsKey(item);
        }

        public bool IsSubsetOf(Set<T> rhs)
        {
            foreach (T item in _members.Keys)
            {
                if (!rhs._members.ContainsKey(item)) return false;
            }

            return true;
        }

        public static Set<T> Union(Set<T> lhs, Set<T> rhs)
        {
            Set<T> newSet = new Set<T>();

            foreach (T item in lhs._members.Keys)
            {
                newSet._members[item] = 1;
            }

            foreach (T item in rhs._members.Keys)
            {
                newSet._members[item] = 1;
            }

            return newSet;
        }

        public static Set<T> Intersection(Set<T> lhs, Set<T> rhs)
        {
            Set<T> newSet = new Set<T>();

            foreach (T item in lhs._members.Keys)
            {
                if (rhs._members.ContainsKey(item))
                {
                    newSet._members[item] = 1;
                }
            }

            return newSet;
        }

        public static Set<T> Difference(Set<T> lhs, Set<T> rhs)
        {
            Set<T> newSet = new Set<T>();

            foreach (T item in lhs._members.Keys)
            {
                if (!rhs._members.ContainsKey(item))
                {
                    newSet._members[item] = 1;
                }
            }

            return newSet;
        }

        public void Clear()
        {
            _members.Clear();
        }

        public void UnionUpdate(T item)
        {
            _members[item] = 1;
        }

        public void UnionUpdate(Set<T> rhs)
        {
            foreach (T item in rhs._members.Keys)
            {
                _members[item] = 1;
            }
        }

        public void UnionUpdate(IEnumerable<T> rhs)
        {
            foreach (T item in rhs)
            {
                _members[item] = 1;
            }
        }

        public void DifferenceUpdate(T rhs)
        {
            _members.Remove(rhs);
        }

        public void DifferenceUpdate(Set<T> rhs)
        {
            foreach (T item in rhs._members.Keys)
            {
                _members.Remove(item);
            }
        }

        public void DifferenceUpdate(IEnumerable<T> rhs)
        {
            foreach (T item in rhs)
            {
                _members.Remove(item);
            }
        }

        public void IntersectionUpdate(Set<T> rhs)
        {
            List<T> itemsToRemove = new List<T>();

            foreach (T item in _members.Keys)
            {
                if (!rhs._members.ContainsKey(item)) itemsToRemove.Add(item);
            }

            foreach (T itemToRemove in itemsToRemove)
            {
                _members.Remove(itemToRemove);
            }
        }

        public static Set<T> operator +(Set<T> lhs, Set<T> rhs)
        {
            return Set<T>.Union(lhs, rhs);
        }

        public static Set<T> operator *(Set<T> lhs, Set<T> rhs)
        {
            return Set<T>.Intersection(lhs, rhs);
        }

        public static Set<T> operator -(Set<T> lhs, Set<T> rhs)
        {
            return Set<T>.Difference(lhs, rhs);
        }

        public static bool operator ==(Set<T> lhs, Set<T> rhs)
        {
            return Set<T>.Equals(lhs, rhs);
        }

        public static bool operator !=(Set<T> lhs, Set<T> rhs)
        {
            return !Set<T>.Equals(lhs, rhs);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _members.Keys.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _members.Keys.GetEnumerator();
        }

        public T AnyItem
        {
            get
            {
                // Grab a random item:
                foreach (T item in _members.Keys)
                {
                    return item;
                }

                // Or if there wasn't one, throw an exception:
                throw new InvalidOperationException(
                    "There must be at least one item in the set for this operation.");
            }
        }

        /// <summary>
        /// Maps the set through a function.
        /// </summary>
        /// <typeparam name="TOutput">The type of elements in the resulting set.</typeparam>
        /// <param name="converter">The function to map the set through.</param>
        /// <returns>The resulting set.</returns>
        /// <remarks>If the function is not one to one, the cadinality of the resulting 
        /// set may differ from this set; this is why the function is not called "ConvertAll", despite
        /// being similar to the same method on lists.</remarks>
        public Set<TOutput> Map<TOutput>(Converter<T, TOutput> converter)
        {
            Set<TOutput> resultSet = new Set<TOutput>();

            foreach (T element in _members.Keys)
            {
                resultSet.UnionUpdate(converter(element));
            }

            return resultSet;
        }

        public TOutput Fold<TOutput>(Folder<T, TOutput> folder, TOutput initial)
        {
            TOutput accumulator = initial;

            foreach (T element in _members.Keys)
            {
                accumulator = folder(element, accumulator);
            }

            return accumulator;
        }

        public Set<T> Filter(Predicate<T> predicate)
        {
            Set<T> resultSet = new Set<T>();

            foreach (T element in _members.Keys)
            {
                if (predicate(element))
                {
                    resultSet.UnionUpdate(element);
                }
            }

            return resultSet;
        }
    }

    public delegate TAccumulated Folder<TInput, TAccumulated>(TInput input, TAccumulated accumulated);
}
