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
    public class FormattingPropertyDictionaryWrapper :  IFormattable
    {
        PropertyDictionary _dictionary;

        public static FormattingPropertyDictionaryWrapper Wrap(PropertyDictionary dictionary)
        {
            FormattingPropertyDictionaryWrapper wrapper = new FormattingPropertyDictionaryWrapper();
            wrapper._dictionary = dictionary;

            return wrapper;
        }

        private FormattingPropertyDictionaryWrapper()
            :base()
        {}

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format.Contains(",") || format.Contains(";"))
            {
                string[] fieldArray = format.Split(';')[0].Split(',');
                string separator = format.Split(';')[1];

                List<string> fieldResults = new List<string>();

                foreach (string field in fieldArray)
                {
                    if (_dictionary.HasValueFor(field.Trim()))
                    {
                        fieldResults.Add(_dictionary.ValueFor(field.Trim()).ToString());
                    }
                }

                return string.Join(separator, fieldResults.ToArray());
            }
            else if (_dictionary.HasValueFor(format))
            {
                object value = _dictionary.ValueFor(format);

                return value == null ? null : value.ToString();
            }

            return null;
        }

        #endregion
    }
}
