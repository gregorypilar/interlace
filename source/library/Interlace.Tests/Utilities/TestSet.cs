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

using Interlace.Collections;

#endregion

namespace Interlace.Tests.Utilities
{
    [TestFixture]
    public class TestSet
    {
        [Test]
        public void SimpleValueTests()
        {
            Set<int> empty = new Set<int>();
            Set<int> a = new Set<int>(1, 2, 3);
            Set<int> b = new Set<int>(3, 4, 5);

            Set<int> a_and_b = a + b;

            Assert.AreEqual("{}", empty.ToString());
            Assert.AreEqual("{ 1, 2, 3 }", a.ToString());
            Assert.AreEqual("{ 3, 4, 5 }", b.ToString());

            Assert.AreEqual("{ 1, 2, 3, 4, 5 }", a_and_b.ToString());

            Assert.AreEqual("{}", (empty * a).ToString());
            Assert.AreEqual("{}", (a * empty).ToString());

            Assert.IsTrue(a_and_b * a == a);
            Assert.IsTrue((a_and_b * a).Equals(a));

            Assert.IsTrue(a_and_b - a == new Set<int>(4, 5));
            Assert.IsTrue(a - a == new Set<int>());
            Assert.IsTrue(a * a == a);

            Assert.IsTrue(a.IsSubsetOf(a_and_b));
            Assert.IsTrue(b.IsSubsetOf(a_and_b));
            Assert.IsTrue(a_and_b.IsSubsetOf(a_and_b));

            Assert.IsTrue(a.IsSubsetOf(a));
            Assert.IsTrue(b.IsSubsetOf(b));

            Assert.IsFalse(a.IsSubsetOf(b));
            Assert.IsFalse(b.IsSubsetOf(a));
            Assert.IsFalse(a_and_b.IsSubsetOf(a));
            Assert.IsFalse(a_and_b.IsSubsetOf(b));

            Set<int> a_copy = a.Copy();
            Set<int> b_copy = b.Copy();

            a.UnionUpdate(b);
            Assert.AreEqual(a, a_and_b);

            a.DifferenceUpdate(b);
            Assert.AreEqual(a, a_and_b - b);

            a.UnionUpdate(9);
            Assert.AreEqual(a, (a_and_b - b) + new Set<int>(9));

            a.DifferenceUpdate(1);
            Assert.AreEqual(a, (a_and_b - b) + new Set<int>(9) - new Set<int>(1));

            a_copy.IntersectionUpdate(b_copy);
            Assert.AreEqual(new Set<int>(3), a_copy);
        }
    }
}
