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

#endregion

namespace DatabaseCop
{
    public enum PlanForeignKeyActionKind
    {
        /// <summary>
        /// The relation will not cause dependency issues because it is a
        /// precondition of the operation that no instances of the
        /// relation will exist.
        /// </summary>
        Precondition,

        /// <summary>
        /// Any instances of the relation will be destroyed.
        /// </summary>
        UpdateColumns,

        /// <summary>
        /// The relation is not checked directly, but due to some other
        /// precondition can not be an issue.
        /// </summary>
        ImpliedPrecondition,
    }

    public class PlanForeignKeyAction
    {
        ForeignKeyConstraint _constraint;
        PlanForeignKeyActionKind _kind;

        public PlanForeignKeyAction(ForeignKeyConstraint constraint, PlanForeignKeyActionKind kind)
        {
            _constraint = constraint;
            _kind = kind;
        }

        public ForeignKeyConstraint Constraint
        { 	 
            get { return _constraint; }
        }

        public PlanForeignKeyActionKind Kind
        { 	 
            get { return _kind; }
        }
    }
}
