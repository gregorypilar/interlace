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

namespace Interlace.Utilities
{
    public static class NaturalParsers
    {
        static readonly Regex _percentageRegex = new Regex(@"^\s*(-)?\s*(\d+|\d+\.\d*|\d*\.\d+)\s*%?\s*$");

        public static double ParsePercentage(string value)
        {
            double? percentage = TryParsePercentage(value);

            if (!percentage.HasValue)
            {
                throw new FormatException("A percentage is expected.");
            }

            return percentage.Value;
        }

        public static double? TryParsePercentage(string value)
        {
            Match match = _percentageRegex.Match(value);

            if (!match.Success) return null;

            double percentage = double.Parse(match.Groups[2].Value) / 100.0;

            if (match.Groups[1].Success) percentage = -percentage;

            return percentage;
        }

        static readonly Regex _moneyRegex = new Regex(@"^\s*(-)?\s*\$?\s*(-)?\s*([,0-9]+|[,0-9]+\.[,0-9]*|[,0-9]*\.[,0-9]+)\s*$");

        public static decimal? TryParseMoney(string value)
        {
            Match match = _moneyRegex.Match(value);

            if (!match.Success) return null;

            decimal money = decimal.Parse(match.Groups[3].Value.Replace(",", ""));

            if (match.Groups[1].Success || match.Groups[2].Success) money = -money;

            return money;
        }

        public static decimal ParseMoney(string value)
        {
            decimal? money = TryParseMoney(value);

            if (!money.HasValue)
            {
                throw new FormatException("An amount of money is expected.");
            }

            return money.Value;
        }

    	private static readonly Regex _timeRegex = new Regex(@"^\s*(\d{1,2})(?:\D+(\d{1,2}))?\s*(?:(a|p|A|P)(?:m|M)?)?\s*$");
    	private static readonly Regex _militaryTimeRegex = new Regex(@"^\s*(\d{2})(\d{2})\s*$");

    	public static TimeSpan? TryParseTime(string timeString)
    	{
    		if (_timeRegex.IsMatch(timeString))
    		{
    			Match match = _timeRegex.Match(timeString);

    			int hours = int.Parse(match.Groups[1].Value);
    			int minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                string upperCasePeriod = match.Groups[3].Success ? match.Groups[3].Value.ToUpper() : null;

    			if ((hours < 0 || 23 < hours) || (minutes < 0 || 59 < minutes)) return null;

                if (upperCasePeriod == null)
                {
                    if (1 <= hours && hours <= 6) hours += 12;
                }
                else if (upperCasePeriod == "A")
                {
                    if (hours == 12) hours = 0;

                    if (hours > 12) return null;
                }
                else if (upperCasePeriod == "P")
                {
                    if (hours == 0) return null;

                    if (1 <= hours && hours <= 11) hours += 12;
                }

    			return new TimeSpan(hours, minutes, 0);
    		}
            else if (_militaryTimeRegex.IsMatch(timeString)) 
            {
    			Match match = _militaryTimeRegex.Match(timeString);

    			int hours = int.Parse(match.Groups[1].Value);
    			int minutes = int.Parse(match.Groups[2].Value);

    			if ((hours < 0 || 23 < hours) || (minutes < 0 || 59 < minutes)) return null;

    			return new TimeSpan(hours, minutes, 0);
    		}
    		else
    		{
                return null;
    		}
    	}

        public static TimeSpan ParseTime(string value)
        {
            TimeSpan? span = TryParseTime(value);

            if (!span.HasValue)
            {
                throw new FormatException("A time of day is expected.");
            }

            return span.Value;
        }
    }
}
