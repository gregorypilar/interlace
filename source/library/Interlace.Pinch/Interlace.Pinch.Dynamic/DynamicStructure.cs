using System;
using System.Collections.Generic;
using System.Text;

namespace Interlace.Pinch.Dynamic
{
    public class DynamicStructure
    {
        string _name;
        Dictionary<string, DynamicMember> _members;

        public DynamicStructure(string name)
        {
            _name = name;
            _members = new Dictionary<string, DynamicMember>();
        }

        public object this[string name]
        {
            get { return _members[name].Value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public Dictionary<string, DynamicMember> Members
        {
            get { return _members; }
        }
    }
}
