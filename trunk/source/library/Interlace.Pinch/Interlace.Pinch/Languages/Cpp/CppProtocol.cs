using System;
using System.Collections.Generic;
using System.Text;
using Interlace.PropertyLists;

namespace Interlace.Pinch.Languages.Cpp
{
    public class CppProtocol
    {
        PropertyDictionary _options;

        public CppProtocol(PropertyDictionary options)
        {
            _options = options;
        }

        public bool UsesNamespace
        {
            get { return _options.HasStringFor("namespace"); }
        }

        public string Namespace
        {
            get { return _options.StringFor("namespace"); }
        }

        public List<string> Includes
        {
            get 
            {
                List<string> includes = new List<string>();

                if (_options.HasArrayFor("includes"))
                {
                    PropertyArray array = _options.ArrayFor("includes");

                    for (int i = 0; i < array.Count; i++)
                    {
                        if (array[i] is string)
                        {
                            includes.Add(array[i] as string);
                        }
                    }
                }

                return includes;
            }
        }
    }
}
