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
	public class JourneySegment
	{
		JourneySegment _next, _previous;
		Polyline _line = new Polyline();

        public JourneySegment Next
        {
            get
            {
                return _next;
            }
            set
            {
                _next = value;
            }
        }

        public JourneySegment Previous
        {
            get
            {
                return _previous;
            }
            set
            {
                _previous = value;
            }
        }

        public Polyline Line
        {
            get
            {
                return _line;
            }
            set
            {
                _line = value;
            }
        }

		public Position Start 
		{
			get 
			{
				return Line[0];
			}
		}
		
		public Position End 
		{
			get 
			{
				return Line[Line.Length - 1];
			}
		}
		
		public Angle Heading
		{
			get 
			{
				return new Angle(Math.Atan2(End.Y - Start.Y, End.X - Start.X));
			}
		}
		
		public Angle TurnAngle 
		{
			get 
			{
				if (Next == null) return Heading;
			
				return Next.Heading - Heading;
			}
		}
		
		public double Length 
		{
			get 
			{
				return Math.Sqrt(Math.Pow(End.Y - Start.Y, 2) + Math.Pow(End.X - Start.X, 2));
			}
		}
	}
}
