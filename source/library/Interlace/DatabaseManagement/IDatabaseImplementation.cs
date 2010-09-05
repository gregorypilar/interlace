using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Interlace.DatabaseManagement
{
    public interface IDatabaseImplementation
    {
        void PrepareDoesDatabaseExistCommand(IDbCommand command, string databaseName);
        void PrepareCreateDatabaseCommand(IDbCommand command, string databaseName);
        void PrepareDoesVersionTableExist(IDbCommand command);
        void PrepareCreateVersionTableCommand(IDbCommand command);
        void PrepareDeleteVersionCommand(IDbCommand command);
    
        void PrepareInsertVersionCommand(IDbCommand command, DatabaseVersion version, 
			string failedUpgradeFromVersion, string failedUpgradeToVersion, 
			string failedUpgradeError);

        void PrepareFetchVersionRowsCommand(IDbCommand command);
    }
}
