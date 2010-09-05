#region Using Directives and Copyright Notice

// Copyright (c) 2006-2010, Bit Plantation (ABN 80 332 904 638)
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
using System.Data;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Collections;
using System.Data.Common;

#endregion

namespace Interlace.DatabaseManagement
{
	public class DatabaseManager
	{
		IDbConnection _connection;
        IDatabaseImplementation _implementation;
        string _product;
        IDatabaseSchemaFactory _databaseSchemaFactory;

		DatabaseSchema _schema;

		public const string UninitialisedVersion = "uninitialised";

		public DatabaseManager(IDbConnection connection, IDatabaseImplementation implementation, IDatabaseSchemaFactory databaseSchemaFactory, string product)
		{
			_connection = connection;
            _implementation = implementation;
            _product = product;
            _databaseSchemaFactory = databaseSchemaFactory;

            _schema = null;
		}

        protected void EnsureSchemaLoaded()
        {
            if (_schema == null)
            {
                _schema = _databaseSchemaFactory.LoadDatabaseSchema();

                if (_schema.product != _product)
                {
                    throw new InvalidOperationException(string.Format(
                        "The loaded schema is for the product \"{0}\" where as this database manager instance was " +
                        "passed the product name \"{1}\".", _schema.product, _product));
                }
            }
        }

		public void EnsureDatabaseExists(string databaseName)
		{
            string oldDatabase = _connection.Database;

            _connection.ChangeDatabase(databaseName);

            bool databaseExists;

			// See if the database exists:
			using (IDbCommand command = _connection.CreateCommand())
			{
                _implementation.PrepareDoesDatabaseExistCommand(command, databaseName);

				databaseExists = Convert.ToInt32(command.ExecuteScalar()) > 0;
			}

			// Create it if it doesn't:
			if (!databaseExists)
			{
				using (IDbCommand command = _connection.CreateCommand())
				{
                    _implementation.PrepareCreateDatabaseCommand(command, databaseName);

					command.ExecuteScalar();
				}
			}
		}

		public bool CanUpgrade(string fromVersion, string toVersion)
		{
            EnsureSchemaLoaded();

			try
			{
				GetUpgradePath(_schema, fromVersion, toVersion);

				return true;
			}
			catch (DatabaseUpgradeUnsupportedException)
			{
				return false;
			}
		}

		public delegate void UpgradeDelegate(string fromVersion, string toVersion, UpgradeProgressDelegate progressMonitor);
		public delegate void UpgradeProgressDelegate(int commandsRun, int totalCommands);

		public void Upgrade(string fromVersion, string toVersion, UpgradeProgressDelegate progressMonitor)
		{
            EnsureSchemaLoaded();

			if (fromVersion == UninitialisedVersion)
			{
				if (!VersionTableExists())
				{
					CreateVersionTable();

					UpdateVersionTable(new DatabaseVersion(_schema.product, 
						UninitialisedVersion));
				}
			}

			DatabaseVersionEdge[] edges = GetUpgradePath(_schema, fromVersion, toVersion);

			// Find the total number of commands:
			int commandsRun = 0;
			int totalCommands = 0;

			foreach (DatabaseVersionEdge edge in edges)
			{
				totalCommands += edge.Commands.Length;
			}

			if (progressMonitor != null) progressMonitor(0, totalCommands);

			foreach (DatabaseVersionEdge edge in edges)
			{
				try
				{
					foreach (string commandText in edge.Commands)
					{
						ExecuteCommand(commandText);
						commandsRun += 1;

						if (progressMonitor != null) progressMonitor(commandsRun, totalCommands);
					}
				}
				catch (Exception ex)
				{
					UpdateVersionTable(new DatabaseVersion(_schema.product, 
						edge.FromVersion), edge.FromVersion, edge.ToVersion,
						String.Format("{0}\n\n{1}", ex.Message, ex.StackTrace));

					throw;
				}

				UpdateVersionTable(new DatabaseVersion(_schema.product, 
					edge.ToVersion));
			}
		}

		protected void ExecuteCommand(string commandText)
		{
			using (IDbCommand command = _connection.CreateCommand())
			{
				command.CommandText = commandText;
				command.CommandType = CommandType.Text;

				command.ExecuteNonQuery();
			}
		}

		public bool VersionTableExists()
		{
			try
			{
				using (IDbCommand command = _connection.CreateCommand())
				{
                    _implementation.PrepareDoesVersionTableExist(command);

					return Convert.ToInt32(command.ExecuteScalar()) > 0;
				}
			}
			catch (DbException ex)
			{
				throw new DatabaseVersionTableInvalidException("The database version " +
				  "table could not be found due to an unexpected error; check the " +
				  "full error description for details on the specific error.", ex);
			}
		}

		protected void CreateVersionTable()
		{
			using (IDbCommand command = _connection.CreateCommand())
			{
                _implementation.PrepareCreateVersionTableCommand(command);

				command.ExecuteScalar();
			}
		}

		protected void UpdateVersionTable(DatabaseVersion version, 
			string failedUpgradeFromVersion, string failedUpgradeToVersion, 
			string failedUpgradeError)
		{
			using (IDbTransaction transaction = _connection.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				using (IDbCommand command = _connection.CreateCommand())
				{
					command.Transaction = transaction;
                    _implementation.PrepareDeleteVersionCommand(command);
					command.ExecuteNonQuery();
				}

				using (IDbCommand command = _connection.CreateCommand())
				{
					command.Transaction = transaction;

                    _implementation.PrepareInsertVersionCommand(command, version,
                        failedUpgradeFromVersion, failedUpgradeToVersion, failedUpgradeError);

					command.ExecuteNonQuery();
				}

				transaction.Commit();
			}
		}

		protected void UpdateVersionTable(DatabaseVersion version)
		{
			UpdateVersionTable(version, null, null, null);
		}

		public DatabaseVersion GetVersion()
		{
			if (!VersionTableExists())
			{
				throw new DatabaseVersionTableMissingException("The database does not " +
					"contain a database version table; it is probably a new database.");
			}

			try
			{
				using (IDbCommand command = _connection.CreateCommand())
				{
                    _implementation.PrepareFetchVersionRowsCommand(command);

					using (IDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult))
					{
						// Check for a missing row or invalid fields:
						if (!reader.Read())
						{
							throw new DatabaseVersionTableInvalidException(
								"The database version table does not contain any valid " +
								"version data.");
						}

						if (reader.FieldCount < 5)
						{
							throw new DatabaseVersionTableInvalidException(
							  "The database version table has too few fields to be " +
							  "valid. At least 5 fields are expected.");
						}

						if (reader.GetName(0) != "Product" ||
							reader.GetName(1) != "Version" ||
							reader.GetName(2) != "FailedUpgradeFromVersion" ||
							reader.GetName(3) != "FailedUpgradeToVersion" ||
							reader.GetName(4) != "FailedUpgradeError")
						{
							throw new DatabaseVersionTableInvalidException(
							  "One of the database version table fields is not the " +
							  "correct field name.");
						}

						// Check for an upgrade error:
						if (!reader.IsDBNull(2))
						{
							if (!reader.IsDBNull(3) && !reader.IsDBNull(4))
							{
								throw new DatabaseUpgradeFailedException( 
									reader.GetString(2), reader.GetString(3), 
									reader.GetString(4));
							}
							else
							{
								throw new DatabaseVersionTableInvalidException(
									"One of the database version upgrade failure warning " +
									"fields is not set, when one or more of the others is.");
							}
						}

						// Get the version:
						DatabaseVersion version = new DatabaseVersion(reader.GetString(0), 
							reader.GetString(1));

						// Check for multiple rows:
						if (reader.Read())
						{
							throw new DatabaseVersionTableInvalidException(
								"The database version table includes more than one version row, " +
								"and is therefore invalid.");
						}

						if (version.Product != _product)
						{
							throw new DatabaseVersionTableInvalidException(
								"The database version is not for this product.");
						}

						return version;
					}
				}
			}
			catch (DbException ex)
			{
				throw new DatabaseVersionTableInvalidException("The database version " +
					"table could not be read due to an unexpected error; check the " +
					"full error description for details on the specific error.", ex);
			}
		}

		private DatabaseVersionEdge[] GetUpgradePath(DatabaseSchema schema, string fromVersion, 
			string toVersion)
		{
			DatabaseVersionEdgeSet edges = new DatabaseVersionEdgeSet(schema.upgrade);
			Queue seen = new Queue();

			// Initialise the seen queue:
			if (edges.GetEdgesFrom(fromVersion).Count > 0)
			{
				foreach (DatabaseVersionEdge edge in edges.GetEdgesFrom(fromVersion))
				{
					edge.Unexplored = false;
					seen.Enqueue(edge);
				}
			}
			else
			{
				throw new DatabaseUpgradeUnsupportedException(fromVersion, toVersion);
			}

			// Do a breadth first search, terminating on the first sight of the toVersion:
			DatabaseVersionEdge toEdge = null;

			while (true)
			{
				if (seen.Count == 0)
				{
					throw new DatabaseUpgradeUnsupportedException(fromVersion, toVersion);
				}

				DatabaseVersionEdge edgeToExplore = seen.Dequeue() as DatabaseVersionEdge;
				
				if (edgeToExplore.ToVersion == toVersion) 
				{
					toEdge = edgeToExplore;
					break;
				}

				foreach (DatabaseVersionEdge edge in edges.GetEdgesFrom(edgeToExplore.ToVersion))
				{
					if (edge.Unexplored)
					{
						edge.Unexplored = false;
						edge.PreviousEdge = edgeToExplore;
						seen.Enqueue(edge);
					}
				}
			}

			ArrayList upgradePath = new ArrayList();
			DatabaseVersionEdge currentEdge = toEdge;

			while (currentEdge != null)
			{
				upgradePath.Insert(0, currentEdge);
				currentEdge = currentEdge.PreviousEdge;
			}

			return upgradePath.ToArray(typeof(DatabaseVersionEdge)) as DatabaseVersionEdge[];
		}
	}
}
