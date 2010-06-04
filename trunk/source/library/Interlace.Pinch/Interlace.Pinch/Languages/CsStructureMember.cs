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
    public class CsStructureMember : BaseStructureMember
    {
        CsType _type;

        public CsStructureMember(StructureMember member, CsType type)
            : base(member)
        {
            _member = member;
            _type = type;
        }

        public StructureMember Member
        { 	 
            get { return _member; }
        }

        public string FieldIdentifier
        {
            get
            {
                return string.Format("_{0}{1}", _member.Identifier.Substring(0, 1).ToLower(), _member.Identifier.Substring(1));
            }
        }

        public string CountVariableName
        {
            get
            {
                return string.Format("{0}{1}Count", _member.Identifier.Substring(0, 1).ToLower(), _member.Identifier.Substring(1));
            }
        }

        public string AddedInVersion
        {
            get { return _member.Versioning.AddedInVersion.ToString(); }
        }

        public string RemovedInVersion
        {
            get 
            { 
                if (_member.Versioning.RemovedInVersion.HasValue)
                {
                    return _member.Versioning.RemovedInVersion.ToString();
                }
                else
                {
                    return "null";
                }
            }
        }
        public string InnerTypeName
        {
            get
            {
                string postFix;

                if (_member.Modifier == FieldModifier.Optional && !_type.IsReferenceType && 
                    _member.FieldContainerReference == ContainerType.None)
                {
                    postFix = "?";
                }
                else
                {
                    postFix = "";
                }

                return _type.NativeTypeName + postFix;
            }
        }

        public string TypeName
        {
            get
            {
                switch (_member.FieldContainerReference)
                {
                    case ContainerType.None:
                        return string.Format("{0}", InnerTypeName);

                    case ContainerType.List:
                        return string.Format("List<{0}>", InnerTypeName);

                    case ContainerType.Set:
                        return string.Format("Set<{0}>", InnerTypeName);

                    case ContainerType.Map:
                        throw new InvalidOperationException();

                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
