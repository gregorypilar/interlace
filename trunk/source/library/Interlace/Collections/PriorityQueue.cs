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

#endregion

namespace Interlace.Collections
{
    public class PriorityQueue<T> where T : IComparable
    {
        T[] _elements;
        int _elementsUsed;

        public PriorityQueue()
            : this(16)
        {
        }

        public PriorityQueue(int capacity)
        {
            _elements = new T[BitTricks.NextPowerOfTwo(capacity)];
            _elementsUsed = 0;
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

            T[] newElements = new T[newCapacity];
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

        public void TrimToSize()
        {
            Capacity = Count;
        }

        public int Count
        {
            get { return _elementsUsed; }
        }

        void Heapify(int initialRoot)
        {
            int root = initialRoot;

            while (true)
            {
                int left = 2 * root + 1;
                int right = 2 * root + 2;

                int smallest;

                smallest = left < _elementsUsed && _elements[left].CompareTo(_elements[root]) < 0 ? left : root;
                smallest = right < _elementsUsed && _elements[right].CompareTo(_elements[smallest]) < 0 ? right : smallest;

                if (smallest == root) break;

                T temporary = _elements[root];
                _elements[root] = _elements[smallest];
                _elements[smallest] = temporary;

                root = smallest;
            }
        }

        public T Peek()
        {
            if (_elementsUsed < 1) throw new InvalidOperationException(
                "Elements can not be peeked at from an empty queue.");

            return _elements[0];
        }

        public T Dequeue()
        {
            if (_elementsUsed < 1) throw new InvalidOperationException(
                "Elements can not be dequeued from an empty queue.");

            T element = _elements[0];
            _elements[0] = _elements[_elementsUsed - 1];
            _elementsUsed--;

            // Avoid keeping references in the unused array slots for the garbage collector:
            _elements[_elementsUsed] = default(T);

            Heapify(0);

            return element;
        }

        public void Enqueue(T element)
        {
            EnsureThereIsCapacityForAdd();

            // Add the element at the end, assuming for now that it is the largest:
            _elements[_elementsUsed] = element;
            _elementsUsed++;

            int i = _elementsUsed - 1;

            // Pretend to increase the element to the real value, and let it bubble up:
            while (i > 0 && _elements[(i - 1) / 2].CompareTo(_elements[i]) > 0)
            {
                T temporary = _elements[i];
                _elements[i] = _elements[(i - 1) / 2];
                _elements[(i - 1) / 2] = temporary;

                i = (i - 1) / 2;
            }
        }

        public bool Remove(T element)
        {
            for (int i = 0; i < _elementsUsed; i++)
            {
                if (object.Equals(element, _elements[i]))
                {
                    RemoveAt(i);

                    return true;
                }
            }

            return false;
        }

        void RemoveAt(int i)
        {
            while (i > 0)
            {
                T temporary = _elements[i];
                _elements[i] = _elements[i / 2];
                _elements[(i - 1) / 2] = temporary;

                i = (i - 1) / 2;
            }

            Dequeue();
        }

        public void Clear()
        {
            _elementsUsed = 0;
        }
    }
}
