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
using Interlace.Pinch.Languages.Java;

#endregion

namespace Interlace.Pinch.Languages
{
    public class JavaLanguage : Language
    {
        static Dictionary<IntrinsicType, JavaType> _intrinsics;

        static JavaLanguage()
        {
            _intrinsics = new Dictionary<IntrinsicType, JavaType>();
            _intrinsics.Add(IntrinsicType.Float32, new JavaType("float", "Float"));
            _intrinsics.Add(IntrinsicType.Float64, new JavaType("double", "Double"));
            _intrinsics.Add(IntrinsicType.Int8, new JavaType("byte", "Byte"));
            _intrinsics.Add(IntrinsicType.Int16, new JavaType("short", "Short"));
            _intrinsics.Add(IntrinsicType.Int32, new JavaType("int", "Integer"));
            _intrinsics.Add(IntrinsicType.Int64, new JavaType("long", "Long"));
            _intrinsics.Add(IntrinsicType.Decimal, new JavaType("java.math.BigDecimal", null));
            _intrinsics.Add(IntrinsicType.Bool, new JavaType("boolean", "Boolean"));
            _intrinsics.Add(IntrinsicType.String, new JavaType("String", null));
            _intrinsics.Add(IntrinsicType.Bytes, new JavaType("byte[]", null));
        }

        public JavaLanguage()
            : base("java", "Java") 
        {
        }

        public override object CreateStructureImplementationHelper(Structure structure, PropertyDictionary options)
        {
            return new JavaStructure(structure, options);
        }

        public override object CreateStructureMemberImplementationHelper(StructureMember member)
        {
            if (member.FieldTypeReference is IntrinsicTypeReference)
            {
                IntrinsicTypeReference reference = member.FieldTypeReference as IntrinsicTypeReference;

                return new JavaStructureMember(member, _intrinsics[reference.Type], null);
            }
            else
            {
                DeclarationTypeReference reference = member.FieldTypeReference as DeclarationTypeReference;

                NamespaceName name = reference.Declaration.QualifiedName;
                string codecName = reference.Declaration.Identifier;
                bool isReferenceType = !(reference.Declaration is Enumeration);

                if (reference.Declaration.Implementation is JavaStructure)
                {
                    JavaStructure structure = (JavaStructure)reference.Declaration.Implementation;

                    name = structure.ReferenceTypeName;
                    codecName = structure.Identifier;
                    isReferenceType = structure.IsReferenceType;
                }

                bool isInProtocolNamespace = 
                    name.ContainingName.Equals(member.Parent.Parent.Name);

                JavaType type;

                if (isInProtocolNamespace)
                {
                    type = new JavaType(name.UnqualifiedName, null);
                }
                else
                {
                    type = new JavaType(name.ToString(), null);
                }

                return new JavaStructureMember(member, type, reference.Declaration.Implementation as JavaStructure);
            }
        }

        public override void GenerateFiles(Generator generator, Document document)
        {
            foreach (Protocol protocol in document.Protocols)
            {
                foreach (Declaration declaration in protocol.Declarations)
                {
                    JavaStructure implementation = declaration.Implementation as JavaStructure;

                    string fileSuffix = implementation != null && implementation.IsSurrogate ? "Surrogate.java" : ".java";

                    generator.GenerateFile(
                        Path.Combine(generator.DestinationPath, declaration.Identifier + fileSuffix), 
                        Templates.JavaTemplate, "file", "Declaration", declaration);
                }
            }
        }

        public override object CreateEnumerationImplementationHelper(Enumeration enumeration, PropertyDictionary options)
        {
            return new JavaEnumeration(enumeration, options);
        }

        public override object CreateProtocolImplementationHelper(Protocol protocol, PropertyDictionary options) 
        {
            return new JavaProtocol(protocol, options);
        }
    }
}
