using System;
using System.Collections.Generic;
using System.Text;
using Interlace.Pinch.Dom;

namespace Interlace.Pinch.Dynamic
{
    public class DynamicPincherProtocol
    {
        Dictionary<string, Declaration> _declarations;

        public DynamicPincherProtocol(Protocol protocol)
        {
            _declarations = new Dictionary<string, Declaration>();

            foreach (Declaration declaration in protocol.Declarations)
            {
                _declarations[declaration.Identifier] = declaration;
            }
        }

        public Declaration FindDeclaration(string name)
        {
            if (_declarations.ContainsKey(name))
            {
                return _declarations[name];
            }
            else
            {
                return null;
            }
        }
    }
}
