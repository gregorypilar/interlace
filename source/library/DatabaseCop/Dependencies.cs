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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

using Interlace.Collections;
using Interlace.Utilities;

using DatabaseCop.RuleHelpers;

#endregion

namespace DatabaseCop
{
    public static class Dependencies
    {
        static IEnumerable<Table> EdgeGetter(Table table)
        {
            foreach (ForeignKeyConstraint constraint in table.ForeignKeyConstraints)
            {
                if (constraint.IsDisabled ?? false) continue;

                yield return constraint.UniqueConstraint.Table;
            }
        }

        public static void CheckDependencies(Database database)
        {
            ICollection<Table> ordered = TopologicalSort.Sort<Table>(database.TablesByName.Values, EdgeGetter);
        }

        public static void CreateFilteredDependenciesGraph(Database database, Table startTable, Plan plan)
        {
            Set<Table> visited = new Set<Table>();
            Stack<Table> seen = new Stack<Table>();

            seen.Push(startTable);

            // The relations that, as a precondition of the delete, must not exist:
            while (seen.Count > 0)
            {
                Table table = seen.Pop();

                visited.UnionUpdate(table);

                foreach (ForeignKeyConstraint constraint in table.ForeignKeyDependencies)
                {
                    if (plan.IsConstraintExcludedFromDependencyDiagram(constraint)) continue;

                    if (!visited.Contains(constraint.Table))
                    {
                        seen.Push(constraint.Table);
                    }
                }
            }

            CreateDependenciesGraph(database, visited);
        }

        public static void CreateDependenciesGraph(Database database)
        {
            CreateDependenciesGraph(database, null);
        }

        public static void CreateDependenciesGraph(Database database, Set<Table> tableSubset)
        {
            using (StreamWriter writer = new StreamWriter("C:\\test.viz"))
            {
                writer.WriteLine("digraph database {");
                writer.WriteLine("  node [fontname=Helvetica];");
                writer.WriteLine("  node [shape=none, fontcolor=white, style=filled];");

                foreach (Table table in database.TablesByName.Values)
                {
                    if (tableSubset != null && !tableSubset.Contains(table)) continue;

                    ParsedTableIdentifier tableIdentifier = new ParsedTableIdentifier(table.Name.Name);

                    string nodeStyle = "black";

                    if (tableIdentifier.Prefix == "common") nodeStyle = "fillcolor=yellow4";
                    if (tableIdentifier.Prefix == "courier") nodeStyle = "fillcolor=darkolivegreen";
                    if (tableIdentifier.Prefix == "haulage") nodeStyle = "fillcolor=darkgoldenrod4";

                    string nodeSizing = "";

                    int connectionCount = table.ForeignKeyConstraintsByName.Count;

                    foreach (TableConstraint constraint in table.TableConstraints)
                    {
                        connectionCount += constraint.RelatedForeignKeysByName.Count;
                    }

                    double width = 1.5 + 1.5 * connectionCount / 3.0;
                    double height = 0.5 + 0.5 * connectionCount / 3.0;
                    if (connectionCount > 3) nodeSizing = string.Format(", width={0:0.0}, height={1:0.0}", width, height);

                    writer.WriteLine("  \"{0}\" [{1}{2}]", tableIdentifier.Value, nodeStyle, nodeSizing);

                    foreach (ForeignKeyConstraint foreignKey in table.ForeignKeyConstraints)
                    {
                        if (tableSubset != null && !tableSubset.Contains(foreignKey.Table)) continue;

                        ParsedTableIdentifier foreignKeyTableName = new ParsedTableIdentifier(foreignKey.Table.Name.Name);
                        ParsedTableIdentifier primaryKeyTableName = new ParsedTableIdentifier(foreignKey.UniqueConstraint.Table.Name.Name);

                        string edgeStyle = "color=gray15";

                        string arrowStyle = "diamondnormal";

                        if (foreignKey.IsOneToOne) arrowStyle = "normal";

                        if (foreignKey.IsDisabled ?? false) edgeStyle = "style=dotted, color=gray15";
                        if (foreignKey.UpdateRule == ForeignKeyRule.Cascade) edgeStyle = "color=red4";
                        
                        writer.WriteLine("  \"{0}\" -> \"{1}\" [{2}, arrowhead={3}]", 
                            foreignKeyTableName.Value, primaryKeyTableName.Value, edgeStyle, arrowStyle);
                    }
                }

                writer.WriteLine("}");

                writer.Close();
            }
        }

        public static string IdentifyCompany(Database database, SqlConnection connection)
        {
            if (database.TablesByName.ContainsKey(new ObjectName("dbo", "Company")))
            {
                Table companyTable = database.TablesByName[new ObjectName("dbo", "Company")];

                if (companyTable.Columns.ContainsKey("Comp_ID") && companyTable.Columns.ContainsKey("Comp_Name"))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText =
                            "SELECT TOP 1 Comp_Name FROM dbo.Company ORDER BY Comp_ID ASC";

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                return Capitalisation.ToNameCase(reader.GetString(0)).Trim();
                            }
                        }
                    }
                }
            }

            return "Unknown Company";
        }

        public static string IdentifyLastUseDate(Database database, SqlConnection connection)
        {
            if (database.TablesByName.ContainsKey(new ObjectName("dbo", "Booking_1")))
            {
                Table bookingTable = database.TablesByName[new ObjectName("dbo", "Booking_1")];

                if (bookingTable.Columns.ContainsKey("Job_BookingTime"))
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText =
                            "SELECT MAX(Job_BookingTime) FROM dbo.Booking_1";

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(0))
                            {
                                return reader.GetDateTime(0).ToString("d MMMM yyyy");
                            }
                        }
                    }
                }
            }

            return "Unknown Date";
        }

        static Regex _invalidFileNameComponent = new Regex("[^-_(), A-Za-z0-9]+");

        static string GetFilename(Database database, SqlConnection connection, string extension)
        {
            string identity = IdentifyCompany(database, connection);
            string date = IdentifyLastUseDate(database, connection);

            string fileNameBase = string.Format("Database Dump, {0} ({1}, {2})", 
                database.Name, identity, date);

            string filteredBase = _invalidFileNameComponent.Replace(fileNameBase, "");

            return string.Format("{0}.{1}", filteredBase, extension.TrimStart('.'));
        }

        public static void DumpSerialized(Database database, SqlConnection connection)
        {
            string fileName = GetFilename(database, connection, "databasecop");

            using (Stream stream = new FileStream(fileName, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, database);
            }
        }

        public static void Dump(Database database, SqlConnection connection)
        {
            string fileName = GetFilename(database, connection, "txt");

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                List<ObjectName> tableNames = new List<ObjectName>();
                tableNames.AddRange(database.TablesByName.Keys);

                tableNames.Sort();

                foreach (ObjectName tableName in tableNames)
                {
                    Table table = database.TablesByName[tableName];

                    writer.WriteLine("table \"{0}\"", table.Name);
                    writer.WriteLine("{");

                    foreach (Column column in table.Columns.Values)
                    {
                        writer.Write("    column \"{0}\" ", column.Name);
                        writer.Write("{ ");
                        writer.Write("type \"{0}\";", column.DataType);

                        if (column.IsNullable) writer.Write(" nullable;");

                        if (column.ColumnDefault != null)
                        {
                            writer.Write(" default \"{0}\";", column.ColumnDefault);
                        }

                        if (column.CharacterMaximumLength.HasValue)
                        {
                            writer.Write(" length {0};", column.CharacterMaximumLength.Value);
                        }

                        writer.WriteLine(" }");
                    }

                    writer.WriteLine("}");
                    writer.WriteLine();
                }
            }
        }

        static List<Database> LoadDatabasesInCurrentDirectory()
        {
            List<Database> databases = new List<Database>();

            DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory);

            foreach (FileInfo file in directory.GetFiles("*.databasecop"))
            {
                using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    object databaseObject = formatter.Deserialize(stream);
                    Database database = databaseObject as Database;

                    if (database != null) databases.Add(database);
                }
            }

            return databases;
        }

        static string NormaliseColumnName(string columnName)
        {
            return columnName.ToLower();
        }

        internal static void CompareInCurrentDirectory()
        {
            List<Database> databases = LoadDatabasesInCurrentDirectory();

            // Find which tables are common to all databases:
            Set<ObjectName> allTables = new Set<ObjectName>();
            Set<ObjectName> commonTables = new Set<ObjectName>();

            if (databases.Count > 0) commonTables.UnionUpdate(databases[0].TablesByName.Keys);

            foreach (Database database in databases)
            {
                Set<ObjectName> tablesInThisDatabase = new Set<ObjectName>(database.TablesByName.Keys);

                allTables.UnionUpdate(tablesInThisDatabase);
                commonTables.IntersectionUpdate(tablesInThisDatabase);
            }

            Set<ObjectName> nonCommonTables = Set<ObjectName>.Difference(allTables, commonTables);

            Console.WriteLine(nonCommonTables);

            // Find which columns are common to all databases:
            foreach (ObjectName commonTableName in commonTables)
            {
                Set<string> allColumns = new Set<string>();
                Set<string> commonColumns = new Set<string>();

                if (databases.Count > 0)
                {
                    Set<string> columns = new Set<string>(databases[0].TablesByName[commonTableName].Columns.Keys);
                    commonColumns.UnionUpdate(columns.Map<string>(NormaliseColumnName));
                }

                foreach (Database database in databases)
                {
                    Set<string> columnsInThisTable = new Set<string>(database.TablesByName[commonTableName].Columns.Keys).Map<string>(NormaliseColumnName);

                    commonColumns.IntersectionUpdate(columnsInThisTable);
                    allColumns.UnionUpdate(columnsInThisTable);
                }

                Set<string> nonCommonColumns = Set<string>.Difference(allColumns, commonColumns);

                if (nonCommonColumns.Count > 0)
                {
                    Console.WriteLine(commonTableName.Name);
                    Console.WriteLine(nonCommonColumns);
                }
            }
        }
    }
}
