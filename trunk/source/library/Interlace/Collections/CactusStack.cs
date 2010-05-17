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
    /// <summary>
    /// An immutable cactus stack; see http://www.nist.gov/dads/HTML/cactusstack.html for details.
    /// </summary>
    /// <typeparam name="T">The type stored in the stack.</typeparam>
    public class CactusStack<T>
    {
        T _value;
        CactusStack<T> _parent;

        public CactusStack(T value)
        {
            _value = value;
            _parent = null;
        }

        CactusStack(CactusStack<T> parent, T value)
        {
            _value = value;
            _parent = parent;
        }

        public CactusStack<T> Push(T value)
        {
            return new CactusStack<T>(this, value);
        }

        public T Value
        { 	 
           get { return _value; }
        }

        public CactusStack<T> Parent
        { 	 
           get { return _parent; }
        }

        public IEnumerable<T> ValuesInReverse
        {
            get
            {
                CactusStack<T> current = this;

                while (current != null)
                {
                    yield return current.Value;

                    current = current.Parent;
                }
            }
        }

        public IEnumerable<CactusStack<T>> StackedValues
        {
            get
            {
                CactusStack<T> current = this;

                while (current != null)
                {
                    yield return current;

                    current = current.Parent;
                }
            }
        }

        public CactusStack<T> Reversed
        {
            get
            {
                CactusStack<T> top = new CactusStack<T>(_value);
                CactusStack<T> current = _parent;

                while (current != null)
                {
                    top = top.Push(current.Value);

                    current = current.Parent;
                }

                return top;
            }
        }

        public int Count
        {
            get 
            {
                CactusStack<T> current = this;

                int count = 0;

                while (current != null)
                {
                    count++;

                    current = current.Parent;
                }

                return count;
            }
        }

        public IEnumerable<T> Values
        {
            get
            {
                T[] values = new T[Count];

                int i = values.Length - 1;

                CactusStack<T> current = this;

                while (current != null)
                {
                    values[i] = current.Value;
                    i--;

                    current = current.Parent;
                }

                return values;
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            CactusStack<T> current = this;

            result.Append("[CactusStack, top first: ");

            while (current != null)
            {
                if (current._value != null) 
                {
                    result.Append(current._value.ToString());
                }
                else
                {
                    result.Append("(null)");
                }

                if (current.Parent != null) result.Append(", ");

                current = current.Parent;
            }

            result.Append("]");

            return result.ToString();
        }

        public override bool Equals(object obj)
        {
            CactusStack<T> currentLeft = this;
            CactusStack<T> currentRight = obj as CactusStack<T>;

            if (currentRight == null) return false;

            do
            {
                if (!object.Equals(currentLeft._value, currentRight._value)) return false;

                currentLeft = currentLeft._parent;
                currentRight = currentRight._parent;

                if ((currentLeft == null) != (currentRight == null))
                {
                    return false;
                }
            }
            while (currentLeft != null && currentRight != null);

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = 0;

            CactusStack<T> current = this;

            while (current != null)
            {
                if (current._value != null) hashCode ^= current._value.GetHashCode();

                current = current.Parent;
            }

            return hashCode;
        }
    }
}
