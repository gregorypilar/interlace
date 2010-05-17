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
using System.Text;

#endregion

namespace Interlace.Utilities
{
    [Serializable]
    public class Pair<F, S> : IComparable
    {
        readonly F _first;
        readonly S _second;

        public Pair(F first, S second)
        {
            _first = first;
            _second = second;
        }

        public F First
        {
            get { return _first; }
        }

        public S Second
        {
            get { return _second; }
        }

        public override bool Equals(object obj)
        {
            Pair<F, S> rhs = obj as Pair<F, S>;

            if (rhs == null) return false;

            return Object.Equals(_first, rhs._first) && Object.Equals(_second, rhs._second);
        }

        public override int GetHashCode()
        {
            int hash = 0;

            if (_first != null) hash ^= _first.GetHashCode();
            if (_second != null) hash ^= ~_second.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", _first.ToString(), _second.ToString());
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Pair<F, S>)) throw new InvalidOperationException(
                "A Pair instance can only be compared to another Pair instant with " +
                "exactly the same generic type arguments.");

            Pair<F, S> rhs = obj as Pair<F, S>;

            if (rhs == null) return 1;

            IComparable firstComparable = _first as IComparable;
            IComparable secondComparable = _second as IComparable;

            if (firstComparable != null)
            {
                int comparison = firstComparable.CompareTo(rhs.First);

                if (comparison != 0) return comparison;
            }

            if (secondComparable != null)
            {
                return secondComparable.CompareTo(rhs.Second);
            }

            // If neither are comparable, consider the objects identical:
            return 0;
        }
    }
}
