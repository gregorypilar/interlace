using System;
using System.Collections.Generic;
using System.Text;

namespace Interlace.Pinch.Dynamic
{
    public class DynamicMember
    {
        string _identifier;
        object _value;

        public DynamicMember(string identifier, object value)
        {
            _identifier = identifier;
            _value = value;
        }

        public string Identifier
        {
            get { return _identifier; }
        }

        public object Value
        {
            get { return _value; }
        }
    }
}
