using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;

namespace Interlace.ReactorCore
{
    [Serializable]
    public class DnsResolutionException : Exception
    {
        public DnsResolutionException() 
        { 
        }

        public DnsResolutionException(string message) : base(message) 
        { 
        }

        public DnsResolutionException(string message, Exception inner) : base(message, inner) 
        { 
        }

        protected DnsResolutionException(SerializationInfo info, StreamingContext context)
        : base(info, context) 
        { 
        }
    }
}
