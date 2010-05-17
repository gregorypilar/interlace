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

namespace Interlace.Collections
{
    public class CountedSet<T> 
    {
        Dictionary<T, int> _members;

        public CountedSet()
        {
            _members = new Dictionary<T, int>();
        }

        public CountedSet(params T[] members)
        : this(members as IEnumerable<T>)
        { 
        }

        public CountedSet(IEnumerable<T> members)
        : this()
        {
            foreach(T item in members)
            {
                _members[item] = 1;
            }
        }

        public int UniqueCount
        {
            get { return _members.Count; }
        }

        public int NonUniqueCount
        {
            get
            {
                int count = 0;

                foreach (int value in _members.Values)
                {
                    count += value;
                }

                return count;
            }
        }

        public bool Contains(T item)
        {
            return _members.ContainsKey(item);
        }

        public int CountFor(T item)
        {
            if (!_members.ContainsKey(item)) return 0;

            return _members[item];
        }

        public void UnionUpdate(T item)
        {
            if (!_members.ContainsKey(item)) _members[item] = 0;

            _members[item] = _members[item] + 1;
        }

        public void UnionUpdate(T item, int count)
        {
            if (!_members.ContainsKey(item)) _members[item] = 0;

            _members[item] = _members[item] + count;
        }

        public void UnionUpdate(CountedSet<T> rhs)
        {
            foreach (KeyValuePair<T, int> pair in rhs._members)
            {
                UnionUpdate(pair.Key, pair.Value);
            }
        }

        public void UnionUpdate(IEnumerable<T> rhs)
        {
            foreach (T item in rhs)
            {
                UnionUpdate(item, 1);
            }
        }

        public void DifferenceUpdate(T rhs)
        {
            if (!_members.ContainsKey(rhs)) return;

            if (_members[rhs] == 1)
            {
                _members.Remove(rhs);
            }
            else
            {
                _members[rhs] = _members[rhs] - 1;
            }
        }

        public void DifferenceUpdate(T rhs, int count)
        {
            if (!_members.ContainsKey(rhs)) return;

            int newCount = _members[rhs] - count;

            if (newCount < 1)
            {
                _members.Remove(rhs);
            }
            else
            {
                _members[rhs] = newCount;
            }
        }

        public void DifferenceUpdate(CountedSet<T> rhs)
        {
            foreach (KeyValuePair<T, int> pair in rhs._members)
            {
                DifferenceUpdate(pair.Key, pair.Value);
            }
        }

        public void DifferenceUpdate(IEnumerable<T> rhs)
        {
            foreach (T item in rhs)
            {
                DifferenceUpdate(item, 1);
            }
        }

        public IEnumerable<T> UniqueEnumerable
        {
            get
            {
                return _members.Keys;
            }
        }
    }
}
