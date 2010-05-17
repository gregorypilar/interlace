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

#endregion

namespace DatabaseCop
{
    [Serializable]
    public class Database
    {
        string _name;
        Dictionary<ObjectName, Table> _tables;
        Dictionary<string, Table> _tablesByShortName;

        protected Database(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public static Database FromConnection(SqlConnection connection)
        {
            Database database = null;

            Dictionary<ObjectName, Table> tables = new Dictionary<ObjectName, Table>();

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = 
                    "SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE FROM INFORMATION_SCHEMA.TABLES";

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Create the database upon reading the first row so that the database name
                        // can be set using the canonical capitalisation.
                        if (database == null) database = new Database(reader.GetString(0));

                        Table table = new Table(database, new ObjectName(reader.GetString(1), reader.GetString(2)));
                        tables[table.Name] = table;
                    }
                }
            }

            foreach (Table table in tables.Values)
            {
                table.LoadColumnsFromDatabase(connection);
            }

            database._tables = tables;
            database._tablesByShortName = new Dictionary<string, Table>();

            foreach (Table table in tables.Values)
            {
                database._tablesByShortName[table.ParsedName.Value] = table;
            }

            Constraint.PopulateTableConstraintsFromConnection(connection, database);

            return database;
        }

        public void Visit(DatabaseVisitor visitor)
        {
            visitor.BeginDatabaseVisit(this);

            foreach (Table table in _tables.Values)
            {
                table.Visit(visitor);
            }

            visitor.EndDatabaseVisit(this);
        }

        public IDictionary<ObjectName, Table> TablesByName
        {
            get { return _tables; }
        }

        public IDictionary<string, Table> TablesByShortName
        {
            get { return _tablesByShortName; }
        }

        public IEnumerable<Table> Tables
        {
            get 
            { 
                foreach (KeyValuePair<ObjectName, Table> pair in _tables)
                {
                    yield return pair.Value;
                }
            }
        }
    }
}
