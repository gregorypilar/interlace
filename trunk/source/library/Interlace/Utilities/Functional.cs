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
using System.Collections;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Interlace.Utilities
{
    public delegate T BinaryOperator<T>(T lhs, T rhs);

    public static class Functional
    {
        public static List<TDestination> CastList<TDestination>(IEnumerable source)
        {
            List<TDestination> destination = new List<TDestination>();

            foreach (object value in source)
            {
                destination.Add((TDestination)value);
            }

            return destination;
        }

        public static void ApplyAdd<TSource, TDestination>(IEnumerable<TSource> source, ICollection<TDestination> destination) 
        where TSource : TDestination
        {
            foreach (TSource obj in source)
            {
                destination.Add(obj);
            }
        }

        public static void ApplyAdd<TSource, TDestination>(IEnumerable<TSource> source, int offset, int length, ICollection<TDestination> destination) 
        where TSource : TDestination
        {
            IEnumerator<TSource> enumerator = source.GetEnumerator();

            int i = 0;

            while (i < offset) 
            {
                if (!enumerator.MoveNext()) break;

                i++;
            }

            while (i < offset + length) 
            {
                if (!enumerator.MoveNext()) break;

                destination.Add(enumerator.Current);

                i++;
            }
        }

        public static bool Contains<T>(IEnumerable<T> list, T obj)
        {
            foreach (T candidate in list)
            {
                if (obj.Equals(candidate)) return true;
            }

            return false;
        }

        public static T Reduce<T>(BinaryOperator<T> binaryOperator, IEnumerable<T> list)
        {
            IEnumerator<T> e = list.GetEnumerator();

            if (!e.MoveNext()) return default(T);

            T accumulator = e.Current;

            while (e.MoveNext())
            {
                accumulator = binaryOperator(accumulator, e.Current);
            }

            return accumulator;
        }

        public static int IndexOf<T>(IEnumerable<T> list, T obj)
        {
            int i = 0;

            foreach (T candidate in list)
            {
                if (object.Equals(candidate, obj))
                {
                    return i;
                }

                i++;
            }

            return -1;
        }

        public static bool ListStartsWith<T>(IEnumerable<T> list, IEnumerable<T> prefix)
        {
            IEnumerator<T> prefixEnumerator = prefix.GetEnumerator();
            IEnumerator<T> listEnumerator = list.GetEnumerator();

            while (prefixEnumerator.MoveNext())
            {
                if (!listEnumerator.MoveNext()) return false;

                if (!object.Equals(prefixEnumerator.Current, listEnumerator.Current)) return false;
            }

            return true;
        }

        public static T[] RemovePrefix<T>(IEnumerable<T> list, IEnumerable<T> prefix)
        {
            IEnumerator<T> prefixEnumerator = prefix.GetEnumerator();
            IEnumerator<T> listEnumerator = list.GetEnumerator();

            while (true)
            {
                if (!listEnumerator.MoveNext()) return new T[] { };
                if (!prefixEnumerator.MoveNext()) break;

                if (!object.Equals(prefixEnumerator.Current, listEnumerator.Current)) break;
            }

            List<T> newList = new List<T>();

            do 
            {
                newList.Add(listEnumerator.Current);
            } 
            while (listEnumerator.MoveNext());

            return newList.ToArray();
        }

        public static T[] Concatenate<T>(params IEnumerable<T>[] lists)
        {
            List<T> newList = new List<T>();

            foreach (IEnumerable<T> list in lists)
            {
                newList.AddRange(list);
            }

            return newList.ToArray();
        }
    }
}
