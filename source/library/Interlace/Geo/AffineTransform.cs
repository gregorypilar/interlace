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
	public class AffineTransform
	{
		// The matrix, in Postscript order:
		double[] _matrix;
		
		public AffineTransform()
		{
			_matrix = new double[6] { 1.0, 0.0, 0.0, 1.0, 0.0, 0.0 };
		}

		public AffineTransform(Box from_box, Box to_box)
		{
			_matrix = new double[6] { 1.0, 0.0, 0.0, 1.0, 0.0, 0.0 };
			
			Translate(-from_box.X1, -from_box.Y1);
			Scale(to_box.Width / from_box.Width, to_box.Height / from_box.Height);
			Translate(to_box.X1, to_box.X1);
		}
		
		private void ApplyTransform(double[] rhs)
		{
			_matrix = new double[6] {
			  _matrix[0] * rhs[0] + _matrix[1] * rhs[2],
			  _matrix[0] * rhs[1] + _matrix[1] * rhs[3],
			  _matrix[2] * rhs[0] + _matrix[3] * rhs[2],
			  _matrix[2] * rhs[1] + _matrix[3] * rhs[3],
			  _matrix[4] * rhs[0] + _matrix[5] * rhs[2] + rhs[4],
			  _matrix[4] * rhs[1] + _matrix[5] * rhs[3] + rhs[5],
			};
		}
		
		public void Scale(double x, double y)
		{
			ApplyTransform(new double[6] { x, 0.0, 0.0, y, 0.0, 0.0 });
		}
		
		public void Translate(double x, double y)
		{
			ApplyTransform(new double[6] { 1.0, 0.0, 0.0, 1.0, x, y });			
		}
		
		public void Rotate(double radians)
		{
			ApplyTransform(new double[6] { Math.Cos(radians), Math.Sin(radians), 
			  -Math.Sin(radians), Math.Cos(radians), 0, 0 } );
		}
		
		public Position Transform(Position p)
		{
			return new Position(_matrix[0] * p.X + _matrix[2] * p.Y + _matrix[4],
			                 _matrix[1] * p.X + _matrix[3] * p.Y + _matrix[5]);
		}
		
		public Polyline Transform(Polyline pl)
		{
			Polyline npl = new Polyline();
			for (int i = 0; i < pl.Length; i++) {
				npl.Add(Transform(pl[i]));
			}
			
			return npl;
		}
	}
}
