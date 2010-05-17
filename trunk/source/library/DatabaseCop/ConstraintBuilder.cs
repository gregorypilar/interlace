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

namespace DatabaseCop
{
    public class ConstraintBuilder
    {
        readonly ObjectName _constraintName;

        ObjectName _tableName;

        ConstraintType _constraintType;

        List<string> _constraintColumns;

        ObjectName _foreignUniqueConstraintName;
        ForeignKeyRule _foreignUpdateRule;
        ForeignKeyRule _foreignDeleteRule;

        bool? _isDisabled;

        public ConstraintBuilder(ObjectName constraintName)
        {
            _constraintName = constraintName;
            _constraintColumns = new List<string>();
        }

        public ObjectName ConstraintName
        { 	 
            get { return _constraintName; }
        }

        public ObjectName TableName
        { 	 
            get { return _tableName; }
            set { _tableName = value; }
        }

        public ConstraintType ConstraintType
        { 	 
            get { return _constraintType; }
            set { _constraintType = value; }
        }

        public List<string> ConstraintColumns
        { 	 
            get { return _constraintColumns; }
        }

        public ObjectName ForeignUniqueConstraintName
        { 	 
            get { return _foreignUniqueConstraintName; }
            set { _foreignUniqueConstraintName = value; }
        }

        public ForeignKeyRule ForeignUpdateRule
        { 	 
            get { return _foreignUpdateRule; }
            set { _foreignUpdateRule = value; }
        }

        public ForeignKeyRule ForeignDeleteRule
        { 	 
            get { return _foreignDeleteRule; }
            set { _foreignDeleteRule = value; }
        }

        public bool? IsDisabled
        { 	 
            get { return _isDisabled; }
            set { _isDisabled = value; }
        }

        internal TableConstraint BuildTableConstraint(Database database)
        {
            if (!(_constraintType == ConstraintType.PrimaryKey || 
                _constraintType == ConstraintType.Unique) ||
                _foreignUniqueConstraintName != null)
            {
                throw new InvalidOperationException();
            }

            if (!database.TablesByName.ContainsKey(_tableName))
            {
                throw new InvalidOperationException("A table referenced in a constraint was not found in the table set.");
            }

            Table table = database.TablesByName[_tableName];

            return new TableConstraint(_constraintName, table, _constraintType, ResolveColumns(_constraintColumns, table));
        }

        internal static IEnumerable<Column> ResolveColumns(IEnumerable<string> columnNames, Table table)
        {
            List<Column> columns = new List<Column>();

            foreach (string columnName in columnNames)
            {
                if (!table.Columns.ContainsKey(columnName))
                {
                    throw new InvalidOperationException("A column referenced in a constraint does not exist in the table.");
                }

                columns.Add(table.Columns[columnName]);
            }

            return columns;
        }

        internal ForeignKeyConstraint BuildForeignKeyConstraint(Database database, Dictionary<ObjectName, TableConstraint> tableConstraints)
        {
            if (_constraintType != ConstraintType.ForeignKey ||
                _foreignUniqueConstraintName == null)
            {
                throw new InvalidOperationException();
            }

            if (!database.TablesByName.ContainsKey(_tableName))
            {
                throw new InvalidOperationException("A table referenced in a constraint was not found in the table set.");
            }

            if (!tableConstraints.ContainsKey(_foreignUniqueConstraintName))
            {
                throw new InvalidOperationException("A unique constraint referenced in a foreign key was not found.");
            }

            Table table = database.TablesByName[_tableName];
            TableConstraint uniqueConstraint = tableConstraints[_foreignUniqueConstraintName];

            return new ForeignKeyConstraint(_constraintName, table, _constraintType, ResolveColumns(_constraintColumns, table),
                uniqueConstraint, _foreignUpdateRule, _foreignDeleteRule, _isDisabled);
        }
    }
}
