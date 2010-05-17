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
using System.Collections.ObjectModel;
using System.Text;

#endregion

namespace Interlace.Collections
{
    public class DictionaryOfLists<TKey, TValue> : IEnumerable<KeyValuePair<TKey, ICollection<TValue>>>
    {
        Dictionary<TKey, List<TValue>> _dictionary;
        readonly ReadOnlyCollection<TValue> _emptyCollection =
            new ReadOnlyCollection<TValue>(new List<TValue>());

        public DictionaryOfLists()
        {
            _dictionary = new Dictionary<TKey, List<TValue>>();
        }

        public IEnumerable<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        public void Add(TKey key, TValue value)
        {
            List<TValue> list;

            bool exists = _dictionary.TryGetValue(key, out list);

            if (!exists)
            {
                list = new List<TValue>();
                _dictionary[key] = list;
            }

            list.Add(value);
        }

        public void Add(TKey key, ICollection<TValue> values)
        {
            List<TValue> list;

            bool exists = _dictionary.TryGetValue(key, out list);

            if (!exists)
            {
                list = new List<TValue>();
                _dictionary[key] = list;
            }

            foreach (TValue value in values)
            {
                list.Add(value);
            }
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public void Clear(TKey key)
        {
            _dictionary.Remove(key);
        }

        public ICollection<TValue> this[TKey key]
        {
            get
            {
                List<TValue> list;

                bool exists = _dictionary.TryGetValue(key, out list);

                if (exists)
                {
                    return new ReadOnlyCollection<TValue>(list);
                }
                else
                {
                    return _emptyCollection;
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, ICollection<TValue>>> GetEnumerator()
        {
            return new Enumerator(_dictionary.GetEnumerator());
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(_dictionary.GetEnumerator());
        }

        class Enumerator : IEnumerator<KeyValuePair<TKey, ICollection<TValue>>>
        {
            IEnumerator<KeyValuePair<TKey, List<TValue>>> _baseEnumerator;

            public Enumerator(IEnumerator<KeyValuePair<TKey, List<TValue>>> baseEnumerator)
            {
                _baseEnumerator = baseEnumerator;
            }

            public KeyValuePair<TKey, ICollection<TValue>> Current
            {
                get 
                { 
                    KeyValuePair<TKey, List<TValue>> pair = _baseEnumerator.Current;

                    return new KeyValuePair<TKey, ICollection<TValue>>(
                        pair.Key, new ReadOnlyCollection<TValue>(pair.Value));
                }
            }

            public void Dispose()
            {
                _baseEnumerator.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                return _baseEnumerator.MoveNext();
            }

            public void Reset()
            {
                _baseEnumerator.Reset();
            }
        }
    }
}
