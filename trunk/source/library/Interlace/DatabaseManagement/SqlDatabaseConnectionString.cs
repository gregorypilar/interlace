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

#endregion

namespace Interlace.DatabaseManagement
{
	public class SqlDatabaseConnectionString : DatabaseConnectionString
	{
		private string _serverName;

		private string _username;
		private string _password;

		private bool _useIntegratedAuthentication;

		private int _connectionTimeout;

		public SqlDatabaseConnectionString()
		{
			_serverName = ".";
			_databaseName = "master";

			_username = "";
			_password = "";

			_useIntegratedAuthentication = true;

			_connectionTimeout = 10;
		}

		public string ServerName
		{
			get { return _serverName; }
			set { _serverName = value; }
		}

		public string Username
		{
			get { return _username; }
			set { _username = value; }
		}

		public string Password
		{
			get { return _password; }
			set { _password = value; }
		}

		public bool UseIntegratedAuthentication
		{
			get { return _useIntegratedAuthentication; }
			set { _useIntegratedAuthentication = value; }
		}

		public int ConnectionTimeout
		{
			get { return _connectionTimeout; }
			set { _connectionTimeout = value; }
		}

		public override string GetStringRepresentation()
		{
			if (_useIntegratedAuthentication)
			{
				return String.Format("Server={0};Database={1};" + 
					"Trusted_Connection=True;Connect Timeout={2}", _serverName, _databaseName,
					_connectionTimeout);
			}
			else
			{
				return String.Format("Server={0};Database={1};User ID={2};Password={3};" + 
					"Trusted_Connection=False;Connect Timeout={4}", _serverName, _databaseName,
					_username, _password, _connectionTimeout);
			}
		}
	}
}
