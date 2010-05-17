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

using Interlace.Collections;
using Interlace.Utilities;

using DatabaseCop.RuleHelpers;

#endregion

namespace DatabaseCop.Rules
{
    public class IdReferenceWithoutMatchingTable : Rule
    {
        static readonly Set<string> _specialReferenceNames = new Set<string>(
            "Universal", "Parent");

        static readonly Set<string> _qualifierGlueWords = new Set<string>(
            "For", "With", "From", "By", "Into");

        public override void CheckColumn(ViolationReport report, Column column)
        {
            if (column.ParsedName.LastWord.Value == "Id" && column.Name != "Id")
            {
                ParsedIdentifier fullTableName = column.ParsedName.GetRange(0, -1);
                ParsedIdentifier nonQualifiedTableName = column.ParsedName.GetRange(1, -1);

                if (nonQualifiedTableName.Words.Count > 1 && 
                    _qualifierGlueWords.Contains(nonQualifiedTableName.FirstWord.Value))
                {
                    nonQualifiedTableName = nonQualifiedTableName.GetRange(1);
                }

                if (!(_specialReferenceNames.Contains(fullTableName.Value) ||
                    column.Table.Database.TablesByShortName.ContainsKey(fullTableName.Value) ||
                    column.Table.Database.TablesByShortName.ContainsKey(nonQualifiedTableName.Value)))
                {
                    report.AddViolation(column, 
                        "The column ends with the word \"Id\" but no table exists that this " +
                        "column should reference.");
                }
            }

            base.CheckColumn(report, column);
        }
    }
}
