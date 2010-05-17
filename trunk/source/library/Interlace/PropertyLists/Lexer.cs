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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Interlace.PropertyLists
{
    class Lexer
    {
        TextReader _reader;
        int _lineNumber;
        string _nameForExceptions;

        public Lexer(TextReader reader, string nameForExceptions)
        {
            _reader = reader;
            _lineNumber = 1;
            _nameForExceptions = nameForExceptions;
        }

        internal string NameForExceptions
        {
            get { return _nameForExceptions; }
        }

        public Token Next()
        {
            while (true)
            {
                int ic = _reader.Read();
                char c = (char)ic;

                if (ic == -1) return new Token(TokenKind.End, _lineNumber);

                switch (c)
                {
                    case '{':
                        return new Token(TokenKind.DictionaryOpen, _lineNumber);

                    case '}':
                        return new Token(TokenKind.DictionaryClose, _lineNumber);

                    case '(':
                        return new Token(TokenKind.ArrayOpen, _lineNumber);

                    case ')':
                        return new Token(TokenKind.ArrayClose, _lineNumber);

                    case '=':
                        return new Token(TokenKind.Equals, _lineNumber);

                    case ';':
                    case ',':
                        return new Token(TokenKind.Separator, _lineNumber);

                    case '/':
                        ConsumeComment();
                        continue;
                }

                // Unquoted string support:
                if (char.IsLetter(c) || c == '_') return NextUnquotedString(c);

                // Quoted string support:
                if (c == '"') return NextQuotedString(c);

                if (char.IsDigit(c) || c == '-' || c == '.') return NextNumber(c);

                if (c == '\n') _lineNumber++;

                if (!char.IsWhiteSpace(c))
                {
                    throw new PropertyListException(string.Format(
                        "An invalid character (\"{0}\") was found on line {1} of \"{2}\".",
                        c, _lineNumber, _nameForExceptions));
                }
            }
        }

        void ConsumeComment()
        {
            int ic = _reader.Read();
            char c = (char)ic;

            if (ic == -1)
            {
                throw new PropertyListException(string.Format(
                    "An end of file was found at the start of a comment in line {0} of \"{1}\".",
                    _lineNumber, _nameForExceptions));
            }

            if (c != '/')
            {
                throw new PropertyListException(string.Format(
                    "An invalid character (\"{0}\") was found on line {1} of \"{2}\".",
                    c, _lineNumber, _nameForExceptions));
            }

            do
            {
                ic = _reader.Read();
                c = (char)ic;
            }
            while (ic != -1 && c != '\n');
        }

        static bool IsIdentifierCharacter(char c)
        {
            return char.IsLetterOrDigit(c) || c == '-' || c == '_';
        }

        Token NextUnquotedString(char c)
        {
            StringBuilder s = new StringBuilder();
            s.Append(c);

            while (IsIdentifierCharacter((char)_reader.Peek()))
            {
                s.Append((char)_reader.Read());
            }

            // Special handling for booleans.
            if (s.ToString() == "true")
            {
                return new Token(TokenKind.Literal, _lineNumber, true);
            }
            else if (s.ToString() == "false")
            {
                return new Token(TokenKind.Literal, _lineNumber, false);
            }
            else
            {
                return new Token(TokenKind.Literal, _lineNumber, s.ToString());
            }
        }

        Token NextQuotedString(char c)
        {
            StringBuilder s = new StringBuilder();
            bool escaping = false;

            while (true)
            {
                int ic = _reader.Read();
                c = (char)ic;

                if (ic == -1) throw new PropertyListException(string.Format(
                    "A string literal in \"{0}\" was not closed before the file ended.",
                    _nameForExceptions));

                if (escaping)
                {
                    if (c == 'n') c = '\n';
                    if (c == 'r') c = '\r';
                    if (c == 't') c = '\t';
                    if (c == '"') c = '\"';

                    // Otherwise, just let the character through.

                    escaping = false;
                }
                else
                {
                    if (c == '\\')
                    {
                        escaping = true;
                        continue;
                    }

                    if (c == '"') break;
                }

                if (c == '\n') _lineNumber++;

                s.Append(c);
            }

            return new Token(TokenKind.Literal, _lineNumber, s.ToString());
        }

        static readonly Regex _doubleRegex = new Regex(@"^-?(\d*\.\d*|\d+|\.\d*)([Ee][-+]?\d*)?$");
        static readonly Regex _integerRegex = new Regex(@"^-?\d*$");

        Token NextNumber(char c)
        {
            string value = "";

            value += c;

            while (true)
            {
                int next = _reader.Peek();

                if (next == -1) break;

                string newValue = value + (char)next;

                if (!_doubleRegex.IsMatch(newValue)) break;

                _reader.Read();

                value = newValue;
            }

            if (_integerRegex.IsMatch(value))
            {
                int integerValue;

                if (!int.TryParse(value, out integerValue))
                {
                    throw new PropertyListException(string.Format(
                        "An invalid integer was found on line {0} of \"{1}\".",
                        _lineNumber, _nameForExceptions));
                }

                return new Token(TokenKind.Literal, _lineNumber, integerValue);
            }
            else
            {
                double doubleValue;

                if (!double.TryParse(value, out doubleValue))
                {
                    throw new PropertyListException(string.Format(
                        "An invalid double was found on line {0} of \"{1}\".",
                        _lineNumber, _nameForExceptions));
                }

                return new Token(TokenKind.Literal, _lineNumber, doubleValue);
            }
        }
    }
}
