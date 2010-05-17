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
	/// Implements the simple version of the Douglas-Peucker 
	/// Line-Simplification algorithm.
	/// 
	/// The original paper is not widely available; this implemention
	/// is based on a summary of the original paper in:
	/// 
	///     J. Hershberger, J. Snoeyink. Speeding Up the Douglas-Peucker
	/// 	  Line-Simplification Algorithm. Technical Report TR-92-07, 
	///       Department of Computer Science, University of British Columbia, 
	///       April 1992.
    ///
	/// </summary>
	public class DouglasPeucker
	{
		public delegate void HandleSegment(int i, int j);
		
		private static double GetSegmentSquaredDistance(Position a, Position b, Position p)
		{
			Position ab = b - a;
			Position ap = p - a;
			double abLength = ab.SquaredDistanceFromOrigin();
			
			// Handle a zero length segment; where a and b coincide:
			if (abLength == 0.0) {
				return Position.SquaredDistance(a, p);
			}
			
			double projAbAp = Position.DotProduct(ab, ap) / abLength;
			
			if (projAbAp <= 0.0) {
				return Position.SquaredDistance(a, p);
			}
			if (projAbAp >= 1.0) {
				return Position.SquaredDistance(b, p);
			}
			
			Position closest = new Position(a.X + ab.X * projAbAp, a.Y + ab.Y * projAbAp);
			return Position.SquaredDistance(closest, p);
		}
		
		private static void FindSplit(Polyline l, int i, int j, 
			out double maxDistance, out int maxI)
		{
			maxI = -1;
			maxDistance = 0.0;
			
			if (j - i <= 1) return;
			
			Position a = l[i];
			Position b = l[j];
			
			for (int k = i + 1; k < j; k++) {
				double distance = GetSegmentSquaredDistance(a, b, l[k]);
				if (distance > maxDistance || maxI == -1) {
					maxI = k;
					maxDistance = distance;
				}
			}
		}
		
		private static void Recurse(Polyline input, double threshold, HandleSegment handler, 
		  int i, int j)
		{
			double maxDistance;
			int maxI;
			
			FindSplit(input, i, j, out maxDistance, out maxI);
			
			if (maxDistance > threshold * threshold) {
				Recurse(input, threshold, handler, i, maxI);
				Recurse(input, threshold, handler, maxI, j);
			} else {
				handler(i, j);
			}
		}
		
		public static void Simplify(Polyline input, double threshold, HandleSegment handler)
		{
			if (input.Length < 2) return;
			
			Polyline output = new Polyline();
			Recurse(input, threshold, handler, 0, input.Length - 1);
		}
	}
}
