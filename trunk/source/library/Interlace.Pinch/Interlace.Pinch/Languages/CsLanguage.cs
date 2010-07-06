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
using System.IO;
using System.Text;

using Interlace.Pinch.Dom;
using Interlace.Pinch.Generation;
using Interlace.PropertyLists;

#endregion

namespace Interlace.Pinch.Languages
{
    public class CsLanguage : Language
    {
        static Dictionary<IntrinsicType, CsType> _intrinsics;

        static CsLanguage()
        {
            _intrinsics = new Dictionary<IntrinsicType, CsType>();
            _intrinsics.Add(IntrinsicType.Float32, new CsType("float", false));
            _intrinsics.Add(IntrinsicType.Float64, new CsType("double", false));
            _intrinsics.Add(IntrinsicType.Int8, new CsType("byte", false));
            _intrinsics.Add(IntrinsicType.Int16, new CsType("short", false));
            _intrinsics.Add(IntrinsicType.Int32, new CsType("int", false));
            _intrinsics.Add(IntrinsicType.Int64, new CsType("long", false));
            _intrinsics.Add(IntrinsicType.Decimal, new CsType("decimal", false));
            _intrinsics.Add(IntrinsicType.Bool, new CsType("bool", false));
            _intrinsics.Add(IntrinsicType.String, new CsType("string", true));
            _intrinsics.Add(IntrinsicType.Bytes, new CsType("byte[]", true));
        }

        public CsLanguage()
            : base("cs", "C#")
        {
        }

        public override object CreateStructureImplementationHelper(Structure structure, PropertyDictionary options)
        {
            return new CsStructure(structure, options);
        }

        public override object CreateStructureMemberImplementationHelper(StructureMember member)
        {
            if (member.FieldTypeReference is IntrinsicTypeReference)
            {
                IntrinsicTypeReference reference = member.FieldTypeReference as IntrinsicTypeReference;

                return new CsStructureMember(member, _intrinsics[reference.Type], null);
            }
            else
            {
                DeclarationTypeReference reference = member.FieldTypeReference as DeclarationTypeReference;

                NamespaceName name = reference.Declaration.QualifiedName;
                string codecName = reference.Declaration.Identifier;
                bool isReferenceType = !(reference.Declaration is Enumeration);

                if (reference.Declaration.Implementation is CsStructure)
                {
                    CsStructure structure = (CsStructure)reference.Declaration.Implementation;

                    name = structure.ReferenceTypeName;
                    codecName = structure.Identifier;
                    isReferenceType = structure.IsReferenceType;
                }

                bool isInProtocolNamespace = 
                    name.ContainingName.Equals(member.Parent.Parent.Name);

                CsType type;

                if (isInProtocolNamespace)
                {
                    type = new CsType(name.UnqualifiedName, isReferenceType);
                }
                else
                {
                    type = new CsType(name.ToString(), isReferenceType);
                }

                return new CsStructureMember(member, type, reference.Declaration.Implementation as CsStructure);
            }
        }

        public override IEnumerable<LanguageOutput> GetLanguageOutputs(string baseName, string destinationPath)
        {
            yield return new LanguageOutput(Path.Combine(destinationPath, baseName + ".cs"), LanguageOutputTemplateKind.StringTemplate, Templates.CsTemplate, "file");
        }
    }
}
