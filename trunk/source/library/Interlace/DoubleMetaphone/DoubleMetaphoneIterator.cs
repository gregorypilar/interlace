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

#endregion

// Portions of this code were originally developed for Bit Plantation Dinero.
// (Portions Copyright © 2006 Bit Plantation)

using System;
using System.Diagnostics;

namespace Interlace.DoubleMetaphone
{
	public class DoubleMetaphoneIterator
	{
		string _word;
		int _position;

		public DoubleMetaphoneIterator(string word)
		{
			_word = word.ToUpper();
			_position = 0;
		}

		public int Length
		{
			get { return _word.Length; }
		}

		public int Position
		{
			get { return _position; }
		}

		public void Advance(int count)
		{
			_position = Math.Min(_position + count, _word.Length);
		}

		public char Letter
		{
			get 
			{ 
				if (_position < _word.Length)
					return _word[_position]; 
				else
					return (char)0;
			}
		}

		public char NextLetter
		{
			get
			{
				if (_position + 1 < _word.Length)
					return _word[_position + 1]; 
				else
					return (char)0;
			}
		}

		public char LastLetter
		{
			get
			{
				if (_position - 1 >= 0)
					return _word[_position - 1]; 
				else
					return (char)0;
			}
		}

		public char FirstLetter
		{
			get
			{
				if (_word.Length > 0)
					return _word[0];
				else
					return (char)0;
			}
		}

		public bool AtEnd
		{
			get { return _position == _word.Length; }
		}

		public bool AtLastLetter
		{
			get { return _position == _word.Length - 1; }
		}

		public bool AtStart
		{
			get { return _position == 0; }
		}

		public bool Matches(int offset, params string[] matches)
		{
			foreach (string match in matches)
			{
				if (_position + offset < 0) continue;
				if (_position + offset + match.Length > _word.Length) continue;

				if (_word.Substring(_position + offset, match.Length) == match) return true;
			}

			return false;
		}

		public bool Matches(params string[] matches)
		{
			return Matches(0, matches);
		}

		public bool StartsWith(params string[] matches)
		{
			foreach (string match in matches)
			{
				if (_word.StartsWith(match)) return true;
			}		

			return false;
		}

		public bool EndsWith(params string[] matches)
		{
			foreach (string match in matches)
			{
				if (_word.EndsWith(match)) return true;
			}

			return false;
		}

		public bool IsBeyondEnd(int offset)
		{
			return _position + offset >= _word.Length;
		}
		
		public void SkipOnMatch(params string[] matches)
		{
			foreach (string match in matches)
			{
				if (_position < 0) continue;
				if (_position + match.Length > _word.Length) continue;

				if (_word.Substring(_position, match.Length) == match) 
				{
					_position += match.Length;
					return;
				}
			}
		}

		public bool Contains(string match)
		{
			return _word.IndexOf(match) != -1;
		}
	}
}
