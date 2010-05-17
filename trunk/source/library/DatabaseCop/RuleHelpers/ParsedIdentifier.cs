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

using Interlace.Collections;

#endregion

namespace DatabaseCop.RuleHelpers
{
    [Serializable]
    public class ParsedIdentifier
    {
        List<ParsedWord> _words;
        string _value;

        static readonly Regex _parserRegex = new Regex(
            @"\G(?:([A-Z][a-z]+)|([A-Z]+(?![a-z]))|([a-z]+)|([0-9]+)|([-_ ])|(.))");

        private ParsedIdentifier(IList<ParsedWord> words, int beginning, int end)
        {
            _words = new List<ParsedWord>();

            for (int i = beginning; i < end; i++)
            {
                _words.Add(words[i]);
            }

            _value = GetIdentifier(_words);
        }

        public ParsedIdentifier(string identifier)
        {
            _words = new List<ParsedWord>();

            Match match = _parserRegex.Match(identifier);

            while (match.Success)
            {
                ParsedWordFlags flags;

                if (match.Groups[1].Success)
                {
                    flags = ParsedWordFlags.ProperCase;
                }
                else if (match.Groups[2].Success)
                {
                    flags = ParsedWordFlags.UpperCase;

                    if (match.Value.Length == 1) flags |= ParsedWordFlags.ProperCase;
                }
                else if (match.Groups[3].Success)
                {
                    flags = ParsedWordFlags.LowerCase;
                }
                else if (match.Groups[4].Success)
                {
                    flags = ParsedWordFlags.Number;
                }
                else if (match.Groups[5].Success)
                {
                    flags = ParsedWordFlags.Spacer;
                }
                else 
                {
                    flags = ParsedWordFlags.Junk;
                }

                _words.Add(new ParsedWord(match.Value, flags));

                match = match.NextMatch();
            }

            _value = GetIdentifier(_words);
        }

        protected static string GetIdentifier(IList<ParsedWord> words)
        {
            StringBuilder identifierBuilder = new StringBuilder();

            foreach (ParsedWord word in words)
            {
                identifierBuilder.Append(word.Value);
            }

            return identifierBuilder.ToString();
        }

        public string Value
        {
            get 
            {
                return _value;
            }
        }

        protected List<ParsedWord> InternalWords
        {
            get { return _words; }
        }

        protected string InternalValue
        {
            get { return _value; }
            set { _value = value; }
        }

        public ICollection<ParsedWord> Words
        {
            get { return _words; }
        }

        public ParsedWord FirstWord
        {
            get
            {
                return _words[0];
            }
        }

        public ParsedWord LastWord
        {
            get
            {
                return _words[_words.Count - 1];
            }
        }

        public ParsedIdentifier GetRange(int beginning)
        {
            return GetRange(beginning, int.MaxValue);
        }

        public ParsedIdentifier GetRange(int beginning, int end)
        {
            if (beginning < 0) beginning = _words.Count + beginning;
            if (end < 0) end = _words.Count + end;

            beginning = Math.Max(Math.Min(beginning, _words.Count), 0);
            end = Math.Max(Math.Min(end, _words.Count), 0);

            return new ParsedIdentifier(_words, beginning, end);
        }

        public bool ContainsWord(string candidateWord)
        {
            foreach (ParsedWord word in _words)
            {
                if (word.Value == candidateWord) return true;
            }

            return false;
        }

        public bool ContainsOneOfWord(Set<string> candidateWords)
        {
            foreach (ParsedWord word in _words)
            {
                if (candidateWords.Contains(word.Value)) return true;
            }

            return false;
        }
    }
}
