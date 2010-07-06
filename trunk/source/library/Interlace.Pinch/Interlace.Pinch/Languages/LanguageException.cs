using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Interlace.Pinch.Languages
{
    [Serializable]
    public class LanguageException : Exception
    {
        public LanguageException() 
        { 
        }

        public LanguageException(string message) : base(message) 
        { 
        }

        public LanguageException(string message, Exception inner) : base(message, inner) 
        { 
        }

        protected LanguageException(SerializationInfo info, StreamingContext context)
            : base(info, context) 
        { 
        }
    }
}
