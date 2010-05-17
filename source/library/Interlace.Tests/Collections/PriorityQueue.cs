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
using System.Reflection;
using System.Text;

using MbUnit.Framework;

using Interlace.Collections;

#endregion

namespace Interlace.Tests.Collections
{
    [TestFixture]
    public class PriorityQueue
    {
        public void AssertDequeues(PriorityQueue<int> queue, params int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                Assert.AreEqual(values.Length - i, queue.Count);

                Assert.AreEqual(values[i], queue.Peek());
                Assert.AreEqual(values[i], queue.Peek());
                Assert.AreEqual(values[i], queue.Dequeue());

                Assert.AreEqual(values.Length - i - 1, queue.Count);

                AssertQueueProperty(queue);
            }
        }

        public void AssertQueueProperty(PriorityQueue<int> queue)
        {
            Type type = queue.GetType();

            FieldInfo elementsField = type.GetField("_elements", BindingFlags.NonPublic | BindingFlags.Instance);
            int[] elements = (int[])elementsField.GetValue(queue);

            FieldInfo elementsUsedField = type.GetField("_elementsUsed", BindingFlags.NonPublic | BindingFlags.Instance);
            int elementsUsed = (int)elementsUsedField.GetValue(queue);

            for (int i = 1; i < elementsUsed; i++)
            {
                int parent = (i - 1) / 2;

                if (elements[parent] > elements[i])
                {
                    Assert.Fail("Heap property violated.");
                }
            }
        }

        [Test]
        public void SimpleTests()
        {
            PriorityQueue<int> queue = new PriorityQueue<int>();

            Assert.AreEqual(0, queue.Count);
            queue.Enqueue(5);
            queue.Enqueue(10);
            queue.Enqueue(6);
            queue.Enqueue(1);
            queue.Enqueue(20);
            AssertQueueProperty(queue);
            Assert.AreEqual(5, queue.Count);

            AssertDequeues(queue, 1, 5, 6, 10, 20);
        }

        [Test]
        public void TestWithRandomData()
        {
            PriorityQueue<int> queue = new PriorityQueue<int>();

            byte lfsr = 1;

            List<int> sortedValues = new List<int>();

            for (int i = 0; i < 255; i++)
            {
                if ((lfsr & 1) == 1)
                {
                    lfsr ^= (byte)((lfsr >> 1) | 0xc1);
                }
                else
                {
                    lfsr >>= 1;
                }

                queue.Enqueue((int)lfsr);
                sortedValues.Add((int)lfsr);

                AssertQueueProperty(queue);
            }

            sortedValues.Sort();

            AssertDequeues(queue, sortedValues.ToArray());
        }
    }
}
