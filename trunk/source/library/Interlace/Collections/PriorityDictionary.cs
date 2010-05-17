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

using Interlace.Mathematics;
using Interlace.Utilities;

#endregion

namespace Interlace.Collections
{
    public class PriorityDictionary<K, V> 
        where V : IComparable
    {
        Pair<K, V>[] _elements;
        int _elementsUsed;
        Dictionary<K, int> _indiciesByKey;

        public PriorityDictionary()
            : this(16)
        {
        }

        public PriorityDictionary(int capacity)
        {
            _elements = new Pair<K, V>[BitTricks.NextPowerOfTwo(capacity)];
            _elementsUsed = 0;
            _indiciesByKey = new Dictionary<K, int>(capacity);
        }

        public int Capacity
        {
            get 
            { 
                return _elements.Length; 
            }

            set
            {
                ReallocateToCapacity(value);
            }
        }

        void ReallocateToCapacity(int newCapacity)
        {
            if (newCapacity < _elementsUsed)
            {
                throw new ArgumentOutOfRangeException(
                    "The capacity of a heap can not be less than the count.");
            }

            Pair<K, V>[] newElements = new Pair<K, V>[newCapacity];
            Array.Copy(_elements, newElements, _elementsUsed);

            _elements = newElements;
        }

        void EnsureThereIsCapacityForAdd()
        {
            if (_elementsUsed < Capacity) return;

            int roundedOldCapacity = BitTricks.NextPowerOfTwo(Capacity);
            int newCapacity = roundedOldCapacity * 2;

            ReallocateToCapacity(newCapacity);
        }

        public int Count
        {
            get { return _elementsUsed; }
        }

        void Swap(int i, int j)
        {
            Pair<K, V> temporary = _elements[i];
            _elements[i] = _elements[j];
            _elements[j] = temporary;

            _indiciesByKey[_elements[i].First] = i;
            _indiciesByKey[_elements[j].First] = j;
        }

        void Heapify(int initialRoot)
        {
            int root = initialRoot;

            while (true)
            {
                int left = 2 * root + 1;
                int right = 2 * root + 2;

                int smallest;

                smallest = left < _elementsUsed && _elements[left].Second.CompareTo(_elements[root].Second) < 0 ? left : root;
                smallest = right < _elementsUsed && _elements[right].Second.CompareTo(_elements[smallest].Second) < 0 ? right : smallest;

                if (smallest == root) break;

                Swap(root, smallest);

                root = smallest;
            }
        }

        public Pair<K, V> Peek()
        {
            if (_elementsUsed < 1) throw new InvalidOperationException(
                "Elements can not be peeked at from an empty queue.");

            return _elements[0];
        }

        public K PeekKey()
        {
            return _elements[0].First;
        }

        public V PeekValue()
        {
            return _elements[0].Second;
        }

        public Pair<K, V> Dequeue()
        {
            if (_elementsUsed < 1) throw new InvalidOperationException(
                "Elements can not be dequeued from an empty queue.");

            Pair<K, V> element = _elements[0];
            _indiciesByKey.Remove(element.First);

            _elements[0] = _elements[_elementsUsed - 1];
            _indiciesByKey[_elements[0].First] = 0;
            _elementsUsed--;

            // Avoid keeping references in the unused array slots for the garbage collector:
            _elements[_elementsUsed] = new Pair<K, V>(default(K), default(V));

            Heapify(0);

            return element;
        }

        public V GetValue(K key, V defaultValue)
        {
            if (_indiciesByKey.ContainsKey(key))
            {
                return _elements[_indiciesByKey[key]].Second;
            }
            else
            {
                return defaultValue;
            }
        }

        public V this[K key]
        {
            set
            {
                int i;

                if (_indiciesByKey.ContainsKey(key))
                {
                    i = _indiciesByKey[key];

                    _elements[i] = new Pair<K, V>(key, value);
                }
                else
                {
                    EnsureThereIsCapacityForAdd();

                    // Add the element at the end, assuming for now that it is the largest:
                    _elements[_elementsUsed] = new Pair<K, V>(key, value);
                    _indiciesByKey[key] = _elementsUsed;
                    _elementsUsed++;

                    i = _elementsUsed - 1;
                }

                // Pretend to increase the element to the real value, and let it bubble up:
                while (i > 0 && _elements[(i - 1) / 2].Second.CompareTo(_elements[i].Second) > 0)
                {
                    Swap(i, (i - 1) / 2);

                    i = (i - 1) / 2;
                }
            }
        }

        void RemoveAt(int i)
        {
            while (i > 0)
            {
                Swap(i, (i - 1) / 2);

                i = (i - 1) / 2;
            }

            Dequeue();
        }

        public void Clear()
        {
            // Avoid keeping references in the unused array slots for the garbage collector:
            for (int i = 0; i < _elementsUsed; i++)
            {
                _elements[i] = new Pair<K, V>(default(K), default(V));
            }

            _elementsUsed = 0;
            _indiciesByKey.Clear();
        }
    }
}
