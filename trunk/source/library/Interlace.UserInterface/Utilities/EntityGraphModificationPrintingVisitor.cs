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

using SD.LLBLGen.Pro.ORMSupportClasses;

using Interlace.Collections;

#endregion

namespace Interlace.Utilities
{
    public class EntityGraphModificationPrintingVisitor : EntityGraphVisitor
    {
        StringBuilder _builder;

        public EntityGraphModificationPrintingVisitor()
        {
            _builder = new StringBuilder();
        }

        public string Result
        {
            get { return _builder.ToString(); }
        }

        public override void VisitEntity(CactusStack<IEntity2> current)
        {
            // Build a list of primary key fields:
            List<IEntityField2> primaryKeyFields = new List<IEntityField2>(current.Value.PrimaryKeyFields);
            List<IEntityField2> dirtyFields = new List<IEntityField2>();

            // Print the details:
            string prefix;

            if (current.Value.IsNew)
            {
                prefix = "[New]    ";
            }
            else if (current.Value.IsDirty)
            {
                prefix = "[Dirty]  ";
            }
            else
            {
                prefix = "         ";
            }

            string line = string.Format("{0}{1}{2} ({3}{4}{5})", 
                prefix,
                "".PadLeft((current.Count - 1) * 4),
                EntityGraphPrintingVisitor.GetShortEntityName(current.Value), 
                EntityGraphPrintingVisitor.GetFieldValuesDebugString(current.Value.PrimaryKeyFields, false),
                current.Value.IsDirty ? "; " : "",
                EntityGraphPrintingVisitor.GetFieldValuesDebugString(EntityGraphPrintingVisitor.GetDirtyFields(current.Value), true));

            _builder.AppendLine(line);
        }
    }
}
