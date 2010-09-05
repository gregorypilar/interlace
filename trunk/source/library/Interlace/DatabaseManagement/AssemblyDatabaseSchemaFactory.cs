using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;

namespace Interlace.DatabaseManagement
{
    public class AssemblyDatabaseSchemaFactory : IDatabaseSchemaFactory
    {
        Assembly _assembly;
        string _schemaResourceName;

        protected AssemblyDatabaseSchemaFactory(Assembly assembly, string schemaResourceName)
        {
            _assembly = assembly;
            _schemaResourceName = schemaResourceName;
        }

        public static AssemblyDatabaseSchemaFactory FromEntryAssembly(string schemaResourceName)
        {
            return new AssemblyDatabaseSchemaFactory(Assembly.GetEntryAssembly(), schemaResourceName);
        }

        public static AssemblyDatabaseSchemaFactory FromCallingAssembly(string schemaResourceName)
        {
            return new AssemblyDatabaseSchemaFactory(Assembly.GetCallingAssembly(), schemaResourceName);
        }

        public static AssemblyDatabaseSchemaFactory FromAssembly(Assembly assembly, string schemaResourceName)
        {
            return new AssemblyDatabaseSchemaFactory(assembly, schemaResourceName);
        }

        public DatabaseSchema LoadDatabaseSchema()
        {
            using (Stream xmlStream = _assembly.GetManifestResourceStream(_schemaResourceName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DatabaseSchema));
                return serializer.Deserialize(xmlStream) as DatabaseSchema;
            }
        }
    }
}
