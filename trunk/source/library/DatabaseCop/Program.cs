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
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Interlace.Collections;
using Interlace.DatabaseManagement;

#endregion

namespace DatabaseCop
{
    class Program
    {
        static Regex _connectionStringExpression = new Regex("(([^:@]+):([^:@]+)@)?([^:@]+):([^:@]+)");

        static Regex _fields = new Regex("([^.(]|\\w+\\()(\\w+)\\.Fields\\(\\\"([a-zA-Z0-9_]+)\\\"\\)\\.Value");

        static void Main(string[] args)
        {
            DatabaseConnectionString connectionString = new DatabaseConnectionString();
            connectionString.ServerName = ".\\SQLEXPRESS";
            connectionString.DatabaseName = "Avance";
            connectionString.UseIntegratedAuthentication = false;
            connectionString.Username = "tmsplus";
            connectionString.Password = "cool";

            Database database;

            using (SqlConnection connection = new SqlConnection(connectionString.ToString()))
            {
                connection.Open();

                database = Database.FromConnection(connection);
            }

            Plan plan = new Plan(database);

            plan.AddPrecondition("InvoiceLineSource", "CourierJobId");
            plan.AddPrecondition("JobDriverEffort", "JobId");
            plan.AddPrecondition("Transfer", "JobNoteId");
            plan.AddPrecondition("Transfer", "WaypointId");

            Dependencies.CreateDependenciesGraph(database);
        }

        static void CodePatchMain(string[] args)
        {
            DatabaseConnectionString connectionString = new DatabaseConnectionString();
            connectionString.ServerName = "SERVER14";
            connectionString.DatabaseName = "RMTSv2";
            connectionString.UseIntegratedAuthentication = false;
            connectionString.Username = "tmsplus";
            connectionString.Password = "cool";

            Database database;

            using (SqlConnection connection = new SqlConnection(connectionString.ToString()))
            {
                connection.Open();

                database = Database.FromConnection(connection);
            }

            Dictionary<string, string> fieldMappings = GetColumnTypeMappings(database);

            Dictionary<string, string> functionMap = new Dictionary<string,string>();
            functionMap["char"] = "FieldCasts.FromChar";
            functionMap["nvarchar"] = "FieldCasts.FromNVarChar";
            functionMap["nchar"] = "FieldCasts.FromNChar";
            functionMap["float"] = "FieldCasts.FromFloat";
            functionMap["real"] = "FieldCasts.FromReal";
            functionMap["datetime"] = "FieldCasts.FromDateTime";
            functionMap["bit"] = "FieldCasts.FromBit";
            functionMap["varchar"] = "FieldCasts.FromVarChar";
            functionMap["text"] = "FieldCasts.FromText";
            functionMap["ntext"] = "FieldCasts.FromNText";
            functionMap["numeric"] = "FieldCasts.FromNumeric";
            functionMap["int"] = "FieldCasts.FromInt";
            functionMap["image"] = "FieldCasts.FromImage";
            functionMap["varbinary"] = "FieldCasts.FromVarBinary";
            functionMap["smallint"] = "FieldCasts.FromSmallInt";
            functionMap["tinyint"] = "FieldCasts.FromTinyInt";

            DirectoryInfo info = new DirectoryInfo(@"C:\Versioning\Haulage");

            FileInfo[] files = info.GetFiles("*.vb", SearchOption.AllDirectories);

            Set<string> unknownFields = new Set<string>();

            foreach (FileInfo file in files)
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {
                    string entireFile = reader.ReadToEnd();

                    _fields.Replace(entireFile, (MatchEvaluator)delegate(Match match)
                    {
                        Console.WriteLine(match.Value);

                        string characterBeforeIdentifier = match.Groups[1].Value;
                        string recordSetIdentifier = match.Groups[2].Value;
                        string fieldName = match.Groups[3].Value;

                        string converterName = "FieldCasts.Identity";

                        if (fieldMappings.ContainsKey(fieldName.ToLower()))
                        {
                            string fieldType = fieldMappings[fieldName.ToLower()];

                            converterName = functionMap[fieldType];
                        }
                        else
                        {
                            unknownFields.UnionUpdate(fieldName.ToLower());
                        }

                        string replacementText = string.Format("{0}{1}({2}.Fields(\"{3}\").Value)",
                            characterBeforeIdentifier, converterName, recordSetIdentifier, fieldName);

                        Console.WriteLine("    " + replacementText);

                        return replacementText;
                    });
                }
            }
        }

        static Dictionary<string, string> GetColumnTypeMappings(Database database)
        {
            Dictionary<string, string> fieldMappings = new Dictionary<string, string>();

            Set<string> ambiguousMappings = new Set<string>();

            foreach (Table table in database.Tables)
            {
                foreach (KeyValuePair<string, Column> pair in table.Columns)
                {
                    string fieldNameLower = pair.Key.ToLower();

                    if (fieldMappings.ContainsKey(fieldNameLower))
                    {
                        if (fieldMappings[fieldNameLower] != pair.Value.DataType)
                        {
                            ambiguousMappings.UnionUpdate(fieldNameLower);
                        }
                    }
                    else
                    {
                        fieldMappings[fieldNameLower] = pair.Value.DataType;
                    }
                }
            }

            foreach (string ambiguousMapping in ambiguousMappings)
            {
                fieldMappings.Remove(ambiguousMapping);
            }

            return fieldMappings;
        }

        static void OldMain(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(":(");

                return;
            }

            if (args[0] == "/dump")
            {
                if (args.Length < 2)
                {
                    Console.WriteLine(":( :(");

                    return;
                }

                DatabaseConnectionString connectionString = ParseConnectionString(args[1]);

                Database database;

                using (SqlConnection connection = new SqlConnection(connectionString.ToString()))
                {
                    connection.Open();

                    database = Database.FromConnection(connection);

                    Dependencies.DumpSerialized(database, connection);
                }
            }

            if (args[0] == "/compare")
            {
                Dependencies.CompareInCurrentDirectory();
            }
        }

        private static DatabaseConnectionString ParseConnectionString(string argument)
        {
            Match match = _connectionStringExpression.Match(argument);

            DatabaseConnectionString connectionString = new DatabaseConnectionString();

            if (match.Groups[1].Success)
            {
                connectionString.Username = match.Groups[2].Value;
                connectionString.Password = match.Groups[3].Value;
                connectionString.UseIntegratedAuthentication = false;
            }
            else
            {
                connectionString.UseIntegratedAuthentication = true;
            }

            connectionString.ServerName = match.Groups[4].Value;
            connectionString.DatabaseName = match.Groups[5].Value;
            connectionString.ConnectionTimeout = 10;
            return connectionString;
        }

        static void CheckRules(Database database)
        {
            RuleVisitor visitor = new RuleVisitor();

            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            PopulateRuleVisitorFromAssembly(visitor, thisAssembly);

            database.Visit(visitor);
        }

        private static void PopulateRuleVisitorFromAssembly(RuleVisitor visitor, Assembly assembly)
        {
            Type[] types = assembly.GetExportedTypes();

            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(Rule)) && type != typeof(Rule))
                {
                    ConstructorInfo constructor = type.GetConstructor(new Type[0]);

                    Rule rule = constructor.Invoke(new object[0]) as Rule;

                    visitor.AddRule(rule);
                }
            }
        }
    }
}
