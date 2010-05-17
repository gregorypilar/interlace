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

using SD.LLBLGen.Pro.ORMSupportClasses;

#endregion

namespace Interlace.Utilities
{
    public class EntityClonerField
    {
        readonly int _fieldIndex;
        readonly string _containingObjectName;
        readonly string _name;
        readonly bool _isPrimaryKey;

        public EntityClonerField(EntityField2 field)
        {
            _fieldIndex = field.FieldIndex;
            _containingObjectName = field.ContainingObjectName;
            _name = field.Name;
            _isPrimaryKey = field.IsPrimaryKey;
        }

        public int FieldIndex
        { 	 
           get { return _fieldIndex; }
        }

        public string ContainingObjectName
        { 	 
           get { return _containingObjectName; }
        }

        public string Name
        {
            get { return _name; }
        }

        public bool IsPrimaryKey
        {
            get { return _isPrimaryKey; }
        }

        public override bool Equals(object obj)
        {
            EntityClonerField rhs = obj as EntityClonerField;

            if (rhs == null) return false;

            return _fieldIndex == rhs._fieldIndex &&
                object.Equals(_containingObjectName, rhs._containingObjectName);
        }

        public override int GetHashCode()
        {
            int hashCode = _fieldIndex.GetHashCode();

            if (_containingObjectName != null) hashCode ^= _containingObjectName.GetHashCode();

            return hashCode;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
