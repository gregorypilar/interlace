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
using System.Data.SqlClient;
using System.Text;

#endregion

namespace DatabaseCop
{
    [Serializable]
    public enum ConstraintType
    {
        PrimaryKey,
        ForeignKey,
        Unique
    }

    [Serializable]
    public enum ForeignKeyRule
    {
        NoAction,
        Cascade
    }

    [Serializable]
    public class Constraint
    {
        readonly ObjectName _name;
        readonly Table _table;
        readonly ConstraintType _constraintType;
        readonly List<Column> _keyColumns;

        protected Constraint(ObjectName name, Table table, ConstraintType constraintType, IEnumerable<Column> keyColumns)
        {
            _name = name;
            _table = table;
            _constraintType = constraintType;
            _keyColumns = new List<Column>(keyColumns);
        }

        public ObjectName Name
        { 	 
            get { return _name; }
        }

        public Table Table
        { 	 
            get { return _table; }
        }

        public ConstraintType ConstraintType
        { 	 
            get { return _constraintType; }
        }

        public IEnumerable<Column> KeyColumns
        { 	 
            get { return _keyColumns; }
        }

        static ForeignKeyRule StringToForeignKeyRule(string foreignKeyRuleString)
        {
            switch (foreignKeyRuleString.ToUpper())
            {
                case "NO ACTION":
                    return ForeignKeyRule.NoAction;

                case "CASCADE":
                    return ForeignKeyRule.Cascade;

                default:
                    throw new NotImplementedException(string.Format(
                        "The foreign key rule \"{0}\" is not recognised or supported.", foreignKeyRuleString));
            }
        }

        static ConstraintType StringToConstraintType(string constraintTypeString)
        {
            switch (constraintTypeString.ToUpper())
            {
                case "PRIMARY KEY":
                    return ConstraintType.PrimaryKey;

                case "FOREIGN KEY":
                    return ConstraintType.ForeignKey;

                case "UNIQUE":
                    return ConstraintType.Unique;

                default:
                    throw new NotImplementedException(string.Format(
                        "The constraint type \"{0}\" is not recognised or supported.", constraintTypeString));
            }
        }

        public static void PopulateTableConstraintsFromConnection(SqlConnection connection, Database database)
        {
            Dictionary<ObjectName, ConstraintBuilder> builders = FetchConstraints(connection);
            FetchConstraintColumns(connection, builders);
            FetchReferentialConstraints(connection, builders);
            FetchDisabledForeignKeyConstraints(connection, builders);

            // Build the table constraints first; they will be required for building referential constraints:
            Dictionary<ObjectName, TableConstraint> tableConstraints = new Dictionary<ObjectName, TableConstraint>();

            foreach (ConstraintBuilder builder in builders.Values)
            {
                if (builder.ConstraintType != ConstraintType.ForeignKey)
                {
                    tableConstraints[builder.ConstraintName] = builder.BuildTableConstraint(database);
                }
            }

            // Build the referential constraints:
            Dictionary<ObjectName, ForeignKeyConstraint> foreignKeyConstraints = new Dictionary<ObjectName, ForeignKeyConstraint>();

            foreach (ConstraintBuilder builder in builders.Values)
            {
                if (builder.ConstraintType == ConstraintType.ForeignKey)
                {
                    foreignKeyConstraints[builder.ConstraintName] = 
                        builder.BuildForeignKeyConstraint(database, tableConstraints);
                }
            }

            // Attach and link them:
            foreach (TableConstraint constraint in tableConstraints.Values)
            {
                constraint.Table.AddTableConstraint(constraint);
            }

            foreach (ForeignKeyConstraint constraint in foreignKeyConstraints.Values)
            {
                constraint.Table.AddForeignKeyConstraint(constraint);
                constraint.UniqueConstraint.AddRelatedForeignKeyConstraint(constraint);
            }
        }

        static Dictionary<ObjectName, ConstraintBuilder> FetchConstraints(SqlConnection connection)
        {
            Dictionary<ObjectName, ConstraintBuilder> builders = new Dictionary<ObjectName, ConstraintBuilder>();

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = 
                    "SELECT CONSTRAINT_SCHEMA, CONSTRAINT_NAME, TABLE_SCHEMA, TABLE_NAME, CONSTRAINT_TYPE " +
                    "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS";

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ConstraintBuilder builder = new ConstraintBuilder(new ObjectName(reader.GetString(0), reader.GetString(1)));
                        builder.TableName = new ObjectName(reader.GetString(2), reader.GetString(3));
                        builder.ConstraintType = StringToConstraintType(reader.GetString(4));

                        if (builders.ContainsKey(builder.ConstraintName))
                        {
                            throw new InvalidOperationException("Duplicate constraint names were found in the database.");
                        }

                        builders[builder.ConstraintName] = builder;
                    }
                }
            }

            return builders;
        }
        
        static void FetchConstraintColumns(SqlConnection connection, Dictionary<ObjectName, ConstraintBuilder> builders)
        {
            Dictionary<ObjectName, List<string>> constraintColumns = new Dictionary<ObjectName, List<string>>();

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText =
                    "SELECT CONSTRAINT_SCHEMA, CONSTRAINT_NAME, COLUMN_NAME, ORDINAL_POSITION " +
                    "FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE ORDER BY ORDINAL_POSITION";

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ObjectName constraintName = new ObjectName(reader.GetString(0), reader.GetString(1));

                        if (!builders.ContainsKey(constraintName))
                        {
                            throw new InvalidOperationException(
                                "A constraint mentioned by a constraint column was not listed in the constraint table.");
                        }

                        ConstraintBuilder builder = builders[constraintName];

                        int ordinalPosition = reader.GetInt32(3);

                        if (builder.ConstraintColumns.Count + 1 != ordinalPosition)
                        {
                            throw new InvalidOperationException(
                                "A constraint column was found that was not in the correct ordinal position.");
                        }

                        builder.ConstraintColumns.Add(reader.GetString(2));
                    }
                }
            }
        }

        static void FetchReferentialConstraints(SqlConnection connection, Dictionary<ObjectName, ConstraintBuilder> builders)
        {
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText =
                    "SELECT CONSTRAINT_SCHEMA, CONSTRAINT_NAME, UNIQUE_CONSTRAINT_SCHEMA, UNIQUE_CONSTRAINT_NAME, " +
                    "UPDATE_RULE, DELETE_RULE " +
                    "FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS";

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ObjectName constraintName = new ObjectName(reader.GetString(0), reader.GetString(1));
                        
                        if (!builders.ContainsKey(constraintName))
                        {
                            throw new InvalidOperationException("A referential constraint was not listed in the constraint table.");
                        }

                        ConstraintBuilder builder = builders[constraintName];

                        builder.ForeignUniqueConstraintName = new ObjectName(reader.GetString(2), reader.GetString(3));
                        builder.ForeignUpdateRule = StringToForeignKeyRule(reader.GetString(4));
                        builder.ForeignDeleteRule = StringToForeignKeyRule(reader.GetString(5));
                    }
                }
            }
        }

        static void FetchDisabledForeignKeyConstraints(SqlConnection connection, Dictionary<ObjectName, ConstraintBuilder> builders)
        {
            // Disabled foreign keys are specific to Microsoft SQL Server; the SQL-92 standard
            // instead uses deferred constraints which are present in the INFORMATION_SCHEMA schema.

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText =
                    "SELECT " +
                    "   master.dbo.sysusers.name, " +
                    "   dbo.sysobjects.name, " +
                    "   ObjectProperty(dbo.sysobjects.id, 'CnstIsDisabled') " +
                    "FROM sysobjects INNER JOIN master.dbo.sysusers ON " +
                    "ObjectProperty(sysobjects.id, 'OwnerId') = master.dbo.sysusers.uid " +
                    "WHERE sysobjects.xtype = 'F'";

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ObjectName constraintName = new ObjectName(reader.GetString(0), reader.GetString(1));
                        
                        if (!builders.ContainsKey(constraintName))
                        {
                            throw new InvalidOperationException("A referential constraint was not listed in the constraint table.");
                        }

                        ConstraintBuilder builder = builders[constraintName];

                        builder.IsDisabled = reader.GetInt32(2) != 0;
                    }
                }
            }
        }
    }
}
