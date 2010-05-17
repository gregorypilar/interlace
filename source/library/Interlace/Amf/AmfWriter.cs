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
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

#endregion

namespace Interlace.Amf
{
    public class AmfWriter : IDisposable
    {
        BinaryWriter _writer;
        AmfRegistry _registry;

        Dictionary<string, int> _stringTable;

        // The object and date time table are actually the same table, but couldn't be stored
        // in the same dictionary (values are boxed and never compare reference-equal):
        ObjectIDGenerator _idGenerator;
        Dictionary<DateTime, int> _dateTimeTable;
        Dictionary<long, int> _objectTable;

        Dictionary<long, int> _traitsTable;

        static Dictionary<Type, int> _typeMap;

        static AmfWriter()
        {
            _typeMap = new Dictionary<Type, int>();

            _typeMap[typeof(bool)] = AmfMarker.True;
            _typeMap[typeof(int)] = AmfMarker.Integer;
            _typeMap[typeof(double)] = AmfMarker.Double;
            _typeMap[typeof(string)] = AmfMarker.String;
            _typeMap[typeof(DateTime)] = AmfMarker.Date;
            _typeMap[typeof(AmfArray)] = AmfMarker.Array;
            _typeMap[typeof(XmlDocument)] = AmfMarker.Xml;
            _typeMap[typeof(byte[])] = AmfMarker.ByteArray;
        }

        static readonly DateTime _amfEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public AmfWriter(BinaryWriter writer, AmfRegistry registry)
        {
            _writer = writer;
            _registry = registry;

            _stringTable = new Dictionary<string, int>();

            _idGenerator = new ObjectIDGenerator();
            _dateTimeTable = new Dictionary<DateTime, int>();
            _objectTable = new Dictionary<long, int>();

            _traitsTable = new Dictionary<long, int>();
        }

        public static byte[] Write(AmfRegistry registry, object value)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (AmfWriter writer = new AmfWriter(new BinaryWriter(stream), registry))
                {
                    writer.Write(value);

                    return stream.ToArray();
                }
            }
        }

        void WritePackedInteger(int value)
        {
            if (value < 0) throw new AmfException("An attempt was made to serialize a negative integer.");

            if (value < 0x80)
            {
                _writer.Write((byte)value);
            }
            else if (value < 0x4000)
            {
                _writer.Write((byte)(0x80 | (value >> 7) & 0x7f));
                _writer.Write((byte)(value & 0x7f));
            }
            else if (value < 0x200000)
            {
                _writer.Write((byte)(0x80 | (value >> 14) & 0x7f));
                _writer.Write((byte)(0x80 | (value >> 7) & 0x7f));
                _writer.Write((byte)(value & 0x7f));
            }
            else if (value < 0x40000000)
            {
                _writer.Write((byte)(0x80 | (value >> 22) & 0x7f));
                _writer.Write((byte)(0x80 | (value >> 15) & 0x7f));
                _writer.Write((byte)(0x80 | (value >> 8) & 0x7f));
                _writer.Write((byte)(value & 0xff));
            }
            else
            {
                throw new AmfException("An integer too large to serialize was serialized.");
            }
        }

        void WriteFlaggedInteger(int value, bool flag)
        {
            WritePackedInteger((value << 1) | (flag ? 1 : 0));
        }

        void WriteNetworkDouble(double value)
        {
            _writer.Write((long)IPAddress.HostToNetworkOrder(BitConverter.DoubleToInt64Bits(value)));
        }

        void WriteNull()
        {
            _writer.Write((byte)AmfMarker.Null);
        }

        void Write(bool value)
        {
            if (value)
            {
                _writer.Write((byte)AmfMarker.True);
            }
            else
            {
                _writer.Write((byte)AmfMarker.False);
            }
        }

        void Write(int value)
        {
            _writer.Write((byte)AmfMarker.Integer);
            WritePackedInteger(value);
        }

        void Write(double value)
        {
            _writer.Write((byte)AmfMarker.Double);
            WriteNetworkDouble(value);
        }

        void WriteBareString(string value)
        {
            if (_stringTable.ContainsKey(value))
            {
                int index = _stringTable[value];

                WriteFlaggedInteger(index, false);
            }
            else
            {
                if (value != "") _stringTable[value] = _stringTable.Count;

                byte[] data = Encoding.UTF8.GetBytes(value);

                WriteFlaggedInteger(data.Length, true);

                _writer.Write(data);
            }
        }

        void Write(string value)
        {
            _writer.Write((byte)AmfMarker.String);

            WriteBareString(value);
        }

        void Write(DateTime value)
        {
            _writer.Write((byte)AmfMarker.Date);

            if (_dateTimeTable.ContainsKey(value))
            {
                int index = _dateTimeTable[value];

                WriteFlaggedInteger(index, false);
            }
            else
            {
                _dateTimeTable[value] = _dateTimeTable.Count + _objectTable.Count;

                double doubleValue = (value - _amfEpoch).TotalMilliseconds;

                WriteFlaggedInteger(0, true);
                WriteNetworkDouble(doubleValue);
            }
        }

        void Write(AmfArray value)
        {
            _writer.Write((byte)AmfMarker.Array);

            bool firstTime;

            long uniqueValue = _idGenerator.GetId(value, out firstTime);

            if (!firstTime)
            {
                WriteFlaggedInteger(_objectTable[uniqueValue], false);
            }
            else
            {
                _objectTable[uniqueValue] = _objectTable.Count + _dateTimeTable.Count;

                WriteFlaggedInteger(value.DenseElements.Count, true);

                foreach (KeyValuePair<string, object> pair in value.AssociativeElements)
                {
                    WriteBareString(pair.Key);
                    Write(pair.Value);
                }

                WriteBareString("");

                foreach (object denseElement in value.DenseElements)
                {
                    Write(denseElement);
                }
            }
        }

        void Write(byte[] value)
        {
            _writer.Write((byte)AmfMarker.ByteArray);

            bool firstTime;

            long uniqueValue = _idGenerator.GetId(value, out firstTime);

            if (!firstTime)
            {
                WriteFlaggedInteger(_objectTable[uniqueValue], false);
            }
            else
            {
                _objectTable[uniqueValue] = _objectTable.Count + _dateTimeTable.Count;

                WriteFlaggedInteger(value.Length, true);

                _writer.Write(value);
            }
        }

        void WriteObjectTraits(AmfTraits traits)
        {
            bool firstTime;

            long uniqueValue = _idGenerator.GetId(traits, out firstTime);

            if (!firstTime)
            {
                int argument = (_traitsTable[uniqueValue] << 2) | 0x01;
                WritePackedInteger(argument);
            }
            else
            {
                _traitsTable[uniqueValue] = _traitsTable.Count;

                if (traits.Kind == AmfTraitsKind.Externalizable) throw new NotImplementedException();

                int argument = (traits.MemberNames.Length << 4) | 0x03;

                if (traits.Kind == AmfTraitsKind.Dynamic) argument |= 0x08;

                WritePackedInteger(argument);
                WriteBareString(traits.ClassName);

                foreach (string memberName in traits.MemberNames)
                {
                    WriteBareString(memberName);
                }
            }
        }

        void WriteObject(Type valueType, object value)
        {
            _writer.Write((byte)AmfMarker.Object);

            bool firstTime;

            long uniqueValue = _idGenerator.GetId(value, out firstTime);

            if (!firstTime)
            {
                WriteFlaggedInteger(_objectTable[uniqueValue], false);
            }
            else
            {
                _objectTable[uniqueValue] = _objectTable.Count + _dateTimeTable.Count;

                IAmfClassDescriptor descriptor = _registry.GetByType(valueType);

                AmfTraits traits;
                IDictionary<string, object> staticMembers;
                IDictionary<string, object> dynamicMembers;

                descriptor.SerializeObject(value, out traits, out staticMembers, out dynamicMembers);

                WriteObjectTraits(traits);

                foreach (string memberName in traits.MemberNames)
                {
                    Write(staticMembers[memberName]);
                }

                if (traits.Kind == AmfTraitsKind.Dynamic)
                {
                    foreach (KeyValuePair<string, object> pair in dynamicMembers)
                    {
                        WriteBareString(pair.Key);
                        Write(pair.Value);
                    }

                    WriteBareString("");
                }
            }
        }

        public const byte Object = 0x0a;

        public void Write(object value)
        {
            if (value == null)
            {
                WriteNull();
            } 
            else 
            {
                Type type = value.GetType();

                int simpleKind;

                if (_typeMap.TryGetValue(type, out simpleKind))
                {
                    switch (simpleKind)
                    {
                        case AmfMarker.True:
                            Write((bool)value);
                            break;

                        case AmfMarker.Integer:
                            Write((int)value);
                            break;

                        case AmfMarker.Double:
                            Write((double)value);
                            break;

                        case AmfMarker.String:
                            Write((string)value);
                            break;

                        case AmfMarker.Date:
                            Write((DateTime)value);
                            break;

                        case AmfMarker.Array:
                            Write((AmfArray)value);
                            break;

                        case AmfMarker.ByteArray:
                            Write((byte[])value);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    WriteObject(type, value);
                }
            }
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer = null;
            }
        }
    }
}
