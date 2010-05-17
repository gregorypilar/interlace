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

namespace Interlace.Utilities
{
    public static class NullableDateTime
    {
        public static DateTime? ToLocal(DateTime? t)
        {
            if (t.HasValue)
            {
                return t.Value.ToLocalTime();
            }
            else
            {
                return null;
            }
        }

        public static DateTime? ToUniversal(DateTime? t)
        {
            if (t.HasValue)
            {
                return t.Value.ToUniversalTime();
            }
            else
            {
                return null;
            }
        }


        public static DateTime? Date(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return dateTime.Value.Date;
            }
            else
            {
                return null;
            }
        }

        public static DateTime? CombineDateAndTime(DateTime? date, DateTime? time)
        {
            if (!date.HasValue || !time.HasValue) return null;

            return new DateTime(date.Value.Date.Ticks, time.Value.Kind) + time.Value.TimeOfDay;
        }

        public static DateTime CombineDateAndTime(DateTime date, DateTime time)
        {
            return new DateTime(date.Date.Ticks, time.Kind) + time.TimeOfDay;
        }

        public static DateTime? Min(DateTime? first, DateTime? second)
        {
            if (!first.HasValue && !second.HasValue)
            {
                return null;
            }
            else if (first.HasValue && !second.HasValue)
            {
                return first.Value;
            }
            else if (!first.HasValue && second.HasValue)
            {
                return second.Value;
            }
            else
            {
                return first.Value < second.Value ? first : second;
            }
        }

        public static DateTime? Max(DateTime? first, DateTime? second)
        {
            if (!first.HasValue && !second.HasValue)
            {
                return null;
            }
            else if (first.HasValue && !second.HasValue)
            {
                return first.Value;
            }
            else if (!first.HasValue && second.HasValue)
            {
                return second.Value;
            }
            else
            {
                return first.Value > second.Value ? first : second;
            }
        }
    }
}
