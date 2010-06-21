#region Using Directives and Copyright Notice

// Copyright (c) 2007-2010, Computer Consultancy Pty Ltd
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Computer Consultancy Pty Ltd nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;

using Interlace.Pinch.Dom;
using Interlace.Pinch.Languages;

#endregion

namespace Interlace.Pinch.Generation
{
    public abstract class Language
    {
        readonly string _name; 
        readonly string _description;

        public Language(string name, string description)
        {
            _name = name;  
            _description = description;
        }

        public static Language Cs
        {
            get { return new CsLanguage(); }
        }

        public string Name
        { 	 
            get { return _name; }
        }

        public string Description
        { 	 
            get { return _description; }
        }

        public virtual void CreateDomImplementationHelpers(Document document)
        {
            foreach (Protocol protocol in document.Protocols)
            {
                protocol.Implementation = CreateProtocolImplementationHelper(protocol);

                foreach (Declaration declaration in protocol.Declarations)
                {
                    if (declaration is Enumeration)
                    {
                        Enumeration enumeration = (Enumeration)declaration;

                        enumeration.Implementation = CreateEnumerationImplementationHelper(enumeration);

                        foreach (EnumerationMember member in enumeration.MemberBases)
                        {
                            member.Implementation = CreateEnumerationMemberImplementationHelper(member);
                        }
                    }
                    else if (declaration is Structure)
                    {
                        Structure structure = (Structure)declaration;

                        structure.Implementation = CreateStructureImplementationHelper(structure);

                        foreach (StructureMember member in structure.Members)
                        {
                            member.Implementation = CreateStructureMemberImplementationHelper(member);
                        }
                    }
                }
            }
        }

        public virtual object CreateProtocolImplementationHelper(Protocol protocol) { return null; }
        public virtual object CreateEnumerationImplementationHelper(Enumeration enumeration) { return null; }
        public virtual object CreateEnumerationMemberImplementationHelper(EnumerationMember member) { return null; }
        public virtual object CreateStructureImplementationHelper(Structure structure) { return null; }
        public virtual object CreateStructureMemberImplementationHelper(StructureMember member) { return null; }

        public abstract IEnumerable<LanguageOutput> GetLanguageOutputs(string baseName, string destinationPath);
    }
}
