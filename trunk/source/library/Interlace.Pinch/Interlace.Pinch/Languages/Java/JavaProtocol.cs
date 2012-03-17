using System;
using System.Collections.Generic;
using System.Text;
using Interlace.Pinch.Dom;
using Interlace.PropertyLists;

namespace Interlace.Pinch.Languages.Java
{
    public class JavaProtocol
    {
        Protocol _protocol;
        PropertyDictionary _options;

        public JavaProtocol(Protocol protocol, PropertyDictionary options)
        {
            _protocol = protocol;
            _options = options;
        }

        public string PackageName
        {
            get
            {
                return _options.StringFor("package-name", "com.interlacelibrary.pinch.defaultpackage");
            }
        }
    }
}
