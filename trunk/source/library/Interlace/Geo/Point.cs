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
    [Serializable]
	public struct Position
	{
		public double Longitude, Latitude;
		
		public static Position FromDegrees(double longitude, double latitude)
		{
			return new Position(Math.PI * longitude / 180.0,
			                 Math.PI * latitude / 180.0);
		}
		
		public void ToDegrees(out double longitude, out double latitude)
		{
			longitude = 180.0 * this.Longitude / Math.PI;
			latitude = 180.0 * this.Latitude / Math.PI;
		}		

		public Position ToDegrees()
		{
			return new Position(180.0 * this.Longitude / Math.PI, 180.0 * this.Latitude / Math.PI);
		}

		public Position ToRadians()
		{
			return new Position(Math.PI * this.Longitude / 180.0, Math.PI * this.Latitude / 180.0);
		}

		public Position(double longitude, double latitide)
		{
			this.Longitude = longitude;
			this.Latitude = latitide;
		}

		public double X {
			get 
            {
				return Longitude;
			}
            set
            {
                Longitude = value;
            }
		}
		
		public double Y {
			get 
            {
				return Latitude;
			}
            set
            {
                Latitude = value;
            }
		}
		
		public static Position operator - (Position lhs, Position rhs)
		{
			return new Position(lhs.X - rhs.X, lhs.Y - rhs.Y);
		}

		public static Position operator + (Position lhs, Position rhs)
		{
			return new Position(lhs.X + rhs.X, lhs.Y + rhs.Y);
		}

		public static double DotProduct(Position lhs, Position rhs)
		{
			return lhs.X * rhs.X + lhs.Y * rhs.Y;
		}
		
		public static double SquaredDistance(Position lhs, Position rhs)
		{
			return (lhs.X - rhs.X) * (lhs.X - rhs.X) +
				(lhs.Y - rhs.Y) * (lhs.Y - rhs.Y);
		}
		
		public double SquaredDistanceFromOrigin()
		{
			return X * X + Y * Y;
		}

		public Position GetPointFromTargetAndDistance(Position target, double distance)
		{
			Position delta = target - this;
            double delta_length = Math.Sqrt(delta.SquaredDistanceFromOrigin());

			double udx = delta.X / delta_length;
			double udy = delta.Y / delta_length;

			return new Position(this.X + udx * distance, this.Y + udy * distance);
		}
		
		private static double Radians(double d)
		{
			return Math.PI * d / 180;
		}
	
		public static bool PointsEqual(Position a, Position b, double tolerance)
		{
			return Math.Abs(a.Latitude - b.Latitude) <= tolerance &&
			       Math.Abs(a.Longitude - b.Longitude) <= tolerance;
		}
		
		public static double CartesianDistance(Position a, Position b)
		{
			return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
		}

		// Calculates the distance between two points, using the method
		// described in:
		//
		//     Vincenty, T. Direct and Inverse Solutions of Geodesics of the
		//       Ellipsoid with Applications of Nested Equations, Survey Review,
		//       No. 176, 1975.
		//
		// Points that are nearly antipodal yield no solution; a 
		// Double.NaN is returned.
		
		public static double CalculateDistance(Position a, Position b, Ellipsoid e)
		{
			if (PointsEqual(a, b, 1e-12)) return 0.0;
			
			double lambda = Radians(b.Longitude - a.Longitude);
			double last_lambda = 2 * Math.PI;
			
			double U1 = Math.Atan((1 - e.F) * Math.Tan(Radians(a.Latitude)));
			double U2 = Math.Atan((1 - e.F) * Math.Tan(Radians(b.Latitude)));

			double sin_U1 = Math.Sin(U1);
			double sin_U2 = Math.Sin(U2);
			double cos_U1 = Math.Cos(U1);
			double cos_U2 = Math.Cos(U2);
			
			double alpha, sin_sigma, cos_sigma, sigma, cos_2sigma_m, sqr_cos_2sigma_m;
			
			int loop_limit = 30;
		
			const double threshold = 1e-12;
			
			do {
				double sin_lambda = Math.Sin(lambda);
				double cos_lambda = Math.Cos(lambda);
				
		       	sin_sigma = 
		       	  Math.Sqrt(
		       		   Math.Pow(cos_U2 * sin_lambda, 2) +
		       		   Math.Pow(cos_U1 * sin_U2 - sin_U1 * cos_U2 * cos_lambda, 2)
		       		  );
		       	
		       	cos_sigma = sin_U1 * sin_U2 + cos_U1 * cos_U2 * cos_lambda;
		       	sigma = Math.Atan2(sin_sigma, cos_sigma);
		       	alpha = Math.Asin(cos_U1 * cos_U2 * sin_lambda / sin_sigma);
		       	double sqr_cos_alpha = Math.Pow(Math.Cos(alpha), 2);
		       	cos_2sigma_m = cos_sigma - 2 * sin_U1 * sin_U2 / sqr_cos_alpha;
				sqr_cos_2sigma_m = Math.Pow(cos_2sigma_m, 2);
		       	double C = e.F / 16 * sqr_cos_alpha * (4 + e.F * (4 - 3 * sqr_cos_alpha));
		       	
		       	last_lambda = lambda;
		       	lambda =
		       		Radians(b.Longitude - a.Longitude) + (1 - C) * e.F * Math.Sin(alpha) *
		       		(sigma + C * sin_sigma * 
			       		(cos_2sigma_m + C * cos_sigma * (-1 + 2 * sqr_cos_2sigma_m))
			       	);
		       	
		       	loop_limit -= 1;
			} while (Math.Abs(lambda - last_lambda) > threshold && loop_limit > 0);
			
			// As in Vincenty 1975, "The inverse formula may give no 
			// solution over a line between two nearly antipodal points":
			if (loop_limit == 0) return Double.NaN;
			
			double sqr_u = Math.Pow(Math.Cos(alpha), 2) * 
				(e.A * e.A - e.B * e.B) / (e.B * e.B);
			double A = 1 + sqr_u / 16384 * (4096 + sqr_u * (-768 + sqr_u * (320 - 175 * sqr_u)));
			double B = sqr_u / 1024 * (256 + sqr_u * (-128 + sqr_u * (74 - 47 * sqr_u)));
			double delta_sigma = 
				B * sin_sigma * 
				(cos_2sigma_m + B / 4 *
				 (cos_sigma * (-1 + 2 * sqr_cos_2sigma_m) -
					B / 6 *	cos_2sigma_m * (-3 + 4 * Math.Pow(sin_sigma, 2)) * 
						(-3 + 4 * sqr_cos_2sigma_m)
				));
			                                       
			return e.B * A * (sigma - delta_sigma);
		}

		private static void TotalDegreesToDegreesMinutesSeconds(double totalDegrees, 
			out int degrees, out int minutes, out double seconds)
		{
			degrees = (int)totalDegrees;
			double totalMinutes = (Math.Abs(totalDegrees - degrees)) * 60.0;
			minutes = (int)totalMinutes;
			seconds = (Math.Abs(totalMinutes - minutes)) * 60.0;
		}

		private static string TotalDegreesToString(double totalDegrees, string positiveHemisphere, 
			string negativeHemisphere)
		{
			int degrees, minutes;
			double seconds;

			TotalDegreesToDegreesMinutesSeconds(totalDegrees, out degrees, out minutes, out seconds);

			return String.Format("{0}° {1}' {2:0.000}\" {3}", Math.Abs(degrees), minutes, 
				seconds, degrees >= 0.0 ? positiveHemisphere : negativeHemisphere);
		}

		public string LatitudeString
		{
			get 
			{
				return TotalDegreesToString(Latitude, "N", "S");
			}
		}

		public string LongitudeString
		{
			get 
			{
				return TotalDegreesToString(Longitude, "E", "W");
			}
		}

        public override bool Equals(object obj)
        {
            if (!(obj is Position)) return false;

            Position rhs = (Position)obj;

            return Longitude == rhs.Longitude && Latitude == rhs.Latitude;
        }

        public override int GetHashCode()
        {
            return Longitude.GetHashCode() ^ Latitude.GetHashCode();
        }

        public static bool operator==(Position lhs, Position rhs)
        {
            return lhs.Longitude == rhs.Longitude && lhs.Latitude == rhs.Latitude;
        }

        public static bool operator!=(Position lhs, Position rhs)
        {
            return lhs.Longitude != rhs.Longitude || lhs.Latitude != rhs.Latitude;
        }
	}
}
