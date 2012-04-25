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

#endregion

namespace Interlace.Pinch.Languages.Python
{
    public class PythonStructureMember : BaseStructureMember
    {
        PythonType _type;
        PythonStructure _structure;

        PythonStructure _containedInStructure;

        public PythonStructureMember(StructureMember member, PythonType type, PythonStructure structure)
            : base(member)
        {
            _member = member;
            _type = type;
            _structure = structure;

            _containedInStructure = member.Parent.Implementation as PythonStructure;
        }

        public string DefaultValue
        {
            get
            {
                switch (_member.FieldContainerReference)
                {
                    case ContainerType.None:
                        return _member.Modifier == FieldModifier.Required ? _type.DefaultValue : "None";

                    case ContainerType.List:
                        return "[]";

                    case ContainerType.Set:
                        throw new InvalidOperationException();

                    case ContainerType.Map:
                        throw new InvalidOperationException();

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public StructureMember Member
        { 	 
            get { return _member; }
        }

        public string FieldIdentifier
        {
            get
            {
                return PythonLanguage.ToPrivateIdentifier(_member.Identifier);
            }
        }

        public string ConstantName
        {
            get 
            {
                return PythonLanguage.ToPrivateIdentifier(_member.Identifier).ToUpper();
            }
        }

        public string CountVariableName
        {
            get
            {
                return string.Format("{0}_count", _member.Identifier);
            }
        }

        public bool IsSurrogate
        {
            get
            {
                if (_structure == null) return false;

                return _structure.IsSurrogate;
            }
        }

        public string InnerTypeFactoryName
        {
            get
            {
                return _structure.Identifier + "Factory";
            }
        }

        public string InnerTypeCodecName
        {
            get
            {
                return _structure.Identifier;
            }
        }

        public override string RequiredOptionalModifier
        {
            get { return _member.Modifier == FieldModifier.Required ? "required" : "optional"; }
        }

        public override string CodecMethodSuffix
        {
            get
            {
                string modifier = RequiredOptionalModifier;

                if (_member.FieldTypeReference is IntrinsicTypeReference)
                {
                    IntrinsicTypeReference intrinsicTypeReference = _member.FieldTypeReference as IntrinsicTypeReference;

                    switch (intrinsicTypeReference.Type)
                    {
                        case IntrinsicType.Float32:
                            return modifier + "_float32";

                        case IntrinsicType.Float64:
                            return modifier + "_float64";

                        case IntrinsicType.Int8:
                            return modifier + "_int8";

                        case IntrinsicType.Int16:
                            return modifier + "_int16";

                        case IntrinsicType.Int32:
                            return modifier + "_int32";

                        case IntrinsicType.Int64:
                            return modifier + "_int64";

                        case IntrinsicType.Decimal:
                            return modifier + "_decimal";

                        case IntrinsicType.Bool:
                            return modifier + "_bool";

                        case IntrinsicType.String:
                            return modifier + "_string";

                        case IntrinsicType.Bytes:
                            return modifier + "_bytes";

                        default:
                            throw new InvalidOperationException();
                    }
                }
                else if (_member.FieldTypeReference is DeclarationTypeReference)
                {
                    DeclarationTypeReference declaration = _member.FieldTypeReference as DeclarationTypeReference;

                    if (declaration.Declaration is Enumeration)
                    {
                        return modifier + "_enumeration";
                    }
                    else 
                    {
                        return modifier + "_structure";
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public override string NullVersionLiteral
        {
            get { return "None"; }
        }
    }
}
