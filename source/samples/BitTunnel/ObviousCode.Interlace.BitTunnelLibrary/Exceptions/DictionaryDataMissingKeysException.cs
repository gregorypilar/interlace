using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObviousCode.Interlace.BitTunnelLibrary.Exceptions
{
    [global::System.Serializable]
    public class DictionaryDataMissingKeysException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public DictionaryDataMissingKeysException() { }
        public DictionaryDataMissingKeysException(Type type) : base(
            "Cannot create " + type.Name + "from data. Unable to locate valid keys."
            ){}
        public DictionaryDataMissingKeysException(string message) : base(message) { }
        public DictionaryDataMissingKeysException(string message, Exception inner) : base(message, inner) { }
        protected DictionaryDataMissingKeysException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
