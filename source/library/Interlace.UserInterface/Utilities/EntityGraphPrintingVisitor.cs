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
    internal class EntityGraphPrintingVisitor : EntityGraphVisitor
    {
        ReferenceLabeller _labeller;
        StringBuilder _builder;
        string[] _fieldsToDisplay;

        public EntityGraphPrintingVisitor(string[] fieldsToDisplay)
        {
            _labeller = new ReferenceLabeller();
            _builder = new StringBuilder();
            _fieldsToDisplay = fieldsToDisplay;

            if (_fieldsToDisplay == null) _fieldsToDisplay = new string[] { };
        }

        public string Result
        {
            get { return _builder.ToString(); }
        }

        public override void VisitEntity(CactusStack<IEntity2> current)
        {
            PrintCurrentEntity(_builder, current.Count - 1, current.Value, _labeller, _fieldsToDisplay);
        }

        public override void  VisitAlreadyVisitedEntity(CactusStack<IEntity2> current)
        {
            PrintCurrentEntity(_builder, current.Count - 1, current.Value, _labeller, _fieldsToDisplay, "Already Visited");
        }

        internal static string GetValueDebugString(object value)
        {
            if (value == null) return "null";

            if (value is string) return string.Format("\"{0}\"", value);

            return string.Format("{0}", value);
        }

        internal static string GetFieldValuesDebugString(IEnumerable<IEntityField2> fields, bool includePreviousValues)
        {
            List<string> fieldStrings = new List<string>();

            foreach (IEntityField2 field in fields)
            {
                fieldStrings.Add(string.Format(includePreviousValues ? "{0}: {1} -> {2}" : "{0}: {2}",
                    field.Name, GetValueDebugString(field.DbValue), GetValueDebugString(field.CurrentValue)));
            }

            return string.Join(", ", fieldStrings.ToArray());
        }

        internal static List<IEntityField2> GetFieldsFromNames(IEntity2 entity, IEnumerable<string> names)
        {
            List<IEntityField2> fields = new List<IEntityField2>();

            foreach (string fieldName in names)
            {
                IEntityField2 field = entity.Fields[fieldName];

                if (field != null && fields.IndexOf(field) == -1)
                {
                    fields.Add(field);
                }
            }

            return fields;
        }

        internal static List<IEntityField2> GetDirtyFields(IEntity2 entity)
        {
            List<IEntityField2> fields = new List<IEntityField2>();

            foreach (IEntityField2 field in entity.Fields)
            {
                if (field.IsChanged) fields.Add(field);
            }

            return fields;
        }

        internal static string GetShortEntityName(IEntity2 entity)
        {
            Type entityType = entity.GetType();

            const string entitySuffix = "Entity";

            if (entityType.Name.EndsWith(entitySuffix))
            {
                return entityType.Name.Substring(0, entityType.Name.Length - entitySuffix.Length);
            }
            else
            {
                return string.Format("\"{0}\"", entityType.Name);
            }
        }

        internal static void PrintCurrentEntity(StringBuilder builder, int indent, IEntity2 current, 
            ReferenceLabeller labeller, string[] fieldsToDisplay, params string[] notes)
        {
            // Build a list of primary key fields:
            List<IEntityField2> fields = new List<IEntityField2>();

            fields.AddRange(current.PrimaryKeyFields);
            fields.AddRange(GetFieldsFromNames(current, fieldsToDisplay));

            List<string> allNotes = new List<string>();

            allNotes.Add(labeller.Label(current));
            if (current.IsDirty) allNotes.Add("Dirty");
            allNotes.AddRange(notes);

            // Print the details:
            string leftPart = string.Format("{0} {1} ({2})", 
                "".PadLeft(indent * 4),
                GetShortEntityName(current), 
                GetFieldValuesDebugString(fields, false));

            string line = string.Format("{0} [{1}]",
                leftPart.PadRight(40),
                string.Join("; ", allNotes.ToArray()));

            builder.AppendLine(line);
        }
    }
}
