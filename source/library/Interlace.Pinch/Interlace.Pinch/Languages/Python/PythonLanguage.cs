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
using System.Text.RegularExpressions;

#endregion

namespace Interlace.Pinch.Languages.Python
{
    public class PythonLanguage : Language
    {
        static Dictionary<IntrinsicType, PythonType> _intrinsics;

        static PythonLanguage()
        {
            _intrinsics = new Dictionary<IntrinsicType, PythonType>();
            _intrinsics.Add(IntrinsicType.Float32, new PythonType("float", "0.0"));
            _intrinsics.Add(IntrinsicType.Float64, new PythonType("double", "0.0"));
            _intrinsics.Add(IntrinsicType.Int8, new PythonType("int", "0"));
            _intrinsics.Add(IntrinsicType.Int16, new PythonType("int", "0"));
            _intrinsics.Add(IntrinsicType.Int32, new PythonType("int", "0"));
            _intrinsics.Add(IntrinsicType.Int64, new PythonType("long", "0"));
            _intrinsics.Add(IntrinsicType.Decimal, new PythonType("PinchDecimal", "pinch.PinchDecimal(False, 0, 0)"));
            _intrinsics.Add(IntrinsicType.Bool, new PythonType("bool", "False"));
            _intrinsics.Add(IntrinsicType.String, new PythonType("unicode", "None"));
            _intrinsics.Add(IntrinsicType.Bytes, new PythonType("str", "None"));
        }

        public PythonLanguage()
            : base("py", "Python")
        {
        }

        static Regex _identifierRegex = new Regex(@"\G([A-Z]?[^A-Z]+|[A-Z])");

        internal static string ToPrivateIdentifier(string identifier)
        {
            return "_" + ToPublicIdentifier(identifier);
        }

        internal static string ToPublicIdentifier(string identifier)
        {
            Match match = _identifierRegex.Match(identifier);

            string publicIdentifier = "";

            while (match.Success)
            {
                if (publicIdentifier != "") publicIdentifier += "_";

                publicIdentifier += match.Value.ToLower();

                match = match.NextMatch();
            }

            return publicIdentifier;
        }

        public override object CreateStructureImplementationHelper(Structure structure, PropertyDictionary options)
        {
            return new PythonStructure(structure, options);
        }

        public override object CreateStructureMemberImplementationHelper(StructureMember member)
        {
            if (member.FieldTypeReference is IntrinsicTypeReference)
            {
                IntrinsicTypeReference reference = member.FieldTypeReference as IntrinsicTypeReference;

                return new PythonStructureMember(member, _intrinsics[reference.Type], null);
            }
            else
            {
                DeclarationTypeReference reference = member.FieldTypeReference as DeclarationTypeReference;

                return new PythonStructureMember(member, new PythonType("object", "None"), reference.Declaration.Implementation as PythonStructure);
            }
        }

        public override object CreateEnumerationMemberImplementationHelper(EnumerationMember member) 
        {
            return new PythonEnumerationMember(member);
        }

        public override object CreateProtocolImplementationHelper(Protocol protocol, PropertyDictionary options)
        {
            return new PythonProtocol(protocol, options);
        }

        public override void GenerateFiles(Generator generator, Document document)
        {
            foreach (Protocol protocol in document.Protocols)
            {
                PythonProtocol pythonProtocol = protocol.Implementation as PythonProtocol;

                generator.GenerateFile(
                    Path.Combine(generator.DestinationPath, pythonProtocol.ModuleName + ".py"),
                    Templates.PythonTemplate, "file", "Protocol", protocol);
            }
        }
    }
}
