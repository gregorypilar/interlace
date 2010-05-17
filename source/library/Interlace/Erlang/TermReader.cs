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
using System.Collections;
using System.Text;
using System.IO;
using System.Net;

namespace Interlace.Erlang
{
    public class TermReader : IDisposable
    {
        BinaryReader _reader;

        public TermReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public void Close()
        {
            _reader.Close();
        }

        public void Dispose()
        {
            Close();
        }

        byte ReadByte()
        {
            return _reader.ReadByte();
        }

        ushort ReadUnsignedShort()
        {
            return (ushort)IPAddress.NetworkToHostOrder((short)_reader.ReadUInt16());
        }

        uint ReadUnsignedInteger()
        {
            return (uint)IPAddress.NetworkToHostOrder((int)_reader.ReadUInt32());
        }

        public object ReadTerm()
        {
            byte magic = ReadByte();

            if (magic != Tags.Version)
            {
                throw new CodecException("The term does not begin with a known version magic number.");
            }

            return ReadObject();
        }

        object ReadObject()
        {
            byte tag = ReadByte();

            switch (tag)
            {
                case Tags.Atom:
                    return ReadAtom();

                case Tags.SmallTuple:
                    int arity = (int)ReadByte();
                    return ReadTupleData(arity);

                case Tags.LargeTuple:
                    arity = (int)ReadUnsignedInteger();
                    return ReadTupleData(arity);

                case Tags.SmallInteger:
                    return (int)ReadByte();

                case Tags.Integer:
                    return (int)ReadUnsignedInteger();

                case Tags.Nil:
                    return new List<object>();

                case Tags.String:
                    return ReadString();

                case Tags.List:
                    return ReadList();

                case Tags.Binary:
                    return ReadBinary();

                case Tags.Float:
                    throw new CodecException("Float terms are not supported.");

                case Tags.Reference:
                    throw new CodecException("Reference terms are not supported.");

                case Tags.Port:
                    throw new CodecException("Port terms are not supported.");

                case Tags.ProcessId:
                    return ReadProcessId();

                case Tags.SmallBigNumber:
                    throw new CodecException("Small big number terms are not supported.");

                case Tags.LargeBigNumber:
                    throw new CodecException("Large big number terms are not supported.");

                case Tags.NewAtomCacheEntry:
                    throw new CodecException("Cached atom terms are not supported.");

                case Tags.CachedAtom:
                    throw new CodecException("Cached atom terms are not supported.");

                case Tags.NewReference:
                    throw new CodecException("New reference terms are not supported.");

                case Tags.Function:
                    throw new CodecException("Function terms are not supported.");

                case Tags.NewFunction:
                    throw new CodecException("Function terms are not supported.");

                case Tags.ExportFunction:
                    throw new CodecException("Function terms are not supported.");

                case Tags.BitBinary:
                    throw new CodecException("Bit (odd length) binary terms are not supported.");

                default:
                    throw new CodecException(string.Format(
                        "Unrecognised term tag ({0}) found while decoding term.", tag));
            }
        }

        byte[] ReadBinary()
        {
            uint length = ReadUnsignedInteger();
            byte[] binaryBytes = _reader.ReadBytes((int)length);

            if (binaryBytes.Length != length)
            {
                throw new CodecException("End of stream while reading a binary term.");
            }

            return binaryBytes;
        }

        ErlangProcessId ReadProcessId()
        {
            Atom atom = ReadObject() as Atom;

            if (atom == null) throw new CodecException("A process id atom was expected, but some other term was found.");

            uint id = ReadUnsignedInteger();
            uint serial = ReadUnsignedInteger();
            byte creation = ReadByte();

            return new ErlangProcessId(atom, id, serial, creation);
        }

        List<object> ReadList()
        {
            uint length = ReadUnsignedInteger();
            List<object> list = new List<object>((int)length);

            for (int i = 0; i < length; i++)
            {
                list.Add(ReadObject());
            }

            object tail = ReadObject();

            if (!(tail is List<object> && (tail as List<object>).Count == 0))
            {
                throw new ErlangProtocolException("Improper lists (those with a tail that is not a NIL) are not supported.");
            }

            return list;
        }

        Atom ReadAtom()
        {
            ushort length = ReadUnsignedShort();
            byte[] symbolBytes = _reader.ReadBytes(length);

            if (symbolBytes.Length != length)
            {
                throw new CodecException("End of stream while reading an atom term.");
            }

            string symbol = Encoding.ASCII.GetString(symbolBytes);

            return Atom.From(symbol);
        }

        string ReadString()
        {
            ushort length = ReadUnsignedShort();
            byte[] stringBytes = _reader.ReadBytes(length);

            if (stringBytes.Length != length)
            {
                throw new CodecException("End of stream while reading a string term.");
            }

            return Encoding.ASCII.GetString(stringBytes);
        }

        Tuple ReadTupleData(int arity)
        {
            object[] elements = new object[arity];

            for (int i = 0; i < arity; i++)
            {
                elements[i] = ReadObject();
            }

            return new Tuple(elements);
        }
    }
}
