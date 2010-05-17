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

namespace Interlace.Erlang
{
    public class ErlangDictionary : ITermWritable
    {
        Dictionary<string, object> _values;

        public ErlangDictionary()
        {
            _values = new Dictionary<string, object>();
        }

        public ErlangDictionary(object dictionaryTuples)
        {
            if (!(dictionaryTuples is List<object>))
            {
                throw new ErlangProtocolException(
                    "A list of options was expected but some term other than a list was found.");
            }

            List<object> dictionaryTuplesList = dictionaryTuples as List<object>;

            _values = new Dictionary<string, object>(dictionaryTuplesList.Count);

            foreach (object tupleObject in dictionaryTuplesList)
            {
                if (!(tupleObject is Tuple))
                {
                    throw new ErlangProtocolException(
                        "A list of options was expected but an element in the list is not a tuple.");
                }

                Tuple tuple = tupleObject as Tuple;

                if (tuple.Length != 2)
                {
                    throw new ErlangProtocolException(
                        "A tuple in an option list is expected to have two elements, and one was " +
                        "found with fewer or more than two items.");
                }

                Atom key = tuple.AtomAt(0);
                object value = tuple.ObjectAt(1);

                _values[key.Value] = value;
            }
        }

        public object this[string key]
        {
            get { return _values[key]; }
            set { _values[key] = value; }
        }

        T InternalAt<T>(string atom)
        {
            if (!_values.ContainsKey(atom)) throw new ErlangProtocolException(string.Format(
                "A list of options was expected to contain an option \"{0}\", but did not.",
                atom));

            if (typeof(T).Equals(typeof(string)) && _values[atom] is List<object>)
            {
                List<object> list = _values[atom] as List<object>;

                if (list.Count == 0) return (T)(object)"";
            }

            if (!(_values[atom] is T)) throw new ErlangProtocolException(string.Format(
                "The option value for option {0} of an option list is not the correct type."));

            return (T)_values[atom];
        }

        public Tuple TupleAt(string atom)
        {
            return InternalAt<Tuple>(atom);
        }

        public string StringAt(string atom)
        {
            return InternalAt<string>(atom);
        }

        public int IntegerAt(string atom)
        {
            return InternalAt<int>(atom);
        }

        public Atom AtomAt(string atom)
        {
            return InternalAt<Atom>(atom);
        }

        public object ObjectAt(string atom)
        {
            return InternalAt<object>(atom);
        }

        T InternalAt<T>(string atom, T defaultValue)
        {
            if (!_values.ContainsKey(atom)) return defaultValue;

            if (typeof(T).Equals(typeof(string)) && _values[atom] is List<object>)
            {
                List<object> list = _values[atom] as List<object>;

                if (list.Count == 0) return (T)(object)"";
            }

            if (!(_values[atom] is T)) throw new ErlangProtocolException(string.Format(
                "The option value for option {0} of an option list is not the correct type."));

            return (T)_values[atom];
        }

        public Tuple TupleAt(string atom, Tuple defaultValue)
        {
            return InternalAt<Tuple>(atom, defaultValue);
        }

        public string StringAt(string atom, string defaultValue)
        {
            return InternalAt<string>(atom, defaultValue);
        }

        public int IntegerAt(string atom, int defaultValue)
        {
            return InternalAt<int>(atom, defaultValue);
        }

        public Atom AtomAt(string atom, Atom defaultValue)
        {
            return InternalAt<Atom>(atom, defaultValue);
        }

        public object ObjectAt(string atom, object defaultValue)
        {
            return InternalAt<object>(atom, defaultValue);
        }

        #region ITermWritable Members

        public object SerializeToTerms()
        {
            List<object> tuples = new List<object>();

            foreach (KeyValuePair<string, object> pair in _values)
            {
                tuples.Add(new Tuple(Atom.From(pair.Key), pair.Value));
            }

            return tuples;
        }

        #endregion

        public override bool Equals(object obj)
        {
            ErlangDictionary rhs = obj as ErlangDictionary;

            if (rhs == null) return false;

            // All of the names in the lhs should be in the rhs, and the values equal:
            foreach (string lhsName in _values.Keys)
            {
                if (!rhs._values.ContainsKey(lhsName)) return false;

                if (!object.Equals(_values[lhsName], rhs._values[lhsName])) return false;
            }

            // All of the names in the rhs should be in the lhs if they are equal:
            foreach (string rhsName in rhs._values.Keys)
            {
                if (!_values.ContainsKey(rhsName)) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;

            List<string> keys = new List<string>(_values.Keys);
            keys.Sort();

            foreach (string key in keys)
            {
                hashCode ^= key.GetHashCode();
                if (_values[key] != null) hashCode ^= _values[key].GetHashCode();
            }

            return hashCode;
        }
    }
}
