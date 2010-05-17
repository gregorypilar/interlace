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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace Interlace.Erlang
{
    public class TermWriter : IDisposable
    {
        BinaryWriter _writer;

        public TermWriter(Stream stream)
        {
            _writer = new BinaryWriter(stream);
        }

        public void Close()
        {
            _writer.Close();
        }

        public void Dispose()
        {
            Close();
        }

        public static byte[] TermToBinary(object term)
        {
            MemoryStream stream = new MemoryStream();

            using (TermWriter writer = new TermWriter(stream))
            {
                writer.WriteTerm(term);

                return stream.ToArray();
            }
        }

        void WriteByte(byte value)
        {
            _writer.Write(value);
        }

        void WriteUnsignedShort(ushort value)
        {
            _writer.Write((ushort)IPAddress.HostToNetworkOrder((short)value));
        }

        void WriteUnsignedInteger(uint value)
        {
            _writer.Write((uint)IPAddress.HostToNetworkOrder((int)value));
        }

        public void WriteTerm(object term)
        {
            _writer.Write((byte)Tags.Version);

            WriteObject(term);

            _writer.Flush();
        }

        void WriteObject(object obj)
        {
            if (obj is ITermWritable)
            {
                ITermWritable writable = obj as ITermWritable;

                object substitutedObject = writable.SerializeToTerms();
                WriteObject(substitutedObject);
                return;
            }

            if (obj is Atom)
            {
                WriteAtom(obj as Atom);
                return;
            }

            if (obj is Tuple)
            {
                WriteTuple(obj as Tuple);
                return;
            }

            if (obj is int || obj is short)
            {
                WriteIntegerTerm((int)Convert.ToInt32(obj));
                return;
            }

            if (obj is uint || obj is ushort || obj is byte)
            {
                WriteIntegerTerm((int)Convert.ToUInt32(obj));
                return;
            }

            if (obj is string)
            {
                WriteString(obj as string);
                return;
            }
            
            if (obj is bool)
            {
                WriteObject((bool)obj ? Atom.From("true") : Atom.From("false"));
                return;
            }

            if (obj is byte[])
            {
                WriteBinary(obj as byte[]);
                return;
            }

            if (obj is ICollection)
            {
                WriteList(obj as ICollection);
                return;
            }

            if (obj is ErlangProcessId)
            {
                WriteProcessId(obj as ErlangProcessId);
                return;
            }

            throw new ErlangProtocolException(string.Format(
                "An object of an unsupported type ({0}) was serialized.", obj.GetType().FullName));

        }

        void WriteIntegerTerm(int integer)
        {
            if (0 <= integer && integer <= 255)
            {
                WriteByte(Tags.SmallInteger);
                WriteByte((byte)integer);
            }
            else
            {
                WriteByte(Tags.Integer);
                WriteUnsignedInteger((uint)integer);
            }
        }

        void WriteTuple(Tuple tuple)
        {
            if (tuple.Length < 256)
            {
                WriteByte(Tags.SmallTuple);
                WriteByte((byte)tuple.Length);
            }
            else
            {
                WriteByte(Tags.LargeTuple);
                WriteUnsignedInteger((uint)tuple.Length);
            }

            for (int i = 0; i < tuple.Length; i++)
            {
                WriteObject(tuple[i]);
            }
        }

        void WriteBinary(byte[] binary)
        {
            WriteByte(Tags.Binary);

            WriteUnsignedInteger((uint)binary.Length);
            _writer.Write(binary);
        }

        void WriteList(ICollection list)
        {
            if (list.Count > 0)
            {
                WriteByte(Tags.List);

                WriteUnsignedInteger((uint)list.Count);

                foreach (object element in list)
                {
                    WriteObject(element);
                }

                WriteByte(Tags.Nil);
            }
            else
            {
                WriteByte(Tags.Nil);
            }
        }

        void WriteAtom(Atom atom)
        {
            WriteByte(Tags.Atom);

            byte[] symbolBytes = Encoding.ASCII.GetBytes(atom.Value);

            WriteUnsignedShort((ushort)symbolBytes.Length);
            _writer.Write(symbolBytes);
        }

        void WriteString(string stringValue)
        {
            WriteByte(Tags.String);

            byte[] stringBytes = Encoding.ASCII.GetBytes(stringValue);
            WriteUnsignedShort((ushort)stringBytes.Length);
            _writer.Write(stringBytes);
        }

        void WriteProcessId(ErlangProcessId processId)
        {
            WriteByte(Tags.ProcessId);

            WriteAtom(processId.Node);
            WriteUnsignedInteger(processId.Id);
            WriteUnsignedInteger(processId.Serial);
            WriteByte(processId.Creation);
        }
    }
}
