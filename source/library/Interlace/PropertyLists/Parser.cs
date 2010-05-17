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

namespace Interlace.PropertyLists
{
    class Parser
    {
        public static object Parse(Lexer lexer)
        {
            return ParseObject(lexer, Token.None);
        }

        static object ParseObject(Lexer lexer, Token savedToken)
        {
            Token token = savedToken.Kind == TokenKind.None ? lexer.Next() : savedToken;

            // TODO: This should be done properly instead of just assuming it's a date time.
            // Date Time handling.
            //DateTime possibleDateValue;

            //if (token.Value is string && token.Kind == TokenKind.Literal &&
            //    DateTime.TryParse(token.Value as string, out possibleDateValue))
            //{
            //    return possibleDateValue;
            //}

            switch (token.Kind)
            {
                case TokenKind.Literal:
                    return token.Value;

                case TokenKind.ArrayOpen:
                    return ParseArray(lexer);

                case TokenKind.DictionaryOpen:
                    return ParseDictionary(lexer);

                case TokenKind.DictionaryClose:
                case TokenKind.ArrayClose:
                case TokenKind.Equals:
                case TokenKind.Separator:
                case TokenKind.End:
                default:
                    ThrowBecauseOfAnUnexpectedToken(lexer, token);
                    return null;
            }
        }

        private static object ParseDictionary(Lexer lexer)
        {
            PropertyDictionary dictionary = new PropertyDictionary();

            while (true)
            {
                Token token = lexer.Next();

                if (token.Kind == TokenKind.DictionaryClose) break;

                object key = ParseObject(lexer, token);

                Token equals = lexer.Next();
                if (equals.Kind != TokenKind.Equals) ThrowBecauseOfAnUnexpectedToken(lexer, equals);

                object value = ParseObject(lexer, Token.None);

                dictionary.SetValueFor(key, value);

                Token terminator = lexer.Next();

                if (terminator.Kind == TokenKind.DictionaryClose) break;
                if (terminator.Kind == TokenKind.Separator) continue;

                ThrowBecauseOfAnUnexpectedToken(lexer, terminator);
            }

            return dictionary;
        }

        private static object ParseArray(Lexer lexer)
        {
            PropertyArray array = new PropertyArray();

            while (true)
            {
                Token token = lexer.Next();

                if (token.Kind == TokenKind.ArrayClose) break;

                object value = ParseObject(lexer, token);

                array.AppendValue(value);

                Token terminator = lexer.Next();

                if (terminator.Kind == TokenKind.ArrayClose) break;
                if (terminator.Kind == TokenKind.Separator) continue;

                ThrowBecauseOfAnUnexpectedToken(lexer, terminator);
            }

            return array;
        }

        static void ThrowBecauseOfAnUnexpectedToken(Lexer lexer, Token token)
        {
            string description = "";

            switch (token.Kind)
            {
                case TokenKind.Literal:
                    description = "literal";
                    break;

                case TokenKind.ArrayOpen:
                    description = "array opening character";
                    break;

                case TokenKind.ArrayClose:
                    description = "array closing character";
                    break;

                case TokenKind.DictionaryOpen:
                    description = "dictionary opening character";
                    break;

                case TokenKind.DictionaryClose:
                    description = "dictionary closing character";
                    break;

                case TokenKind.Equals:
                    description = "equals character";
                    break;

                case TokenKind.Separator:
                    description = "separator character";
                    break;

                case TokenKind.End:
                    description = "end of file";
                    break;

                default:
                    description = "unknown token";
                    break;
            }

            throw new PropertyListException(string.Format(
                "An unexpected {0} was found on line {1} of the configuration " +
                "file \"{2}\".", description, token.LineNumber, lexer.NameForExceptions));
        }
    }
}
