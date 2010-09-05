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

#endregion

namespace Interlace.Geo
{
    [Serializable] 
	public class Polyline
	{
        List<Position> _points = new List<Position>();
		
		public Polyline()
		{
		}

		public Polyline(int capacity)
		{
            _points = new List<Position>(capacity);
		}

		public Polyline(int[][] points)
		{
			for (int i = 0; i < points.Length; i++) 
			{
				Add(new Position(points[i][0], points[i][1]));
			}
		}

		public Polyline(IEnumerable<Position> points)
		{
            _points.AddRange(points);
		}
		
		public Polyline Clone()
		{
			Polyline l = new Polyline();
            l._points = _points.GetRange(0, _points.Count);
			return l;
		}

        public Box Box()
        {
            return new Box(this);
        }

		public void AddPolyline(Polyline p)
		{	
			_points.AddRange(p._points);
		}
			
		public void Add(Position p)
		{
			_points.Add(p);
		}
		
		public int Length
		{
			get 
			{
				return _points.Count;
			}
		}
		
		public delegate Position ApplyDelegate(Position p);
		
		public Polyline Apply(ApplyDelegate f)
		{
			Polyline new_line = new Polyline();

			for (int i = 0; i < _points.Count; i++) 
			{
				new_line.Add(f((Position)_points[i]));
			}

			return new_line;
		}

		public Polyline ApplyDegreesToRadians()
		{
			Polyline new_line = new Polyline();

			for (int i = 0; i < _points.Count; i++) 
			{
				Position p = (Position)_points[i];
				new_line.Add(new Position(Math.PI * p.X / 180.0, Math.PI * p.Y / 180.0));
			}

			return new_line;
		}

		public Position this[int index]
		{
			get 
			{
				return (Position)_points[index];
			}
		}
		
		public Polyline SubString(int i, int j)
		{
			if (j <= i) throw new ArgumentOutOfRangeException();
			
			if (i < 0 || Length < i) throw new ArgumentOutOfRangeException();
			if (j < 0 || Length < j) throw new ArgumentOutOfRangeException();
			
			Polyline new_line = new Polyline();
			
			for (int k = i; k < j; k++) 
			{
				new_line.Add((Position)_points[k]);
			}
			
			return new_line;
		}

		public Polyline Reversed
		{
            get
            {
                Polyline l = new Polyline();
                l._points = _points.GetRange(0, _points.Count);
                l._points.Reverse();
                return l;
            }
		}
            
        public IEnumerator<Geo.Position> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

		public Polyline Simplify(double threshold)
		{
			PolylineSimplifyBuilder builder = new PolylineSimplifyBuilder(this);
			DouglasPeucker.Simplify(this, threshold, new DouglasPeucker.HandleSegment(builder.HandleSegment));
			return builder.output;
		}

        /// <summary>
        /// Calculates the area of non-self-intersecting polygons. If the polygon isn't closed,
        /// it is closed to calculate the area.
        /// </summary>
        /// <returns>The area of the polygon.</returns>
        public double Area
        {
            get
            {
                if (_points.Count < 3) return 0.0;

                double sum = 0.0;

                for (int i = 0; i < _points.Count - 1; i++)
                {
                    Geo.Position i0 = _points[i];
                    Geo.Position i1 = _points[(i + 1) % _points.Count];

                    sum += i0.X * i1.Y - i1.X * i0.Y;
                }

                // If the polygon isn't closed, add the area of the closing segment:
                Geo.Position last = _points[_points.Count - 1];
                Geo.Position first = _points[0];

                if (!Geo.Position.PointsEqual(last, first, Double.Epsilon))
                {
                    sum += last.X * first.Y - first.X * last.Y;
                }

                return Math.Abs(sum / 2.0);
            }
        }

        // Containment routines based on:
        //
        // http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        //
        // Copyright 2001, softSurfer (www.softsurfer.com)
        // This code may be freely used and modified for any purpose
        // providing that this copyright notice is included with it.
        // SoftSurfer makes no warranty for this code, and cannot be held
        // liable for any real or imagined damage resulting from its use.
        // Users of this code must verify correctness for their application.

        /// <summary>
        /// Tests if a point is left, on, or right of an infinite line.
        /// </summary>
        /// <param name="a">The first point of the line.</param>
        /// <param name="b">The second point of the line.</param>
        /// <param name="p">The test point.</param>
        /// <returns>Less than zero for a point left of the line, greater than zero for a 
        /// point right of the line, and zero for a point on the line.</returns>
        static double IsLeft(Position a, Position b, Position p)
        {
            return (b.X - a.X) * (p.Y - a.Y) - (p.X - a.X) * (b.Y - a.Y);
        }

        /// <summary>
        /// Returns the winding number for a point in a polygon.
        /// </summary>
        /// <returns>The winding number, which is non-zero if the point is within the polygon.</returns>
        public int GetWindingNumber(Position p)
        {
            int windingNumber = 0;

            int vLength = _points.Count;

            for (int i = 0; i < vLength; i++) 
            {
                Position v1 = _points[i];
                Position v2 = (i < vLength - 1) ? _points[i + 1] : _points[0];

                // Handle the edge from _points[i] to _points[i + 1]:
                if (v1.Y <= p.Y) 
                {
                    // Check for an upward crossing with the point to the left:
                    if (v2.Y > p.Y) 
                        if (IsLeft(v1, v2, p) > 0)
                            windingNumber++;
                }
                else 
                {
                    // Check for an downward crossing with the point to the right:
                    if (v2.Y <= p.Y)
                        if (IsLeft(v1, v2, p) < 0)
                            windingNumber--;
                }
            }

            return windingNumber;
        }

        public bool Contains(Position position)
        {
            return GetWindingNumber(position) != 0;
        }
    }

    internal class PolylineSimplifyBuilder
	{
		public Polyline input, output;
		
		public PolylineSimplifyBuilder(Polyline input)
		{
			this.input = input;
			output = new Polyline();
		}
		
		public void HandleSegment(int i, int j)
		{
			if (output.Length == 0) output.Add(input[i]);

			output.Add(input[j]);
		}
	}
}
