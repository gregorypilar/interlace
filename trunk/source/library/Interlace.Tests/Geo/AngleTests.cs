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
	public class AngleTests
	{
		[Test]
		public void TestAngle()
		{
			Assert.AreEqual(90, (new Angle(0.0)).HeadingInDegrees);
			Assert.AreEqual(45, (new Angle(Math.PI / 4)).HeadingInDegrees);
			Assert.AreEqual(0, (new Angle(Math.PI / 2)).HeadingInDegrees);

			Assert.AreEqual(180, (new Angle(Math.PI * 3)).AngleInDegrees);
			Assert.AreEqual(270, (new Angle(-Math.PI / 2)).AngleInDegrees);

			Assert.AreEqual(0.1, (new Angle(0.1)).AngleInRadians);
			Assert.AreEqual(Math.PI / 2 - 0.1, (new Angle(0.1)).HeadingInRadians);

			for (int i = 0; i < 360; i += 5) {
				Assert.AreEqual((double)i, Angle.FromAngleInDegrees(i).AngleInDegrees, 0.000001);
				Assert.AreEqual((double)i, Angle.FromHeadingInDegrees(i).HeadingInDegrees, 0.000001);
			}
		}

		[Test]
		public void TestAngleStrings()
		{
			Assert.AreEqual("North", Angle.FromHeadingInDegrees(1).ToCompassString());
			Assert.AreEqual("South", Angle.FromHeadingInDegrees(170).ToCompassString());
			Assert.AreEqual("East", Angle.FromHeadingInDegrees(77).ToCompassString());
			Assert.AreEqual("West", Angle.FromHeadingInDegrees(265).ToCompassString());

			Assert.AreEqual("North West", Angle.FromHeadingInDegrees(-45).ToCompassString());
			Assert.AreEqual("North East", Angle.FromHeadingInDegrees(45).ToCompassString());
			Assert.AreEqual("South East", Angle.FromHeadingInDegrees(130).ToCompassString());
			Assert.AreEqual("South West", Angle.FromHeadingInDegrees(226).ToCompassString());
		}

		[Test]
		public void TestTurnStrings()
		{
			Assert.AreEqual("Continue", Angle.FromAngleInDegrees(-4).ToTurnString());
			Assert.AreEqual("Continue", Angle.FromAngleInDegrees(4).ToTurnString());
			Assert.AreEqual("Curve Left", Angle.FromAngleInDegrees(-6).ToTurnString());
			Assert.AreEqual("Curve Right", Angle.FromAngleInDegrees(6).ToTurnString());
			Assert.AreEqual("Turn Right", Angle.FromAngleInDegrees(70).ToTurnString());
			Assert.AreEqual("Turn Left", Angle.FromAngleInDegrees(-70).ToTurnString());
			Assert.AreEqual("Reverse", Angle.FromAngleInDegrees(-180).ToTurnString());
		}
	}
}
