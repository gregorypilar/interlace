using System;
using System.Collections.Generic;
using System.Text;
using Interlace.Pinch.Dom;

namespace Interlace.Pinch.Languages.Python
{
    public class PythonEnumerationMember
    {
        EnumerationMember _member;

        public PythonEnumerationMember(EnumerationMember member)
        {
            _member = member;
        }

        public string ConstantName
        {
            get 
            {
                return PythonLanguage.ToPublicIdentifier(_member.Identifier).ToUpper();
            }
        }
    }
}
