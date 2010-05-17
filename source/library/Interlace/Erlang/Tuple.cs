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

// Portions of this code were originally developed for Bit Plantation BitLibrary.
// (Portions Copyright © 2006 Bit Plantation)

using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Net;

namespace Interlace.Erlang
{
    /// <summary>
    /// Represents an Erlang style tuple.
    /// </summary>
    public class Tuple
    {
        object[] _elements;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple"/> class.
        /// </summary>
        /// <param name="elements">The elements that make up the tuple items.</param>
        public Tuple(params object[] elements)
        {
            _elements = elements;
        }

        /// <summary>
        /// Gets the object at the specified index.
        /// </summary>
        /// <value>The object.</value>
        public object this[int index]
        {
            get { return _elements[index]; }
        }

        /// <summary>
        /// Gets the number of items in the tuple.
        /// </summary>
        /// <value>The number of items.</value>
        public int Length
        {
            get { return _elements.Length; }
        }

        T InternalAt<T>(int i)
        {
            if (i < 0) throw new IndexOutOfRangeException();

            if (_elements.Length <= i) throw new ErlangProtocolException(string.Format(
                "A tuple of at least {0} (but possibly more) items was expected, and " +
                "a tuple with only {1} elements was received", i + 1, _elements.Length));

            if (typeof(T).Equals(typeof(string)) && _elements[i] is List<object>)
            {
                List<object> list = _elements[i] as List<object>;

                if (list.Count == 0) return (T)(object)"";
            }

            if (!(_elements[i] is T)) throw new ErlangProtocolException(string.Format(
                "The element at index {0} of a tuple is not the correct type."));

            return (T)_elements[i];
        }

        /// <summary>
        /// Gets the tuple at the given index, throwing an <see cref="T:ErlangProtocolException" /> if
        /// the tuple either does not have the specified element or if the element is not the correct type.
        /// </summary>
        /// <param name="i">The index of the element to return.</param>
        /// <returns>The tuple at the specified index.</returns>
        public Tuple TupleAt(int i)
        {
            return InternalAt<Tuple>(i);
        }

        /// <summary>
        /// Gets the string at the given index, throwing an <see cref="T:ErlangProtocolException" /> if
        /// the tuple either does not have the specified element or if the element is not the correct type.
        /// </summary>
        /// <param name="i">The index of the element to return.</param>
        /// <returns>The string at the specified index.</returns>
        public string StringAt(int i)
        {
            return InternalAt<string>(i);
        }

        /// <summary>
        /// Gets the integer at the given index, throwing an <see cref="T:ErlangProtocolException" /> if
        /// the tuple either does not have the specified element or if the element is not the correct type.
        /// </summary>
        /// <param name="i">The index of the element to return.</param>
        /// <returns>The integer at the specified index.</returns>
        public int IntegerAt(int i)
        {
            return InternalAt<int>(i);
        }

        /// <summary>
        /// Gets the atom at the given index, throwing an <see cref="T:ErlangProtocolException" /> if
        /// the tuple either does not have the specified element or if the element is not the correct type.
        /// </summary>
        /// <param name="i">The index of the element to return.</param>
        /// <returns>The atom at the specified index.</returns>
        public Atom AtomAt(int i)
        {
            return InternalAt<Atom>(i);
        }

        /// <summary>
        /// Gets the object at the given index, throwing an <see cref="T:ErlangProtocolException" /> if
        /// the tuple does not have the specified element.
        /// </summary>
        /// <param name="i">The index of the element to return.</param>
        /// <returns>The atom at the specified index.</returns>
        public object ObjectAt(int i)
        {
            return InternalAt<object>(i);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            Tuple rhs = obj as Tuple;
            if (rhs == null) return false;

            if (_elements.Length != rhs.Length) return false;

            for (int i = 0; i < _elements.Length; i++)
            {
                if (!Object.Equals(_elements[i], rhs._elements[i])) return false;
            }

            return true;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override int GetHashCode()
        {
            int hashCode = 0x00010001 * _elements.Length;

            for (int i = 0; i < _elements.Length; i++)
            {
                if (_elements[i] != null)
                {
                    hashCode ^= _elements[i].GetHashCode();
                }
                else
                {
                    hashCode ^= 0x00020002 * i;
                }
            }

            return hashCode;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            string[] elementStrings = new string[_elements.Length];

            for (int i = 0; i < _elements.Length; i++)
            {
                elementStrings[i] = _elements[i].ToString();
            }

            return string.Format("Tuple({0})", string.Join(", ", elementStrings));
        }
    }
}
