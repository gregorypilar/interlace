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
	public class Redfearn
	{
		Ellipsoid ellipsoid;
		Projection projection;
		int zone;
		
		// Cached parameters:
		double lon_zero;
		
		double e_2;
		double md_coef0, md_coef2, md_coef4, md_coef6;
		
		public Redfearn(Ellipsoid ellipsoid, Projection projection, int zone)
		{
			this.ellipsoid = ellipsoid;
			this.projection = projection;
			this.zone = zone;
			
			double lon_zero_degrees = projection.first_lon + 
			  projection.zone_width / 2.0 + projection.zone_width * 
			  (zone - projection.first_zone);
			lon_zero = Math.PI * lon_zero_degrees / 180.0;
			
			PrecalculateMeridianDistance();
		}
		
		public static int GetZoneFromLongitude(Projection projection, double longitude)
		{
			double degrees_from_first = longitude - projection.first_lon;
			double zone = degrees_from_first / projection.zone_width;
			int zone_number = (int)Math.Floor(zone) + projection.first_zone;

			return zone_number;
		}

		void PrecalculateMeridianDistance()
		{
			double a_2 = Math.Pow(ellipsoid.A, 2);
			double b_2 = Math.Pow(ellipsoid.B, 2);

			e_2 = (a_2 - b_2) / a_2;
			double e_4 = Math.Pow(a_2 - b_2, 2) / Math.Pow(ellipsoid.A, 4);
			double e_6 = Math.Pow(a_2 - b_2, 3) / Math.Pow(ellipsoid.A, 6);

			md_coef0 = ellipsoid.A * (1.0 - (e_2 / 4.0) - (3.0 * e_4 / 64.0) - (5.0 * e_6 / 256.0));
			md_coef2 = ellipsoid.A * ((3.0 / 8.0) * (e_2 + e_4 / 4.0 + 15.0 * e_6 / 128.0));
			md_coef4 = ellipsoid.A * ((15.0 / 256.0) * (e_4 + 3.0 * e_6 / 4.0));
			md_coef6 = ellipsoid.A * (35.0 * e_6 / 3072.0);
		}
		
		double MeridianDistance(double latitude)
		{
			return md_coef0 * latitude - md_coef2 * Math.Sin(2.0 * latitude) + 
	  		  md_coef4 * Math.Sin(4.0 * latitude) - md_coef6 * Math.Sin(6.0 * latitude);
		}

		double FootPointLatitude(double northings)
		{
			double n = ellipsoid.F / (2.0 - ellipsoid.F);
			double n_2 = Math.Pow(n, 2);
			double n_3 = Math.Pow(n, 3);
			double n_4 = Math.Pow(n, 4);

			double G = ellipsoid.A * (1.0 - n) * (1.0 - n_2) * (1 + (9.0 / 4.0) * n_2 + 
	  		  (225.0 / 64.0) * n_4) * (Math.PI / 180.0);

			double sigma = (northings * Math.PI / projection.k0) / (180.0 * G);

			double lat_fp = sigma + ((3.0 * n / 2.0) - (27.0 * n_3 / 32.0)) * Math.Sin(2.0 * sigma) +
			  + ((21.0 * n_2 / 16.0) - (55.0 * n_4 / 32.0)) * Math.Sin(4.0 * sigma)
			  + (151.0 * n_3 / 96.0) * Math.Sin(6.0 * sigma)
			  + (1097.0 * n_4 / 512.0) * Math.Sin(8.0 * sigma);

			return lat_fp;
		}
		
		void RadiusOfCurvature(double latitude, out double rc, out double v, out double p)
		{
			double sinlat_2 = Math.Pow(Math.Sin(latitude), 2);
		
			v = ellipsoid.A / Math.Pow(1.0 - e_2 * sinlat_2, 0.5);
			p = ellipsoid.A * (1.0 - e_2) / Math.Pow(1.0 - e_2 * sinlat_2, 1.5);
			rc = v / p;
		}
		
		public Position GeoToGrid(Position geo)
		{
			double m = MeridianDistance(geo.Latitude);
			
			double rc, v, p;
			RadiusOfCurvature(geo.Latitude, out rc, out v, out p);
		
			double t = Math.Tan(geo.Latitude);
			double t_2 = Math.Pow(t, 2);
			double t_4 = Math.Pow(t, 4);
		
			double w = geo.Longitude - lon_zero;
			double w_2 = Math.Pow(w, 2);
			double w_4 = Math.Pow(w, 4);
			double w_6 = Math.Pow(w, 6);
		
			double cos_lat = Math.Cos(geo.Latitude);
			double sin_lat = Math.Sin(geo.Latitude);
			double rc_2 = Math.Pow(rc, 2);
			double t_6 = Math.Pow(t, 2);
			double cos_lat_3 = Math.Pow(cos_lat, 3);
		
			double term1 = (w_2 / 6.0) * Math.Pow(cos_lat, 2) * (rc - t_2);
			double term2 = (w_4 / 120.0) * Math.Pow(cos_lat, 4) * (4.0 * Math.Pow(rc, 3) * (1.0 - 6.0 * t_2) + 
			  rc_2 * (1.0 + 8.0 * t_2) - rc * 2 * t_2 + t_4);
			double term3 = (w_6 / 5040.0) * Math.Pow(cos_lat, 6) * (61 - 479 * t_2 + 179 * t_4 - t_6);
		
			double eastings = (projection.k0 * v * w * cos_lat) * (1.0 + term1 + term2 + term3) + projection.false_easting;
		
			term1 = (w_2 / 2.0) * v * sin_lat * cos_lat;
			term2 = (w_4 / 24.0) * v * sin_lat * cos_lat_3 * (4.0 * rc_2 + rc - t_2);
		
			term3 = (w_6 / 720.0) * v * sin_lat * Math.Pow(cos_lat, 5) * (8 * Math.Pow(rc, 4) * 
				(11.0 - 24.0 * t_2) - 28.0 * Math.Pow(rc, 3) * (1.0 - 6.0 * t_2) + rc_2 * (1.0 - 32.0 * 
				t_2) - rc * (2 * t_2) + t_4);
			double term4 = (Math.Pow(w, 8) / 40320.0) * v * sin_lat * Math.Pow(cos_lat, 7) * (1385.0 - 3111.0 * 
				t_2 + 543.0 * t_4 - Math.Pow(t, 6));
			
			double northings = projection.k0 * (m + term1 + term2 + term3 + term4) + projection.false_northing;
			
			return new Position(eastings, northings);
		}

		public Position GridToGeo(Position grid)
		{
			double northings = grid.Y - projection.false_northing;
			double eastings = grid.X - projection.false_easting;
		
			double lat_fp = FootPointLatitude(northings);
		
			double rc_fp, v_fp, p_fp;
			RadiusOfCurvature(lat_fp, out rc_fp, out v_fp, out p_fp);
			double rc_fp_2 = Math.Pow(rc_fp, 2);
			double rc_fp_3 = Math.Pow(rc_fp, 3);
		
			double t_fp = Math.Tan(lat_fp);
			double t_fp_2 = Math.Pow(t_fp, 2);
			double t_fp_4 = Math.Pow(t_fp, 4);
			double t_fp_6 = Math.Pow(t_fp, 6);
		
			double x = eastings / (projection.k0 * v_fp);
			double x_3 = Math.Pow(x, 3);
			double x_5 = Math.Pow(x, 5);
			double x_7 = Math.Pow(x, 7);
		
			double termp = t_fp / (projection.k0 * p_fp);
			double term1 = termp * (x * eastings / 2.0);
			
			double term2 = termp * (eastings * x_3 / 24.0) * (-4.0 * rc_fp_2 + 9.0 * rc_fp * 
				(1.0 - t_fp_2) + 12.0 * t_fp_2);
			double term3 = termp * ((eastings * x_5) / 720.0) * (8.0 * Math.Pow(rc_fp, 4) * 
				(11.0 - 24.0 * t_fp_2) - 12.0 * rc_fp_3 * (21.0 - 71.0 * t_fp_2) + 15.0 * 
				rc_fp_2 * (15.0 - 98.0 * t_fp_2 + 15.0 * t_fp_4) + 180.0 * rc_fp * 
			   (5.0 * t_fp_2 - 3.0 * t_fp_4) + 360.0 * t_fp_4);
			double term4 = termp * (eastings * x_7 / 40320.0) * (1385.0 + 3633.0 * t_fp_2 + 
				4095.0 * t_fp_4 + 1575.0 * t_fp_6);
			
			double latitude = lat_fp - term1 + term2 - term3 + term4;
		
			double sec_lat_fp = Secant(lat_fp);
			term1 = x * sec_lat_fp;
			term2 = (x_3 / 6.0) * sec_lat_fp * (rc_fp + 2 * t_fp_2);
			term3 = (x_5 / 120.0) * sec_lat_fp * (-4.0 * rc_fp_3 * (1.0 - 6.0 * t_fp_2) + 
				rc_fp_2 * (9.0 - 68.0 * t_fp_2) + 72.0 * rc_fp * t_fp_2 + 24.0 * t_fp_4);
			term4 = (x_7 / 5040.0) * sec_lat_fp * (61.0 + 662.0 * t_fp_2 + 1320.0 * t_fp_4 + 
				720.0 * t_fp_6);
		
			double longitude = lon_zero + term1 - term2 + term3 - term4;
			
			return new Position(longitude, latitude);
		}
		
		double Secant(double x)
		{
			return 1.0 / Math.Cos(x);
		}
	}
}
