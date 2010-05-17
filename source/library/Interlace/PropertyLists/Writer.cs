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
using System.Text.RegularExpressions;

#endregion

namespace Interlace.PropertyLists
{
    class Writer
    {
        StringBuilder _builder = new StringBuilder();
        const int _indentSpaces = 4;
        int _currentIndentLevel = 0;
        bool _atLineStart = true;

        Regex _isPlainString = new Regex("^[A-Za-z]+$");
        Regex _isTrue = new Regex("^true$");
        Regex _isFalse = new Regex("^false$");
        Regex _isQuotableCharacter = new Regex("(\\n|\"|\\\\)");

        void EnsureIndentWritten()
        {
            if (_atLineStart)
            {
                _builder.Append("".PadLeft(_currentIndentLevel * _indentSpaces));

                _atLineStart = false;
            }
        }

        void Append(string format, params object[] args)
        {
            EnsureIndentWritten();
            _builder.Append(string.Format(format, args));
        }

        void AppendLine(string format, params object[] args)
        {
            EnsureIndentWritten();
            _builder.AppendLine(string.Format(format, args));
            _atLineStart = true;
        }

        void Indent()
        {
            _currentIndentLevel++;
        }

        void Dedent()
        {
            _currentIndentLevel--;
        }

        string ReplaceQuotedCharacter(Match match)
        {
            switch (match.Groups[1].Value)
            {
                case "\n":
                    return "\\n";
                case "\\":
                    return "\\\\";
                case "\"":
                    return "\\\"";
                default:
                    throw new InvalidOperationException();
            }
        }

        void WriteObject(object obj)
        {
            if (obj is string)
            {
                string str = (string)obj;

                // If it's not a plain string, it needs quoting.  The strings "true" and "false" also
                // need quoting so they aren't parsed back in as booleans.
                if (_isPlainString.IsMatch(str) && !_isTrue.IsMatch(str) && !_isFalse.IsMatch(str))
                {
                    Append((string)obj);
                }
                else
                {
                    Append("\"{0}\"", _isQuotableCharacter.Replace(str, ReplaceQuotedCharacter));
                }
            }
            else if (obj is int || obj is byte || obj is short)
            {
                Append("{0}", Convert.ToInt32(obj));
            }
            else if (obj is double)
            {
                Append("{0}", Convert.ToDouble(obj));
            }
            else if (obj is bool)
            {
                Append("{0}", obj.ToString().ToLower());
            }
            else if (obj is PropertyDictionary)
            {
                WriteDictionary(obj as PropertyDictionary);
            }
            else if (obj is PropertyArray)
            {
                WriteArray(obj as PropertyArray);
            }
            else if (obj is DateTime)
            {
                Append("\"{0}\"", ((DateTime)obj).ToUniversalTime().ToString("s"));
            }
            else
            {
                throw new NotImplementedException(
                    "Only strings, integers, and other property dictionaries can be persisted.");
            }
        }

        public void WriteArray(PropertyArray array)
        {
            AppendLine("{0}", "(");
            Indent();

            for (int i = 0; i < array.Count; i++)
            {
                WriteObject(array[i]);
                if (i != array.Count - 1) AppendLine(", ");
            }

            Dedent();
            Append("{0}", ")");
        }

        public void WriteDictionary(PropertyDictionary dictionary)
        {
            AppendLine("{0}", "{");
            Indent();

            foreach (object key in dictionary.Keys)
            {
                object value = dictionary.ValueFor(key);

                if (value == null) continue;

                WriteObject(key);
                Append(" = ");
                WriteObject(value);
                AppendLine(";");
            }

            Dedent();
            Append("{0}", "}");
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
