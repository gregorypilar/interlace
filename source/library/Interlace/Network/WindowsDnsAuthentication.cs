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

namespace Interlace.Network
{
    public class WindowsDnsAuthentication : DnsAuthentication
    {
        IntPtr _context;

        bool _disposed;

        public WindowsDnsAuthentication(string username, string domain, string password)
        {
            const int DACH_FLAGS_UNICODE = 1;

            DnsApi.SecWinNtAuthIdentity identity = new DnsApi.SecWinNtAuthIdentity(username, domain, password);

            int result = DnsApi.DnsAcquireContextHandle(DACH_FLAGS_UNICODE, identity, out _context);

            if (result != 0 || _context == IntPtr.Zero) throw new DnsException(
                "An authentication handle could not be created for Windows authentication for DNS updates.");
        }

        internal override void PrepareAuthentication(out IntPtr contextOrZero, ref int flags)
        {
            contextOrZero = _context;
            flags = DnsApi.DNS_UPDATE_SECURITY_ON;
        }

        protected override void Dispose(bool disposing)
        {
             if (!_disposed)
             {
                 DnsApi.DnsReleaseContextHandle(_context);
                 _context = IntPtr.Zero;

                _disposed = true;
             }
        }
    }
}
