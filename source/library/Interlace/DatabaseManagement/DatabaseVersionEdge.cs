#region Using Directives and Copyright Notice

// Copyright (c) 2006-2010, Bit Plantation (ABN 80 332 904 638)
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

namespace Interlace.DatabaseManagement
{
	public class DatabaseVersionEdge
	{
		DatabaseSchemaUpgrade _upgrade;
		bool _unexplored;
		DatabaseVersionEdge _previousEdge;

		public DatabaseVersionEdge(DatabaseSchemaUpgrade upgrade)
		{
			_upgrade = upgrade;
			_unexplored = true;
			_previousEdge = null;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is DatabaseVersionEdge)) return false;

			DatabaseVersionEdge rhs = obj as DatabaseVersionEdge;

			return FromVersion == rhs.FromVersion && ToVersion == rhs.ToVersion;
		}

		public override int GetHashCode()
		{
			return FromVersion.GetHashCode() ^ ToVersion.GetHashCode();
		}

		public string FromVersion { get { return _upgrade.fromversion; } }
		public string ToVersion { get { return _upgrade.toversion; } }
		public string[] Commands { get { return _upgrade.command; } }

		public DatabaseVersionEdge PreviousEdge
		{
			get { return _previousEdge; }
			set { _previousEdge = value; }
		}

		public bool Unexplored
		{
			get { return _unexplored; }
			set { _unexplored = value; }
		}
	}
}
