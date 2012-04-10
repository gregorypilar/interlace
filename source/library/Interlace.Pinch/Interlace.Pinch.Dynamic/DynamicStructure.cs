using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Interlace.Pinch.Dynamic
{
    public class DynamicStructure
    {
        string _name;
        string _identifier;
        Dictionary<string, DynamicMember> _members;
        List<string> _orderedMemberNames;

        public DynamicStructure(string name, string identifier)
        {
            _name = name;
            _identifier = identifier;
            _members = new Dictionary<string, DynamicMember>();
            _orderedMemberNames = new List<string>();
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

        public List<string> OrderedMemberNames
        {
            get { return _orderedMemberNames; }
        }

        public string Dumped
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                Dump(builder);

                return builder.ToString();
            }
        }

        public void Dump(StringBuilder builder)
        {
            Dump(builder, 0);
        }

        internal void Dump(StringBuilder builder, int indent)
        {
            string indentString = "".PadLeft(indent * 2);

            builder.AppendFormat("{0}({1})", indentString, _identifier);
            builder.AppendLine();

            foreach (string name in _orderedMemberNames)
            {
                if (!_members.ContainsKey(name)) continue;

                DynamicMember member = _members[name];

                builder.AppendFormat("{0}{1}: ", indentString, name);

                if (member.Value is List<object>)
                {
                    foreach (object value in (List<object>)member.Value)
                    {
                        builder.AppendFormat("{0}  ", indentString);

                        DumpValue(builder, indent, value);
                    }
                }
                else
                {
                    DumpValue(builder, indent, member.Value);
                }
            }
        }

        void DumpValue(StringBuilder builder, int indent, object value)
        {
            if (value is DynamicStructure)
            {
                builder.AppendLine();

                ((DynamicStructure)value).Dump(builder, indent + 1);
            }
            else if (value is byte[])
            {
                builder.AppendFormat("({0} bytes)", ((byte[])value).Length);
                builder.AppendLine();
            }
            else
            {
                builder.AppendFormat("{0}", value);
                builder.AppendLine();
            }
        }
    }
}
