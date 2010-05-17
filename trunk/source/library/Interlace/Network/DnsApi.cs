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
    static class DnsApi
    {
        public const int DNS_TYPE_A = 1;
        public const int DNS_TYPE_NS = 2;
        public const int DNS_TYPE_MD = 3;
        public const int DNS_TYPE_MF = 4;
        public const int DNS_TYPE_CNAME = 5;
        public const int DNS_TYPE_SOA = 6;
        public const int DNS_TYPE_MB = 7;
        public const int DNS_TYPE_MG = 8;
        public const int DNS_TYPE_MR = 9;
        public const int DNS_TYPE_NULL = 10;
        public const int DNS_TYPE_WKS = 11;
        public const int DNS_TYPE_PTR = 12;
        public const int DNS_TYPE_HINFO = 13;
        public const int DNS_TYPE_MINFO = 14;
        public const int DNS_TYPE_MX = 15;
        public const int DNS_TYPE_TEXT = 16;

        public const uint DNS_QUERY_STANDARD = 0x00000000;
        public const uint DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 0x00000001;
        public const uint DNS_QUERY_USE_TCP_ONLY = 0x00000002;
        public const uint DNS_QUERY_NO_RECURSION = 0x00000004;
        public const uint DNS_QUERY_BYPASS_CACHE = 0x00000008;

        public const uint DNS_QUERY_NO_WIRE_QUERY = 0x00000010;
        public const uint DNS_QUERY_NO_LOCAL_NAME = 0x00000020;
        public const uint DNS_QUERY_NO_HOSTS_FILE = 0x00000040;
        public const uint DNS_QUERY_NO_NETBT = 0x00000080;

        public const uint DNS_QUERY_WIRE_ONLY = 0x00000100;
        public const uint DNS_QUERY_RETURN_MESSAGE = 0x00000200;

        public const uint DNS_QUERY_TREAT_AS_FQDN = 0x00001000;
        public const uint DNS_QUERY_DONT_RESET_TTL_VALUES = 0x00100000;
        public const uint DNS_QUERY_RESERVED = 0xff000000;

        public const int DNS_FREE_RECORD_LIST = 1;

        public const int DNS_RECORD_FLAGS_CHARSET_MASK = 0x18;
        public const int DNS_RECORD_FLAGS_CHARSET_UNICODE = 0x08;

        [StructLayout(LayoutKind.Sequential)]
        public struct DnsRecordHeader
        {
            // This structure must not contain anything that would cause
            // memory allocation when being marshaled and unmarshaled, like strings;
            // the code using it depends on this to avoid leaks.

            public IntPtr pNext;
            public IntPtr pName;
            public ushort wType;
            public ushort wDataLength;
            public uint flags;
            public uint dwTtl;
            public uint dwReserved;
        }

        public const uint SEC_WINNT_AUTH_IDENTITY_UNICODE = 0x02;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class SecWinNtAuthIdentity
        {
            public string User;
            public int UserLength;

            public string Domain;
            public int DomainLength;

            public string Password;
            public int PasswordLength;

            public uint Flags;

            public SecWinNtAuthIdentity(string user, string domain, string password)
            {
                User = user;
                UserLength = user != null ? user.Length : 0;

                Domain = domain;
                DomainLength = domain != null ? domain.Length : 0;

                Password = password;
                PasswordLength = password != null ? password.Length : 0;

                Flags = SEC_WINNT_AUTH_IDENTITY_UNICODE;
            }
        }

        [DllImport("dnsapi.dll", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern int DnsQuery(string lpstrName,
            int wType, uint fOptions, IntPtr pExtra, out IntPtr ppQueryResultsSet,
            IntPtr pReserved);

        [DllImport("dnsapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);

        [DllImport("dnsapi.dll", EntryPoint = "DnsAcquireContextHandle_W", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern int DnsAcquireContextHandle(int credentialsFlags, 
            [In, MarshalAs(UnmanagedType.LPStruct)] SecWinNtAuthIdentity credentials, out IntPtr handle);

        [DllImport("dnsapi.dll", EntryPoint = "DnsReleaseContextHandle", CharSet = CharSet.Unicode, SetLastError = false, ExactSpelling = true)]
        public static extern void DnsReleaseContextHandle(IntPtr handle);


        public const int DNS_UPDATE_SECURITY_USE_DEFAULT = 0x0;
        public const int DNS_UPDATE_SECURITY_OFF = 0x10;
        public const int DNS_UPDATE_SECURITY_ON = 0x20;
        public const int DNS_UPDATE_SECURITY_ONLY = 0x100;
        public const int DNS_UPDATE_CACHE_SECURITY_CONTEXT = 0x200;
        public const int DNS_UPDATE_TEST_USE_LOCAL_SYS_ACCT = 0x400;
        public const int DNS_UPDATE_FORCE_SECURITY_NEGO = 0x800;

        [DllImport("dnsapi.dll", EntryPoint = "DnsModifyRecordsInSet_W", CharSet = CharSet.Unicode, SetLastError = false, ExactSpelling = true)]
        public static extern int DnsModifyRecordsInSet(IntPtr pAddRecords, IntPtr pDeleteRecords, int Options,
            IntPtr hContext, byte[] pServerList, IntPtr pReserved);

        [DllImport("dnsapi.dll", EntryPoint = "DnsReplaceRecordSetW", CharSet = CharSet.Unicode, SetLastError = false, ExactSpelling = true)]
        public static extern int DnsReplaceRecordSet(IntPtr pNewSet, int Options,
            IntPtr hContext, byte[] pServerList, IntPtr pReserved);
    }
}
