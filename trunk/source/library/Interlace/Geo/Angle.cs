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

#endregion

namespace Interlace.Geo
{
	/// <summary>
	/// A value type to represent angles. Angles are constructed and stored in
	/// radians, using the cartesian convertion. 
	/// </summary>
	/// 
	/// Trigonomic functions such as sin() and cos() will then work with 
	/// angles correctly.
	/// 
	/// There are two units that can be used to store angles:
	/// 
	///   * Degrees
	///   * Radians
	///   
	/// There are also two conventions for directions:
	/// 
	///   * Cartesian; with 0.0 to the right, and angles increasing anti-clockwise.
	///   * Compass; with 0 degrees (North) towards the top, and angles 
	///     increasing clockwise.
	///     
	public struct Angle 
	{
		double _angle;

		public Angle(double cartesianRadians)
		{
			_angle = cartesianRadians;

			// Store the angle, adjusted so that 0 <= a <= 2pi.
			while (_angle < 0) _angle += 2 * Math.PI;
			while (_angle >= 2 * Math.PI) _angle -= 2 * Math.PI;
		}

		public static Angle operator -(Angle lhs, Angle rhs)
		{
			return new Angle(lhs._angle - rhs._angle);
		}

		public static Angle operator +(Angle lhs, Angle rhs)
		{
			return new Angle(lhs._angle + rhs._angle);
		}

		public static Angle FromAngleInDegrees(double angleDegrees)
		{
			return new Angle(Math.PI * angleDegrees / 180.0);
		}

		public static Angle FromHeadingInDegrees(double angleDegrees)
		{
			return FromAngleInDegrees(-angleDegrees + 90);
		}

		public double HeadingInRadians
		{
			get {
				double heading = -_angle + Math.PI / 2;
				while (heading < 0) heading += 2 * Math.PI;
				while (heading >= 2 * Math.PI) heading -= 2 * Math.PI;

				return heading;
			}
		}

		public double HeadingInDegrees
		{
			get { return 180.0 * HeadingInRadians / Math.PI; }
		}

		public double AngleInRadians
		{
			get { return _angle; }
		}

		public double AngleInDegrees
		{
			get { return 180.0 * _angle / Math.PI; }
		}

		public string ToTurnString()
		{
			double angle = AngleInDegrees;
			
			if (angle < 5) return "Continue";
			if (angle < 50) return "Curve Left";
			if (angle < 180 - 20) return "Turn Left";
			if (angle < 180 + 20) return "Reverse";
			if (angle < 360 - 50) return "Turn Right";
			if (angle < 360 - 5) return "Curve Right";
			return "Continue";
		}

		public string ToCompassString()
		{
			double heading = HeadingInDegrees;
			
			if (heading < 22.5 + 0 * 45) return "North";
			if (heading < 22.5 + 1 * 45) return "North East";
			if (heading < 22.5 + 2 * 45) return "East";
			if (heading < 22.5 + 3 * 45) return "South East";
			if (heading < 22.5 + 4 * 45) return "South";
			if (heading < 22.5 + 5 * 45) return "South West";
			if (heading < 22.5 + 6 * 45) return "West";
			if (heading < 22.5 + 7 * 45) return "North West";
			return "North";
		}		
	}
}
