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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft;

using Interlace.Collections;
using Interlace.Utilities;

#endregion

namespace Interlace.Network
{
    public class DnsExtensions
    {
        static byte[] GetAddressList(IPAddress[] addresses)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((uint)addresses.Length);

                    foreach (IPAddress address in addresses)
                    {
                        if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            throw new InvalidOperationException();
                        }

                        byte[] bytes = address.GetAddressBytes();

                        if (bytes.Length != 4) throw new InvalidOperationException();

                        writer.Write(bytes);
                    }

                    writer.Flush();

                    return stream.ToArray();
                }
            }
        }

        public static void ReplaceRecords(string domain, DnsRecordCollection records, DnsAuthentication authentication)
        {
            if (authentication == null) throw new ArgumentNullException("authentication");

            using (DnsRecordChain chain = records.Marshal())
            {
                IPAddress[] addresses = ResolveNameServers(domain);
                byte[] addressBytes = GetAddressList(addresses);

                IntPtr context;
                int flags = 0;

                authentication.PrepareAuthentication(out context, ref flags);

                int result = DnsApi.DnsReplaceRecordSet(chain.FirstRecord, flags, context, addressBytes, IntPtr.Zero);

                if (result != 0) throw new DnsException();
            }
        }

        public static void UpdateRecords(string domain, DnsRecordCollection addRecords, DnsRecordCollection removeRecords, DnsAuthentication authentication)
        {
            if (authentication == null) throw new ArgumentNullException("authentication");

            using (DnsRecordChain addRecordsChain = addRecords.Marshal())
            {
                using (DnsRecordChain removeRecordsChain = removeRecords.Marshal())
                {
                    IPAddress[] addresses = ResolveNameServers(domain);
                    byte[] addressBytes = GetAddressList(addresses);

                    IntPtr context;
                    int flags = 0;

                    authentication.PrepareAuthentication(out context, ref flags);

                    int result = DnsApi.DnsModifyRecordsInSet(
                        addRecordsChain.FirstRecord, removeRecordsChain.FirstRecord, flags, context, addressBytes, IntPtr.Zero);

                    if (result != 0) throw new DnsException();
                }
            }
        }

        delegate T Unmarshaller<T>(IntPtr recordPointer) where T : DnsRecord;

        static DnsRecordsResponse<T> ResolveRecords<T>(string name, Unmarshaller<T> unmarshaller, ushort recordType) where T : DnsRecord
        {
            IntPtr queryResultSet;

            int result = DnsApi.DnsQuery(name, recordType,
                DnsApi.DNS_QUERY_BYPASS_CACHE, IntPtr.Zero, out queryResultSet, IntPtr.Zero);

            using (DnsRecordChain chain = DnsRecordChain.FromApiChain(queryResultSet))
            {
                DnsRecordsResponse<T> response = new DnsRecordsResponse<T>();

                if (result != 0)
                {
                    response.Fail(result);
                }
                else
                {
                    foreach (IntPtr record in chain)
                    {
                        DnsApi.DnsRecordHeader header = (DnsApi.DnsRecordHeader)Marshal.PtrToStructure(
                            record, typeof(DnsApi.DnsRecordHeader));

                        if (header.wType == recordType)
                        {
                            response.AddRecord(unmarshaller(record));
                        }
                    }

                    response.Succeed();
                }

                return response;
            }
        }

        public static DnsRecordsResponse<TextDnsRecord> ResolveTextRecords(string name)
        {
            return ResolveRecords<TextDnsRecord>(name, TextDnsRecord.Unmarshal, DnsApi.DNS_TYPE_TEXT);
        }

        public static DnsRecordsResponse<PointerDnsRecord> ResolveNsRecords(string name)
        {
            return ResolveRecords<PointerDnsRecord>(name, PointerDnsRecord.Unmarshal, DnsApi.DNS_TYPE_NS);
        }

        public static IPAddress[] ResolveNameServers(string domain)
        {
            DnsRecordsResponse<PointerDnsRecord> response = ResolveNsRecords(domain);

            if (response.Failed) throw new DnsException();

            Set<IPAddress> addresses = new Set<IPAddress>();

            foreach (PointerDnsRecord pointer in response.Records)
            {
                IPHostEntry entry = Dns.GetHostEntry(pointer.PointerName);

                addresses.UnionUpdate(entry.AddressList);
            }

            return addresses.ToArray();
        }
    }
}
