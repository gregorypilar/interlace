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
    /// <summary>
    /// The format of a formatted list of items; used by natural string to determine what kind of
    /// sentence the list is part of.
    /// </summary>
    public enum FormatListFormat
    {
        /// <summary>A list ending with "and"; for example, beans, peas and cabbage.</summary>
        AndList,

        /// <summary>A list ending with "or"; for example, red, blue, green or black.</summary>
        OrList,

        /// <summary>A list with all commas; for example, 1, 2, 3, 4.</summary>
        AllCommasList,
    }

    /// <summary>
    /// Specifies if abbreviations should be used when formatting a time or time period.
    /// </summary>
    public enum FormatTimeFormat
    {
        /// <summary>Disables the use of any abbreviations.</summary>
        Full,

        /// <summary>Specifies that abbreviations should be used.</summary>
        Compact,
        CompactWithSeconds
    }

    /// <summary>
    /// A set of utility functions for formatting and parsing english language strings.
    /// </summary>
    public static class NaturalStrings
    {
        public static string FormatList<T>(IEnumerable<T> list)
        {
            return FormatList<T>(list, FormatListFormat.AndList, "{0}");
        }

        public static string FormatList<T>(IEnumerable<T> list, string itemStringFormat)
        {
            return FormatList<T>(list, FormatListFormat.AndList, itemStringFormat);
        }

        public static string FormatList<T>(IEnumerable<T> list, FormatListFormat format)
        {
            return FormatList<T>(list, format, "{0}");
        }

        /// <summary>
        /// Formats a list of items into a natural language list; for example,
        /// "cow", "cow and dog", "cow, dog and rat" are typical results.
        /// </summary>
        /// <param name="list">The list of items.</param>
        /// <param name="format">The format of the list.</param>
        /// <param name="itemStringFormat">The format string to apply to all elements.</param>
        /// <returns>The printed list.</returns>
        public static string FormatList<T>(IEnumerable<T> list, FormatListFormat format, string itemStringFormat)
        {
            if (!itemStringFormat.Contains("{0")) 
            {
                throw new ArgumentException(
                    "The item string format must contain a format for the zeroth argument.", itemStringFormat);
            }

            StringBuilder formatted = new StringBuilder();

            string previousItem = null;

            foreach (T item in list)
            {
                string thisItem = string.Format(itemStringFormat, item);

                if (previousItem != null)
                {
                    if (formatted.Length != 0) formatted.Append(", ");
                    formatted.Append(previousItem);
                }

                previousItem = thisItem;
            }

            if (previousItem != null)
            {
                string lastJoiner;

                switch (format)
                {
                    case FormatListFormat.AllCommasList:
                        lastJoiner = ", ";
                        break;

                    case FormatListFormat.OrList:
                        lastJoiner = " or ";
                        break;

                    case FormatListFormat.AndList:
                    default:
                        lastJoiner = " and ";
                        break;
                }

                if (formatted.Length != 0) formatted.Append(lastJoiner);
                formatted.Append(previousItem);
            }

            return formatted.ToString();
        }

        public static string FormatTimeUntil(TimeSpan time, FormatTimeFormat format)
        {
            return FormatTimeUntil(time, format, "{0}", "{0} ago");
        }

    	public static string FormatTimeUntil(TimeSpan time, FormatTimeFormat format,
            string positiveFormat, string negativeFormat)
    	{
            string positiveDescription;
            TimeSpan positiveTime = time > TimeSpan.Zero ? time : time.Negate();

            if (format == FormatTimeFormat.Compact || format == FormatTimeFormat.CompactWithSeconds)
            {
                if (positiveTime.Days > 0)
                {
                    positiveDescription = string.Format("{0} d, {1} h",
                        positiveTime.Days, positiveTime.Hours);
                }
                else if (positiveTime.Hours > 0)
                {
                    positiveDescription = string.Format("{0} h, {1} m",
                        positiveTime.Hours, positiveTime.Minutes);
                }
                else if (positiveTime.Minutes > 0)
                {
                    positiveDescription = string.Format(
                        format == FormatTimeFormat.CompactWithSeconds ? "{0} m, {1} s" : "{0} m",
                        positiveTime.Minutes, positiveTime.Seconds);
                }
                else if (positiveTime.Seconds > 0)
                {
                    if (format == FormatTimeFormat.CompactWithSeconds)
                    {
                        positiveDescription = string.Format("{0} s", positiveTime.Seconds);
                    }
                    else
                    {
                        positiveDescription = "1 m";
                    }
                }
                else
                {
                    positiveDescription = "";
                }
            }
            else
            {
                if (positiveTime.Days > 0)
                {
                    positiveDescription = string.Format("{0} {1}, {2} {3}",
                        positiveTime.Days, positiveTime.Days == 1 ? "day" : "days",
                        positiveTime.Hours, positiveTime.Hours == 1 ? "hour" : "hours");
                }
                else if (positiveTime.Hours > 0)
                {
                    positiveDescription = string.Format("{0} {1}, {2} {3}",
                        positiveTime.Hours, positiveTime.Hours == 1 ? "hour" : "hours",
                        positiveTime.Minutes, positiveTime.Minutes == 1 ? "minute" : "minutes");
                }
                else if (positiveTime.Minutes > 0)
                {
                    positiveDescription = string.Format("{0} {1}",
                        positiveTime.Minutes, positiveTime.Minutes == 1 ? "minute" : "minutes");
                }
                else
                {
                    positiveDescription = "1 minute";
                }
            }

            return string.Format(time >= TimeSpan.Zero ? positiveFormat : negativeFormat,
                positiveDescription);
    	}
    }
}
