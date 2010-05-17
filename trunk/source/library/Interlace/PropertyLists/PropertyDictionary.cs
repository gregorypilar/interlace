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
using System.Reflection;
using System.Text;

#endregion

namespace Interlace.PropertyLists
{
    [Serializable]
    public class PropertyDictionary : IEnumerable
    {
        Dictionary<object, object> _dictionary;

        internal PropertyDictionary()
        {
            _dictionary = new Dictionary<object, object>();
        }

        public static PropertyDictionary FromFileInExecutableDirectory(string relativePath)
        {
            string path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), relativePath);

            return FromFile(path);
        }

        public static PropertyDictionary FromStream(Stream stream, string nameForExceptions)
        {
            if (stream.Length > 0)
            {
                using (TextReader reader = new StreamReader(stream))
                {
                    return FromReader(reader, nameForExceptions);
                }
            }
            else
            {
                return PropertyDictionary.EmptyDictionary();
            }
        }

        public static PropertyDictionary FromReader(TextReader reader, string nameForExceptions)
        {
            Lexer lexer = new Lexer(reader, nameForExceptions);
            return Parser.Parse(lexer) as PropertyDictionary;
        }

        public static PropertyDictionary FromStream(Stream stream)
        {
            return PropertyDictionary.FromStream(stream, "unnamed stream");
        }

        public static PropertyDictionary FromReader(TextReader reader)
        {
            return PropertyDictionary.FromReader(reader, "unnamed stream");
        }

        public static PropertyDictionary FromFile(string path)
        {
            if (!File.Exists(path)) return null;

            using (StreamReader reader = new StreamReader(path))
            {
                return PropertyDictionary.FromReader(reader, path);
            }
        }

        public static PropertyDictionary FromString(string text)
        {
            if (string.IsNullOrEmpty(text)) return PropertyDictionary.EmptyDictionary();

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return PropertyDictionary.FromReader(reader, "text string");
                }
            }
        }

        public static PropertyDictionary EmptyDictionary()
        {
            return new PropertyDictionary();
        }

        public void PersistToFileInExecutableDirectory(string relativePath)
        {
            string path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), relativePath);

            PersistToFile(path);
        }

        public void PersistToFile(string path)
        {
            FileStream file = File.Open(path, FileMode.Create);

            byte[] bytes = Encoding.ASCII.GetBytes(GetPersistString());
            file.Write(bytes, 0, bytes.Length);

            file.Close();
        }

        public void PersistToStream(Stream stream)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(GetPersistString());

            stream.Write(bytes, 0, bytes.Length);
        }

        string GetPersistString()
        {
            Writer writer = new Writer();
            writer.WriteDictionary(this);
            return writer.ToString();
        }

        public byte[] PersistToByteArray()
        {
            return Encoding.ASCII.GetBytes(GetPersistString());
        }

        public string PersistToString()
        {
            return GetPersistString();
        }

        private string AppendIndent(string value, int indentLevel)
        {
            for (int i = 0; i < indentLevel; ++i)
                value += "\t";

            return value;
        }

        public bool HasValueFor(params object[] keys)
        {
            foreach (object key in keys)
            {
                if (!_dictionary.ContainsKey(key)) return false;
            }

            return true;
        }

        public bool HasStringFor(params object[] keys)
        {
            foreach (object key in keys)
            {
                if (!_dictionary.ContainsKey(key) || !(_dictionary[key] is string)) return false;
            }

            return true;
        }

        public bool HasIntegerFor(params object[] keys)
        {
            foreach (object key in keys)
            {
                if (!_dictionary.ContainsKey(key) || !(_dictionary[key] is int)) return false;
            }

            return true;
        }

        public bool HasDoubleFor(params object[] keys)
        {
            foreach (object key in keys)
            {
                if (!_dictionary.ContainsKey(key) || !(_dictionary[key] is double || _dictionary[key] is int)) return false;
            }

            return true;
        }

        public bool HasDictionaryFor(params object[] keys)
        {
            foreach (object key in keys)
            {
                if (!_dictionary.ContainsKey(key) || !(_dictionary[key] is PropertyDictionary)) return false;
            }

            return true;
        }

        public bool HasArrayFor(params object[] keys)
        {
            foreach (object key in keys)
            {
                if (!_dictionary.ContainsKey(key) || !(_dictionary[key] is PropertyArray)) return false;
            }

            return true;
        }

        public object ValueFor(object key)
        {
            if (_dictionary.ContainsKey(key))
            {
                return _dictionary[key];
            }
            else
            {
                return null;
            }
        }

        public object ValueFor(object key, object defaultValue)
        {
            if (_dictionary.ContainsKey(key))
            {
                return _dictionary[key];
            }
            else
            {
                return defaultValue;
            }
        }

        public string StringFor(object key)
        {
            return ValueFor(key) as string;
        }

        public string StringFor(object key, string defaultValue)
        {
            string value = ValueFor(key) as string;

            return value != null ? value : defaultValue;
        }

        public int? IntegerFor(object key)
        {
            return ValueFor(key) as int?;
        }

        public double? DoubleFor(object key)
        {
            object value = ValueFor(key);

            if (value is int) return (double)(int)value;

            return value as double?;
        }

        public int IntegerFor(object key, int defaultValue)
        {
            int? value = ValueFor(key) as int?;

            return value != null ? (int)value : defaultValue;
        }

        public bool? BooleanFor(object key)
        {
            return ValueFor(key) as bool?;
        }

        public bool BooleanFor(object key, bool defaultValue)
        {
            bool? value = ValueFor(key) as bool?;

            return value != null ? (bool)value : defaultValue;
        }

        public byte[] DataFor(object key)
        {
            return ValueFor(key) as byte[];
        }

        public byte[] DataFor(object key, byte[] defaultValue)
        {
            byte[] value = ValueFor(key) as byte[];

            return value != null ? (byte[])value : defaultValue;
        }

        public PropertyArray ArrayFor(object key)
        {
            return ValueFor(key) as PropertyArray;
        }

        public PropertyDictionary DictionaryFor(object key)
        {
            return ValueFor(key) as PropertyDictionary;
        }

        public void SetValueFor(object key, object value)
        {
            _dictionary[key] = value;
        }

        public object[] Keys
        {
            get
            {
                object[] array = new object[_dictionary.Count];

                _dictionary.Keys.CopyTo(array, 0);

                return array;
            }
        }

        public object this[object key]
        {
            get { return _dictionary[key]; }
            set { _dictionary[key] = value; }
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        #endregion
    }
}
