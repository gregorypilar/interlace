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

namespace DefaultNamespace.Tests
{
	[TestFixture]
	public class PointTests
	{
		[Test]
		public void TestWalk()
		{
			Position a = new Position(10, 100);
			Position b = new Position(10 + 2, 100 + 1);
			
			Position c = a.GetPointFromTargetAndDistance(b, Math.Sqrt(5) * 2);
            Assert.AreEqual(10 + 4, c.X, 0.0001);
			Assert.AreEqual(100 + 2, c.Y, 0.0001);
		}

        [Test]
        public void TestSpecificDistance()
        {
            Position a = new Position(121.71597, -33.516224000000001);
            Position b = new Position(121.716843, -33.517730999999998);

            double distance = Position.CalculateDistance(a, b, Ellipsoid.WGS84);
        }

		[Test]
		public void TestDistance()
		{
			Assert.AreEqual(1 / 298.257223563, Ellipsoid.WGS84.F, 0.0000001);
			
			Position a = new Position(150, -30);
			Position b = new Position(150, -31);
			Position c = new Position(151, -31);
			Position d = new Position(151, -32);
			Position e = new Position(151, -33);
			Position f = new Position(151, -34);
			Position g = new Position(151, -35);
			Position h = new Position(151, -40);
			Position i = new Position(151, -50);
			Position j = new Position(151, -60);
			Position k = new Position(151, -70);
			Position l = new Position(151, -80);
			
			Position lh = new Position(-90, 0);
			Position zero = new Position(0, 0);
			Position rh = new Position(90, 0);
			
			Assert.AreEqual(Ellipsoid.WGS84.A * Math.PI / 2, 
				Position.CalculateDistance(zero, rh, Ellipsoid.WGS84), 1);
			
			Assert.AreEqual(Double.NaN, 
				Position.CalculateDistance(lh, rh, Ellipsoid.WGS84));
			
			Assert.AreEqual(0.0, Position.CalculateDistance(a, a, Ellipsoid.WGS84), 1);
			Assert.AreEqual(110861, Position.CalculateDistance(a, b, Ellipsoid.WGS84), 1);
			Assert.AreEqual(146647, Position.CalculateDistance(a, c, Ellipsoid.WGS84), 1);
			Assert.AreEqual(241428, Position.CalculateDistance(a, d, Ellipsoid.WGS84), 1);
			Assert.AreEqual(345929, Position.CalculateDistance(a, e, Ellipsoid.WGS84), 1);
			Assert.AreEqual(453493, Position.CalculateDistance(a, f, Ellipsoid.WGS84), 1);
			Assert.AreEqual(562376, Position.CalculateDistance(a, g, Ellipsoid.WGS84), 1);
			Assert.AreEqual(1113142, Position.CalculateDistance(a, h, Ellipsoid.WGS84), 1);
			Assert.AreEqual(2222323, Position.CalculateDistance(a, i, Ellipsoid.WGS84), 1);
			Assert.AreEqual(3334804, Position.CalculateDistance(a, j, Ellipsoid.WGS84), 1);
			Assert.AreEqual(4449317, Position.CalculateDistance(a, k, Ellipsoid.WGS84), 1);
			Assert.AreEqual(5565218, Position.CalculateDistance(a, l, Ellipsoid.WGS84), 1);
			
			Assert.AreEqual(5565218, Position.CalculateDistance(l, a, Ellipsoid.WGS84), 1);
			Assert.AreEqual(110861, Position.CalculateDistance(b, a, Ellipsoid.WGS84), 1);
		}
	}
}
