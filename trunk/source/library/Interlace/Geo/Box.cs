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
	public struct Box
	{
		double _x1, _y1, _x2, _y2;

        public static Box InfiniteBox
        {
            get
            {
                Box newBox = new Box();

                newBox._x1 = Double.NegativeInfinity;
                newBox._y1 = Double.NegativeInfinity;
                newBox._x2 = Double.PositiveInfinity;
                newBox._y2 = Double.PositiveInfinity;

                return newBox;
            }
        }

        public static Box EmptyBox
        {
            get
            {
                Box newBox = new Box();

                newBox._x1 = Double.PositiveInfinity;
                newBox._y1 = Double.PositiveInfinity;
                newBox._x2 = Double.NegativeInfinity;
                newBox._y2 = Double.NegativeInfinity;

                return newBox;
            }
        }

        public Box(double x1, double y1, double x2, double y2)
        {
            _x1 = Math.Min(x1, x2);
            _y1 = Math.Min(y1, y2);
            _x2 = Math.Max(x1, x2);
            _y2 = Math.Max(y1, y2);
        }

        internal Box(Polyline line)
        {
            if (line.Length == 0)
            {
                _x1 = Double.NegativeInfinity;
                _y1 = Double.NegativeInfinity;
                _x2 = Double.PositiveInfinity;
                _y2 = Double.PositiveInfinity;
            }
            else
            {
                _x1 = _x2 = line[0].X;
                _y1 = _y2 = line[0].Y;

                for (int i = 0; i < line.Length; i++)
                {
                    Position p = line[i];

                    if (p.X < _x1) _x1 = p.X;
                    if (p.Y < _y1) _y1 = p.Y;
                    if (p.X > _x2) _x2 = p.X;
                    if (p.Y > _y2) _y2 = p.Y;
                }
            }
        }

        public double X1
        {
            get
            {
                return _x1;
            }
            set
            {
                _x1 = value;
            }
        }

        public double Y1
        {
            get
            {
                return _y1;
            }
            set
            {
                _y1 = value;
            }
        }

        public double X2
        {
            get
            {
                return _x2;
            }
            set
            {
                _x2 = value;
            }
        }

        public double Y2
        {
            get
            {
                return _y2;
            }
            set
            {
                _y2 = value;
            }
        }

		public double Width {
			get {
				return _x2 - _x1;
			}
		}
		
		public double Height {
			get {
				return _y2 - _y1;
			}
		}

        public bool IsFinite
        {
            get
            {
                return _x1 != Double.NegativeInfinity &&
                       _y1 != Double.NegativeInfinity &&
                       _x2 != Double.PositiveInfinity &&
                       _y2 != Double.PositiveInfinity;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return _x1 > _x2 && _y1 > _y2;
            }
        }

		public Position Centre
		{
			get 
			{
				return new Position(_x1 + (_x2 - _x1) / 2, _y1 + (_y2 - _y1) / 2);
			}
		}
		
		public bool Contains(Box rhs)
		{
			return 
				_x1 <= rhs._x1 && rhs._x1 <= _x2 &&
				_x1 <= rhs._x2 && rhs._x2 <= _x2 &&
				_y1 <= rhs._y1 && rhs._y1 <= _y2 &&
				_y1 <= rhs._y2 && rhs._y2 <= _y2;
		}

		public bool Contains(Position rhs)
		{
			return 
				_x1 <= rhs.X && rhs.X <= _x2 &&
				_y1 <= rhs.Y && rhs.Y <= _y2;
		}

		public double Area
		{
			get 
			{
				return (_x2 - _x1) * (_y2 - _y1);
			}
		}

        public void ExpandToInclude(Box rhs)
        {
            _x1 = Math.Min(_x1, rhs._x1);
            _y1 = Math.Min(_y1, rhs._y1);
            _x2 = Math.Max(_x2, rhs._x2);
            _y2 = Math.Max(_y2, rhs._y2);
        }

		public double GetOverlappingArea(Box rhs)
		{
			double rightMostLeft = Math.Max(_x1, rhs._x1);
			double leftMostRight = Math.Min(_x2, rhs._x2);

			double bottomMostTop = Math.Max(_y1, rhs._y1);
			double topMostBottom = Math.Min(_y2, rhs._y2);

			if (leftMostRight <= rightMostLeft) return 0.0;
			if (topMostBottom <= bottomMostTop) return 0.0;

			return (leftMostRight - rightMostLeft) * (topMostBottom - bottomMostTop);
		}

        public bool Intersects(Box rhs)
        {
            return ((rhs._x1 <= _x1 && _x1 <= rhs._x2) || (_x1 <= rhs._x1 && rhs._x1 <= _x2)) &&
                   ((rhs._y1 <= _y1 && _y1 <= rhs._y2) || (_y1 <= rhs._y1 && rhs._y1 <= _y2));
        }
	}
}
