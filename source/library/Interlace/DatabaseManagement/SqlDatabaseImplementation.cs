using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Interlace.DatabaseManagement
{
    public class SqlDatabaseImplementation : IDatabaseImplementation
    {
        public void PrepareDoesDatabaseExistCommand(IDbCommand command, string databaseName)
        {
			command.CommandText = "SELECT COUNT(*) FROM master.dbo.sysdatabases " +
				"WHERE name = @Name";

            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = "@Name";
            parameter.DbType = DbType.String;
            parameter.Value = databaseName;

            command.Parameters.Add(parameter);
        }

        public void PrepareCreateDatabaseCommand(IDbCommand command, string databaseName)
        {
			command.CommandText = String.Format("CREATE DATABASE [{0}]", 
				databaseName);
        }

        public void PrepareDoesVersionTableExist(IDbCommand command)
        {
			command.CommandText = 
				"SELECT COUNT(*) FROM dbo.sysobjects WHERE " +
				"  id = object_id('[dbo].[Version]') AND " +
				"  OBJECTPROPERTY(id, N'IsUserTable') = 1";
        }

        public void PrepareCreateVersionTableCommand(IDbCommand command)
        {
			command.CommandText = 
				"CREATE TABLE [dbo].[Version] (" +
				"	[Product] [varchar] (250) NOT NULL ," +
				"	[Version] [varchar] (50) NOT NULL ," +
				"	[FailedUpgradeFromVersion] [varchar] (50) NULL ," +
				"	[FailedUpgradeToVersion] [varchar] (50) NULL ," +
				"	[FailedUpgradeError] [varchar] (4000) NULL " +
				") ON [PRIMARY]";
        }

        public void PrepareDeleteVersionCommand(IDbCommand command)
        {
			command.CommandText = "DELETE FROM Version";
        }

        public void PrepareInsertVersionCommand(IDbCommand command, DatabaseVersion version, 
			string failedUpgradeFromVersion, string failedUpgradeToVersion, string failedUpgradeError)
        {
			command.CommandText = "INSERT INTO Version (Product, Version, " +
				"FailedUpgradeFromVersion, FailedUpgradeToVersion, FailedUpgradeError) " +
				"VALUES (@Product, @Version, @FailedUpgradeFromVersion, " +
				"@FailedUpgradeToVersion, @FailedUpgradeError)";

            IDbDataParameter productParameter = command.CreateParameter();
            IDbDataParameter versionParameter = command.CreateParameter();
            IDbDataParameter failedUpgradeFromVersionParameter = command.CreateParameter();
            IDbDataParameter failedUpgradeToVersionParameter = command.CreateParameter();
            IDbDataParameter failedUpgradeErrorParameter = command.CreateParameter();

            command.Parameters.Add(productParameter);
            command.Parameters.Add(versionParameter);
            command.Parameters.Add(failedUpgradeFromVersionParameter);
            command.Parameters.Add(failedUpgradeToVersionParameter);
            command.Parameters.Add(failedUpgradeErrorParameter);

            productParameter.ParameterName = "@Product";
            productParameter.DbType = DbType.String;
            productParameter.Size = 250;

            productParameter.ParameterName = "@Version";
            versionParameter.DbType = DbType.String;
            versionParameter.Size = 250;

            failedUpgradeFromVersionParameter.ParameterName = "@FailedUpgradeFromVersion";
            failedUpgradeFromVersionParameter.DbType = DbType.String;
            failedUpgradeFromVersionParameter.Size = 50;

            failedUpgradeToVersionParameter.ParameterName = "@FailedUpgradeToVersion";
            failedUpgradeToVersionParameter.DbType = DbType.String;
            failedUpgradeToVersionParameter.Size = 50;

            failedUpgradeErrorParameter.ParameterName = "@FailedUpgradeError";
            failedUpgradeErrorParameter.DbType = DbType.String;
            failedUpgradeErrorParameter.Size = 4000;

            productParameter.Value = version.Product;
            versionParameter.Value = version.Version;

			if (failedUpgradeFromVersion != null)
			{
                failedUpgradeFromVersionParameter.Value = failedUpgradeFromVersion;
                failedUpgradeToVersionParameter.Value = failedUpgradeToVersion;
                failedUpgradeErrorParameter.Value = failedUpgradeError;
			}
			else
			{
                failedUpgradeFromVersionParameter.Value = DBNull.Value;
                failedUpgradeToVersionParameter.Value = DBNull.Value;
                failedUpgradeErrorParameter.Value = DBNull.Value;
			}
        }
    
        public void  PrepareFetchVersionRowsCommand(IDbCommand command)
        {
			command.CommandText = "SELECT TOP 2 * FROM Version";
        }
    }
}
