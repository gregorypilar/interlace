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

namespace DatabaseCop.Rules
{
    public class PreferredWords : Rule
    {
        static Dictionary<string, string> _preferredWords;

        static PreferredWords()
        {
            _preferredWords = new Dictionary<string, string>();

            _preferredWords.Add("Phone", "Telephone");
            _preferredWords.Add("Ph", "Telephone");
            _preferredWords.Add("Fax", "Facsimile");
            _preferredWords.Add("Mob", "Facsimile");
            _preferredWords.Add("Note", "Notes");
            _preferredWords.Add("Suburb", "Locality");
            _preferredWords.Add("Speed", "Velocity");
            _preferredWords.Add("Bearing", "Heading");
            _preferredWords.Add("Direction", "Heading");
            _preferredWords.Add("Acc", "Account");
            _preferredWords.Add("Ref", "Reference");
            _preferredWords.Add("Pod", "ProofOfDelivery");
            _preferredWords.Add("Min", "Minimum");
            _preferredWords.Add("Max", "Maximum");
            _preferredWords.Add("Kilometre", "DistanceInKilometres");
            _preferredWords.Add("Kilometer", "DistanceInKilometres");
            _preferredWords.Add("Kg", "Weight");
            _preferredWords.Add("Kilo", "Weight");
            _preferredWords.Add("Kilos", "Weight");
            _preferredWords.Add("Cubic", "Volume");
            _preferredWords.Add("Cubics", "Volume");
            _preferredWords.Add("Horas", "Hours");
            _preferredWords.Add("Cus", "Customer");
            _preferredWords.Add("Dis", "Discount");
            _preferredWords.Add("Disc", "Discount");
            _preferredWords.Add("Dr", "Driver");
            _preferredWords.Add("Op", "Operations");
            _preferredWords.Add("Dg", "DangerousGoods");
            _preferredWords.Add("Reg", "Registered");
            _preferredWords.Add("Veh", "Vehicle");
        }

        public override void CheckColumn(ViolationReport report, Column column)
        {
            foreach (KeyValuePair<string, string> pair in _preferredWords)
            {
                if (column.ParsedName.ContainsWord(pair.Key))
                {
                    report.AddViolation(column, string.Format(
                        "The column contains the word \"{0}\", which should not be used. Prefer the " +
                        "word {1} instead.", pair.Key, pair.Value));
                }
            }
        }
    }
}
