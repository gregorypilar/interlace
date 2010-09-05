using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Interlace.DatabaseManagement
{
    public class DatabaseUpgrader
    {
		DatabaseManager _manager;
        string _requiredVersion;

        public DatabaseUpgrader(IDbConnection connection, IDatabaseImplementation implementation, IDatabaseSchemaFactory factory, string requiredProduct, string requiredVersion)
        {
            _manager = new DatabaseManager(connection, implementation, factory, requiredProduct);
            _requiredVersion = requiredVersion;
        }

        string GetVersion()
        {
			if (_manager.VersionTableExists())
			{
				DatabaseVersion version = _manager.GetVersion();

				return version.Version;
			}
			else
			{
				return DatabaseManager.UninitialisedVersion;
			}
        }

		public void UpdateIfRequired(DatabaseManager.UpgradeProgressDelegate progressDelegate)
		{
			string fromVersion = GetVersion();

			// Exit unless an upgrade is needed:
			if (fromVersion == _requiredVersion) return;

            _manager.Upgrade(fromVersion, _requiredVersion, progressDelegate);
		}

        public bool IsUpgradeRequired()
        {
			string fromVersion = GetVersion();

            return fromVersion != _requiredVersion;
        }
    }
}
