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

using MbUnit.Framework;

using Interlace.Geo;

#endregion

namespace GeoTests
{
	[TestFixture]
	public class PolylineTests
	{
		[Test]
		public void TestSimplify()
		{
			Polyline p = new Polyline();
			Assert.AreEqual(0, p.Simplify(1.0).Length);
			
			p = new Polyline();
			p.Add(new Position(0, 0));
			p.Add(new Position(0, 1));
			p.Add(new Position(10, 1));
			p.Add(new Position(2, 0));
			
			Assert.AreEqual(4, p.Simplify(0.0).Length);
			Assert.AreEqual(4, p.Simplify(0.5).Length);
			Assert.AreEqual(3, p.Simplify(2).Length);
			Assert.AreEqual(2, p.Simplify(20).Length);
			
			Random seed = new Random();
			p = new Polyline();
			for (int i = 0; i < 6000; i++) 
			{
				p.Add(new Position(seed.NextDouble(), seed.NextDouble()));
			}
			Polyline o = p.Simplify(0.01);
		}

		[Test]
		public void AddTest()
		{
			Polyline a = new Polyline();
			a.Add(new Position(1, 1));
			a.Add(new Position(1, 2));

			Polyline b = new Polyline();
			b.Add(new Position(1, 3));

			a.AddPolyline(b);

			Assert.AreEqual(1, a[0].Y);
			Assert.AreEqual(2, a[1].Y);
			Assert.AreEqual(3, a[2].Y);

			Assert.AreEqual(3, b[0].Y);
		}

		[Test]
		public void ReverseTest()
		{
			Polyline a = new Polyline();
			a.Add(new Position(1, 1));
			a.Add(new Position(1, 2));

			Polyline b = a.Reversed;

			Assert.AreEqual(2, b[0].Y);
			Assert.AreEqual(1, b[1].Y);

			Assert.AreEqual(1, a[0].Y);
			Assert.AreEqual(2, a[1].Y);
		}

        [Test]
        public void TestArea()
        {
            Polyline triangle = new Polyline();

            triangle.Add(new Position(1, 1));
            triangle.Add(new Position(1, 2));
            triangle.Add(new Position(2, 1));
            triangle.Add(new Position(1, 1));

            Assert.AreEqual(0.5, triangle.Area, Double.Epsilon);
            
            Polyline arrow = new Polyline();

            arrow.Add(new Position(1, 1));
            arrow.Add(new Position(1, 4));
            arrow.Add(new Position(2, 3));
            arrow.Add(new Position(3, 4));
            arrow.Add(new Position(4, 3));
            arrow.Add(new Position(3, 2));
            arrow.Add(new Position(4, 1));

            Assert.AreEqual(6.5, arrow.Area, Double.Epsilon);
            Assert.AreEqual(6.5, arrow.Reversed.Area, Double.Epsilon);
        }
	}
}

