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
	/// <summary>
	/// Summary description for RedfearnTests.
	/// </summary>
	[TestFixture]
	public class RedfearnTests
	{
		[Test]
		public void TestZones()
		{
			Projection p = Projection.MGA94;

			// Test the Perth zone, and its edges:
			Assert.AreEqual(50, Redfearn.GetZoneFromLongitude(p, 114));
			Assert.AreEqual(50, Redfearn.GetZoneFromLongitude(p, 114.1));
			Assert.AreEqual(50, Redfearn.GetZoneFromLongitude(p, 119.999));
			Assert.AreEqual(51, Redfearn.GetZoneFromLongitude(p, 120));

			// Test for Sydney:
			Assert.AreEqual(56, Redfearn.GetZoneFromLongitude(p, 151));

			// Test the edges of the globe:
			Assert.AreEqual(1, Redfearn.GetZoneFromLongitude(p, -179));
			Assert.AreEqual(60, Redfearn.GetZoneFromLongitude(p, 178));

			// Test around the equator:
			Assert.AreEqual(31, Redfearn.GetZoneFromLongitude(p, 0.0));
			Assert.AreEqual(31, Redfearn.GetZoneFromLongitude(p, 0.1));
			Assert.AreEqual(30, Redfearn.GetZoneFromLongitude(p, -0.1));
		}
	}
}
