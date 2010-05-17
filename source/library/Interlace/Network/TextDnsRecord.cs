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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Interlace.Network
{
    public class TextDnsRecord : DnsRecord
    {
        List<string> _lines;

        public TextDnsRecord(string name, int ttl)
        : base(name, DnsApi.DNS_RECORD_FLAGS_CHARSET_UNICODE, ttl)
        {
            _lines = new List<string>();
        }

        public TextDnsRecord(string name, int ttl, string text)
        : this(name, ttl)
        {
            Text = text;
        }

        internal static TextDnsRecord Unmarshal(IntPtr recordPointer)
        {
            IntPtr data = DnsRecordChain.GetDataPointer(recordPointer);
            DnsApi.DnsRecordHeader header = DnsRecordChain.GetHeader(recordPointer);

            int stringCountFieldSize = Marshal.SizeOf(typeof(int));
            int recordSize = Marshal.SizeOf(typeof(IntPtr));

            int stringCount = Marshal.ReadInt32(data, 0);

            StringBuilder builder = new StringBuilder();

            TextDnsRecord record = new TextDnsRecord(Marshal.PtrToStringUni(header.pName), (int)header.dwTtl);
            record._lines.Clear();

            for (int i = 0; i < stringCount; i++)
            {
                IntPtr stringPointer = Marshal.ReadIntPtr(data, stringCountFieldSize + i * recordSize);

                if (!stringPointer.Equals(IntPtr.Zero))
                {
                    string line = Marshal.PtrToStringUni(stringPointer);

                    record._lines.Add(line);
                }
            }

            return record;
        }

        public string Text
        {
            get 
            {
                return string.Join("\n", _lines.ToArray());
            }
            set
            {
                _lines.Clear();
                _lines.AddRange(value.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None));
            }
        }

        internal override void MarshalToChain(DnsRecordChain chain)
        {
            int pointerOffset = Marshal.SizeOf(typeof(int));
            int pointerSize = Marshal.SizeOf(typeof(IntPtr));

            int recordLength = pointerOffset + pointerSize * _lines.Count;
            
            if (recordLength > ushort.MaxValue) throw new InvalidOperationException();

            IntPtr record = MarshalBase(chain, DnsApi.DNS_TYPE_TEXT, (ushort)recordLength);
            IntPtr recordData = DnsRecordChain.GetDataPointer(record);

            Marshal.WriteInt32(recordData, 0, _lines.Count);

            for (int i = 0; i < _lines.Count; i++)
            {
                Marshal.WriteIntPtr(recordData, pointerOffset + pointerSize * i, chain.AllocatePoolUnicodeString(_lines[i]));
            }
        }
    }
}
