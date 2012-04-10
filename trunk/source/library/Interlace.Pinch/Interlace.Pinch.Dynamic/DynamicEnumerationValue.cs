using System;
using System.Collections.Generic;
using System.Text;

namespace Interlace.Pinch.Dynamic
{
    public class DynamicEnumerationValue
    {
        string _enumerationName;
        string _valueName;
        int _value;

        public DynamicEnumerationValue(string enumerationName, string valueName, int value)
        {
            _enumerationName = enumerationName;
            _valueName = valueName;
            _value = value;
        }

        public string EnumerationName
        {
            get { return _enumerationName; }
        }

        public string ValueName
        {
            get { return _valueName; }
        }

        public int Value
        {
            get { return _value; }
        }

        public override string ToString()
        {
            return string.Format("({0}) {1}, {2}", _enumerationName, _valueName, _value);
        }
    }
}
