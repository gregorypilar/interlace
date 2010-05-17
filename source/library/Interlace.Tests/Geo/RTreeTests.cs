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

using MbUnit.Framework;

using Interlace.Geo;
using Interlace.Geo.RTree;

#endregion

namespace GeoTests
{
    [TestFixture]
    class RTreeTests
    {
        private void AssertSetIsEqual(IEnumerable<int> findResult, params int[] expectedBoxNumbers)
        {
            Dictionary<int, bool> expected = new Dictionary<int,bool>();

            foreach (int boxNumber in expectedBoxNumbers)
            {
                Assert.IsFalse(expected.ContainsKey(boxNumber));

                expected[boxNumber] = true;
            }

            int itemsFound = 0;

            foreach (int boxNumber in findResult)
            {
                Assert.IsTrue(expected.ContainsKey(boxNumber));
                Assert.IsTrue(expected[boxNumber]);

                expected[boxNumber] = false;

                itemsFound += 1;
            }

            Assert.AreEqual(expectedBoxNumbers.Length, itemsFound, String.Format(
                "The number of expected items was {0}, but {1} were found.",
                expectedBoxNumbers.Length, itemsFound));
                
        }

        [Test]
        public void TestSimpleInserts()
        {
            RTree tree = new RTree(4, 2);

            tree.Insert<int>(new Box(0, 0, 10, 10), 1); tree.ThrowOnInvariantsViolated();

            tree.Insert<int>(new Box(20, 0, 30, 10), 2); tree.ThrowOnInvariantsViolated();

            AssertSetIsEqual(tree.Find<int>(new Box(-5, -5, 5, 5)), 1);

            // Cause the first split:
            tree.Insert<int>(new Box(20, 0, 30, 10), 3); tree.ThrowOnInvariantsViolated();
            tree.Insert<int>(new Box(20, 20, 30, 30), 4); tree.ThrowOnInvariantsViolated();

            tree.Insert<int>(new Box(40, 0, 50, 30), 5); tree.ThrowOnInvariantsViolated();

            AssertSetIsEqual(tree.Find<int>(new Box(-5, -5, 5, 5)), 1);

            AssertSetIsEqual(tree.Find<int>(new Box(5, 5, 25, 25)), 1, 2, 3, 4);
            AssertSetIsEqual(tree.Find<int>(new Box(-1000, -1000, 1000, 1000)), 1, 2, 3, 4, 5);

            // Cause another set of splits:
            for (int i = 0; i < 1000; i++)
            {
                tree.Insert<int>(new Box(100 + i * 10, 0, 100 + i * 10 + 10, 10), 6 + i);
                tree.ThrowOnInvariantsViolated();
            }

            AssertSetIsEqual(tree.Find<int>(new Box(-5, -5, 5, 5)), 1);

            AssertSetIsEqual(tree.Find<int>(new Box(5, 5, 25, 25)), 1, 2, 3, 4);
            AssertSetIsEqual(tree.Find<int>(new Box(-1000, -1000, 45, 1000)), 1, 2, 3, 4, 5);
        }
    }
}
