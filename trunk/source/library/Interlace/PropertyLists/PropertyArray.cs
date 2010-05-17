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
using System.IO;
using System.Text;

using Interlace.Utilities;

#endregion

namespace Interlace.PropertyLists
{
    [Serializable]
    public class PropertyArray : IEnumerable
    {
        List<object> _list;

        internal PropertyArray()
        {
            _list = new List<object>();
        }

        public static PropertyArray EmptyArray()
        {
            return new PropertyArray();
        }

        public static PropertyArray FromString(string text)
        {
            if (string.IsNullOrEmpty(text)) return PropertyArray.EmptyArray();

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return PropertyArray.FromReader(reader, "text string");
                }
            }
        }

        public static PropertyArray FromReader(TextReader reader, string nameForExceptions)
        {
            Lexer lexer = new Lexer(reader, nameForExceptions);
            return Parser.Parse(lexer) as PropertyArray;
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public object this[int i]
        {
            get
            {
                if (i < 0 || _list.Count <= i) throw new IndexOutOfRangeException();

                return _list[i];
            }
        }

        public string PersistToString()
        {
            return GetPersistString();
        }

        private string GetPersistString()
        {
            Writer writer = new Writer();
            writer.WriteArray(this);
            return writer.ToString();
        }

        public string StringAt(int i)
        {
            return this[i] as string;
        }

        public int? IntegerAt(int i)
        {
            return this[i] as int?;
        }

        public bool? BooleanAt(int i)
        {
            return this[i] as bool?;
        }

        public byte[] DataAt(int i)
        {
            return this[i] as byte[];
        }

        public PropertyArray ArrayAt(int i)
        {
            return this[i] as PropertyArray;
        }

        public PropertyDictionary DictionaryAt(int i)
        {
            return this[i] as PropertyDictionary;
        }

        public void AppendValue(object value)
        {
            _list.Add(value);
        }

        public void AppendValues(PropertyArray values)
        {
            foreach(object value in values)
            {
                _list.Add(value);
            }
        }

        public void AppendValues(IEnumerable<object> values)
        {
            foreach(object value in values)
            {
                _list.Add(value);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public object[] ToArray()
        {
            return _list.ToArray();
        }

        public override string ToString()
        {
            Dictionary<Type, int> typeCount = new Dictionary<Type, int>();

            foreach(object item in _list)
            {
                Type itemType = item.GetType();

                if (!typeCount.ContainsKey(itemType))    
                {
                    typeCount[itemType] = 0;
                }

                typeCount[itemType]++;
            }

            List<string> outputStrings = new List<string>();

            foreach(KeyValuePair<Type, int> pair in typeCount)
            {
                outputStrings.Add(string.Format("{0}: {1}", pair.Key.Name, pair.Value));
            }

            return NaturalStrings.FormatList(outputStrings, FormatListFormat.AllCommasList);
        }
    }
}
