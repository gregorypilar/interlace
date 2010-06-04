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

namespace Interlace.Pinch.Languages
{
    public class BaseStructureMember
    {
        protected StructureMember _member;

        public BaseStructureMember(StructureMember member)
        {
            _member = member;
        }

        public string PropertyIdentifier
        {
            get 
            {
                return _member.Identifier;
            }
        }

        public string CodecMethodSuffix
        {
            get
            {
                string modifier = (_member.Modifier == FieldModifier.Required) ? "Required" : "Optional";

                if (_member.FieldTypeReference is IntrinsicTypeReference)
                {
                    IntrinsicTypeReference intrinsicTypeReference = _member.FieldTypeReference as IntrinsicTypeReference;

                    switch (intrinsicTypeReference.Type)
                    {
                        case IntrinsicType.Float32:
                            return modifier + "Float32";

                        case IntrinsicType.Float64:
                            return modifier + "Float64";

                        case IntrinsicType.Int8:
                            return modifier + "Int8";

                        case IntrinsicType.Int16:
                            return modifier + "Int16";

                        case IntrinsicType.Int32:
                            return modifier + "Int32";

                        case IntrinsicType.Int64:
                            return modifier + "Int64";

                        case IntrinsicType.Decimal:
                            return modifier + "Decimal";

                        case IntrinsicType.Bool:
                            return modifier + "Bool";

                        case IntrinsicType.String:
                            return modifier + "String";

                        case IntrinsicType.Bytes:
                            return modifier + "Bytes";

                        default:
                            throw new InvalidOperationException();
                    }
                }
                else if (_member.FieldTypeReference is DeclarationTypeReference)
                {
                    DeclarationTypeReference declaration = _member.FieldTypeReference as DeclarationTypeReference;

                    if (declaration.Declaration is Enumeration)
                    {
                        return modifier + "Enumeration";
                    }
                    else 
                    {
                        return modifier + "Structure";
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public bool IsInnerTypeStructure
        {
            get
            {
                if (_member.FieldTypeReference is DeclarationTypeReference)
                {
                    DeclarationTypeReference declaration = _member.FieldTypeReference as DeclarationTypeReference;

                    return declaration.Declaration is Structure;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool ContainerIsScalar
        {
            get
            {
                return _member.FieldContainerReference == ContainerType.None;
            }
        }

        public bool IsIntrinsic
        {
            get { return _member.FieldTypeReference is IntrinsicTypeReference; }
        }

        public bool IsEnumeration
        {
            get 
            {
                return _member.FieldTypeReference is DeclarationTypeReference &&
                    ((DeclarationTypeReference)_member.FieldTypeReference).Declaration is Enumeration;
            }
        }

        public bool IsStructure
        {
            get 
            {
                return _member.FieldTypeReference is DeclarationTypeReference &&
                    ((DeclarationTypeReference)_member.FieldTypeReference).Declaration is Structure;
            }
        }

        public bool IsOptional
        {
            get { return _member.Modifier == FieldModifier.Optional; }
        }

        public bool IsRequired
        {
            get { return _member.Modifier == FieldModifier.Required; }
        }

        public string ContainerTag
        {
            get
            {
                switch (_member.FieldContainerReference)
                {
                    case ContainerType.None:
                        return "scalar";

                    case ContainerType.List:
                        return "list";

                    case ContainerType.Set:
                        throw new InvalidOperationException();

                    case ContainerType.Map:
                        throw new InvalidOperationException();

                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
