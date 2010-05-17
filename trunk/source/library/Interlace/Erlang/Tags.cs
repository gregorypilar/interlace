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

#endregion

// Portions of this code were originally developed for Bit Plantation BitLibrary.
// (Portions Copyright © 2006 Bit Plantation)

using System;
using System.Collections.Generic;
using System.Text;

namespace Interlace.Erlang
{
    class Tags
    {
        internal const byte Version = 131;
        internal const byte Atom = 100;
        internal const byte SmallTuple = 104;
        internal const byte LargeTuple = 105;
        internal const byte SmallInteger = 97;
        internal const byte Integer = 98;
        internal const byte Float = 99;
        internal const byte Nil = 106;
        internal const byte String = 107;
        internal const byte List = 108;
        internal const byte Binary = 109;

        internal const byte Reference = 101;
        internal const byte Port = 102;
        internal const byte ProcessId = 103;
        internal const byte SmallBigNumber = 110;
        internal const byte LargeBigNumber = 111;
        internal const byte NewAtomCacheEntry = 78;
        internal const byte CachedAtom = 67;
        internal const byte NewReference = 114;
        internal const byte Function = 117;
        internal const byte NewFunction = 112;
        internal const byte ExportFunction = 113;
        internal const byte BitBinary = 77;
    }
}
