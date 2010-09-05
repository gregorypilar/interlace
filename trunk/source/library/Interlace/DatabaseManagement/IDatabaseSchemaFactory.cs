using System;
using System.Collections.Generic;
using System.Text;

namespace Interlace.DatabaseManagement
{
    public interface IDatabaseSchemaFactory
    {
        DatabaseSchema LoadDatabaseSchema();
    }
}
