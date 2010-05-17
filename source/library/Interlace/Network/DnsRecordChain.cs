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
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Interlace.Network
{
    class DnsRecordChain : IDisposable
    {
        IntPtr _first;
        bool _firstRequiresDnsFree;

        List<IntPtr> _allocations;

        /// <summary>
        /// Creates and returns a chain from a record list returned from the Windows DNS API, freeing
        /// the chain using the correct DNS API release functions when disposed.
        /// </summary>
        /// <param name="first">The first record in the chain.</param>
        /// <returns>A wrapped chain instance.</returns>
        public static DnsRecordChain FromApiChain(IntPtr first)
        {
            DnsRecordChain chain = new DnsRecordChain();

            chain._first = first;
            if (!first.Equals(IntPtr.Zero)) chain._firstRequiresDnsFree = true;

            return chain;
        }

        public DnsRecordChain()
        {
            _first = IntPtr.Zero;
            _firstRequiresDnsFree = false;

            _allocations = new List<IntPtr>();

            _firstRequiresDnsFree = false;
        }

        internal IntPtr FirstRecord
        {
            get { return _first; }
        }

        public IEnumerator<IntPtr> GetEnumerator()
        {
            IntPtr currentResult = _first;

            while (!currentResult.Equals(IntPtr.Zero))
            {
                IntPtr next = Marshal.ReadIntPtr(currentResult, 0);

                yield return currentResult;

                currentResult = next;
            }

            yield break;
        }

        public static IntPtr GetDataPointer(IntPtr recordPointer)
        {
            return new IntPtr(recordPointer.ToInt32() + Marshal.SizeOf(typeof(DnsApi.DnsRecordHeader)));
        }

        public static DnsApi.DnsRecordHeader GetHeader(IntPtr recordPointer)
        {
            return (DnsApi.DnsRecordHeader)Marshal.PtrToStructure(recordPointer, typeof(DnsApi.DnsRecordHeader));
        }

        public IntPtr PrependRecord(string name, ushort type, uint flags, uint ttl, byte[] data)
        {
            IntPtr record = PrependRecord(name, type, (ushort)data.Length, flags, ttl);

            Marshal.Copy(data, 0, GetDataPointer(record), data.Length);

            return record;
        }

        internal IntPtr AllocatePoolUnicodeString(string s)
        {
            IntPtr pointer = Marshal.StringToHGlobalUni(s);
            _allocations.Add(pointer);

            return pointer;
        }

        public IntPtr PrependRecord(string name, ushort type, ushort dataLength, uint flags, uint ttl)
        {
            DnsApi.DnsRecordHeader header = new DnsApi.DnsRecordHeader();
            header.pNext = _first;
            header.pName = AllocatePoolUnicodeString(name);

            header.wType = type;
            header.wDataLength = dataLength;
            header.flags = flags;
            header.dwTtl = ttl;
            header.dwReserved = 0;

            IntPtr record = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DnsApi.DnsRecordHeader)) + dataLength);
            _allocations.Add(record);

            Marshal.StructureToPtr(header, record, false);

            _first = record;

            return record;
        }

        ~DnsRecordChain()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_firstRequiresDnsFree && _first != IntPtr.Zero)
                {
                    DnsApi.DnsRecordListFree(_first, DnsApi.DNS_FREE_RECORD_LIST);

                    _first = IntPtr.Zero;
                    _firstRequiresDnsFree = false;
                }

                foreach (IntPtr allocation in _allocations)
                {
                    Marshal.FreeHGlobal(allocation);
                }

                _allocations.Clear();

                _disposed = true;
            }
        }
    }
}
