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

using Interlace.Collections;

#endregion

namespace Interlace.Pinch.Dom
{
    public class Protocol
    {
        NamespaceName _name;
        Versioning _versioning;
        ProtocolIdentifier _protocolIdentifier;
        TrackedBindingList<Declaration> _declarations;
        object _implementation;

        public Protocol()
        {
            _declarations = new TrackedBindingList<Declaration>();
            _declarations.Added += new EventHandler<TrackedBindingListEventArgs<Declaration>>(_declarations_Added);
            _declarations.Removed += new EventHandler<TrackedBindingListEventArgs<Declaration>>(_declarations_Removed);
        }

        void _declarations_Added(object sender, TrackedBindingListEventArgs<Declaration> e)
        {
            e.Item.Parent = this;
        }

        void _declarations_Removed(object sender, TrackedBindingListEventArgs<Declaration> e)
        {
            e.Item.Parent = null;
        }

        public NamespaceName Name
        { 	 
            get { return _name; }
            set { _name = value; }
        }

        public Versioning Versioning
        { 	 
            set { _versioning = value; }
        }

        public int Version
        {
            get { return _versioning.AddedInVersion; }
        }

        public ProtocolIdentifier ProtocolIdentifier
        { 	 
            get { return _protocolIdentifier; }
            set { _protocolIdentifier = value; }
        }

        public IList<Declaration> Declarations
        { 	 
            get { return _declarations; }
        }

        internal NamespaceName GetFullNameOfDeclaration(Declaration declaration)
        {
            return new NamespaceName(_name, declaration.Identifier);
        }

        public object Implementation
        { 	 
            get { return _implementation; }
            set { _implementation = value; }
        }
    }
}
