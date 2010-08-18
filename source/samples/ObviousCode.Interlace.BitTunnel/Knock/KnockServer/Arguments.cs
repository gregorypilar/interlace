using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KnockServer
{
    public class Arguments
    {
        string[] _args;
        public Arguments(string[] args)
        {
            _args = args;
        }

        public string Get(int index)
        {
            return _args[index];
        }

        public string Get(string argIdentifier)
        {
            foreach (string arg in _args)
            {
                if (arg.StartsWith(argIdentifier))
                {
                    return arg.Substring(argIdentifier.Length);
                }
            }

            return null;
        }
    }
}
