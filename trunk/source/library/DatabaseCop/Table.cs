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
using System.Data;
using System.Data.SqlClient;
using System.Text;

using Interlace.DatabaseManagement;

using DatabaseCop.RuleHelpers;

#endregion

namespace DatabaseCop
{
    [Serializable]
    public class Table
    {
        Database _database;

        ObjectName _name;
        ParsedTableIdentifier _parsedName;

        Dictionary<string, Column> _columns;
        List<Column> _orderedColumns;

        Dictionary<ObjectName, TableConstraint> _tableConstraints;
        Dictionary<ObjectName, ForeignKeyConstraint> _foreignKeyConstraints;
        List<ForeignKeyConstraint> _foreignKeyDependencies;

        internal Table(Database database, ObjectName name)
        {
            _database = database;

            _name = name;
            _parsedName = new ParsedTableIdentifier(name.Name);

             _tableConstraints = new Dictionary<ObjectName, TableConstraint>();
             _foreignKeyConstraints = new Dictionary<ObjectName, ForeignKeyConstraint>();
             _foreignKeyDependencies = new List<ForeignKeyConstraint>();
        }

        internal void AddTableConstraint(TableConstraint constraint)
        {
            _tableConstraints[constraint.Name] = constraint;
        }

        internal void AddForeignKeyConstraint(ForeignKeyConstraint constraint)
        {
            _foreignKeyConstraints[constraint.Name] = constraint;
            constraint.UniqueConstraint.Table._foreignKeyDependencies.Add(constraint);
        }

        public Database Database
        {
            get { return _database; }
        }

        public ObjectName Name
        {
            get { return _name; }
        }

        public IDictionary<string, Column> Columns
        {
            get { return _columns; }
        }

        public ParsedTableIdentifier ParsedName
        {
            get { return _parsedName; }
        }

        public IDictionary<ObjectName, TableConstraint> TableConstraintsByName
        { 	 
            get { return _tableConstraints; }
        }

        public IEnumerable<TableConstraint> TableConstraints
        {
            get { return _tableConstraints.Values; }
        }

        public IDictionary<ObjectName, ForeignKeyConstraint> ForeignKeyConstraintsByName
        { 	 
            get { return _foreignKeyConstraints; }
        }

        public IEnumerable<ForeignKeyConstraint> ForeignKeyDependencies
        { 	 
            get { return _foreignKeyDependencies; }
        }

        public IEnumerable<ForeignKeyConstraint> ForeignKeyConstraints
        {
            get { return _foreignKeyConstraints.Values; }
        }

        static string GetNullableStringFromReader(IDataReader reader, int index)
        {
            if (reader.IsDBNull(index)) return null;

            return reader.GetString(index);
        }

        static int GetIntFromReader(IDataReader reader, int index)
        {
            return Convert.ToInt32(reader.GetValue(index));
        }

        static int? GetNullableIntFromReader(IDataReader reader, int index)
        {
            if (reader.IsDBNull(index)) return null;

            return reader.GetInt32(index);
        }

        static int? GetNullableByteFromReader(IDataReader reader, int index)
        {
            if (reader.IsDBNull(index)) return null;

            return reader.GetByte(index);
        }

        static int? GetNullableShortFromReader(IDataReader reader, int index)
        {
            if (reader.IsDBNull(index)) return null;

            return reader.GetInt16(index);
        }

        internal void LoadColumnsFromDatabase(SqlConnection connection)
        {
            _columns = new Dictionary<string, Column>();
            _orderedColumns = new List<Column>();

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = 
                    "SELECT COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE, " +
                    "CHARACTER_MAXIMUM_LENGTH, CHARACTER_OCTET_LENGTH, NUMERIC_PRECISION, " +
                    "NUMERIC_PRECISION_RADIX, NUMERIC_SCALE, DATETIME_PRECISION, COLLATION_NAME " +
                    "FROM INFORMATION_SCHEMA.COLUMNS " +
                    "WHERE TABLE_CATALOG = @Catalog AND TABLE_SCHEMA = @Schema AND TABLE_NAME = @Name ORDER BY ORDINAL_POSITION";

                command.Parameters.Add("@Catalog", SqlDbType.NVarChar, 128).Value = _database.Name;
                command.Parameters.Add("@Schema", SqlDbType.NVarChar, 128).Value = _name.Schema;
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 128).Value = _name.Name;

                Column previousColumn = null;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader.GetString(0);

                        Column column = new Column(this,
                            reader.GetString(0), GetIntFromReader(reader, 1), GetNullableStringFromReader(reader, 2), 
                            reader.GetString(3) == "YES", reader.GetString(4),
                            GetNullableIntFromReader(reader, 5), GetNullableIntFromReader(reader, 6), 
                            GetNullableByteFromReader(reader, 7), GetNullableShortFromReader(reader, 8), 
                            GetNullableIntFromReader(reader, 9),
                            GetNullableShortFromReader(reader, 10), GetNullableStringFromReader(reader, 11));

                        _columns[columnName] = column;
                        _orderedColumns.Add(column);

                        if (previousColumn != null) column.SetPreviousColumn(previousColumn);

                        previousColumn = column;
                    }
                }
            }
        }

        internal void Visit(DatabaseVisitor visitor)
        {
            visitor.BeginTableVisit(this);

            foreach (Column column in _columns.Values)
            {
                visitor.VisitColumn(column);
            }

            visitor.EndTableVisit(this);
        }

        public override string ToString()
        {
            return _name.ToString();
        }

        public Column FirstColumn
        {
            get { return _orderedColumns[0]; }
        }

        public Column LastColumn
        {
            get { return _orderedColumns[_columns.Count - 1]; }
        }
    }
}
