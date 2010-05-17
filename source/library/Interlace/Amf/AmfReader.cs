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
using System.Text;
using System.Xml;

#endregion

namespace Interlace.Amf
{
    public class AmfReader : IDisposable
    {
        BinaryReader _reader;
        AmfRegistry _registry;

        List<string> _stringTable;
        List<object> _objectTable;
        List<AmfTraits> _traitsTable;

        static readonly DateTime _amfEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public AmfReader(BinaryReader reader, AmfRegistry registry)
        {
            _reader = reader;
            _registry = registry;

            _stringTable = new List<string>();
            _objectTable = new List<object>();
            _traitsTable = new List<AmfTraits>();
        }

        public static object Read(AmfRegistry registry, byte[] encodedBytes)
        {
            using (MemoryStream stream = new MemoryStream(encodedBytes))
            {
                using (AmfReader reader = new AmfReader(new BinaryReader(stream), registry))
                {
                    return reader.Read();
                }
            }
        }

        void ResetTables()
        {
            _stringTable.Clear();
            _objectTable.Clear();
            _traitsTable.Clear();
        }

        double ReadNetworkDouble()
        {
            return BitConverter.Int64BitsToDouble(IPAddress.NetworkToHostOrder(_reader.ReadInt64()));
        }

        int ReadPackedInteger()
        {
            int accumulator = 0;

            byte first = _reader.ReadByte();
            accumulator = 0x7f & first;
            if ((first & 0x80) == 0) return accumulator;

            byte second = _reader.ReadByte();
            accumulator = (accumulator << 7) | (0x7f & second);
            if ((second & 0x80) == 0) return accumulator;

            byte third = _reader.ReadByte();
            accumulator = (accumulator << 7) | (0x7f & third);
            if ((third & 0x80) == 0) return accumulator;

            byte fourth = _reader.ReadByte();
            accumulator = (accumulator << 8) | fourth;
            return accumulator;
        }

        bool ReadFlaggedInteger(out int value)
        {
            int read = ReadPackedInteger();

            value = read >> 1;

            return (read & 1) == 1;
        }

        bool ReadFlaggedInteger()
        {
            int read = ReadPackedInteger();

            return (read & 1) == 1;

        }

        public object Read()
        {
            byte marker = _reader.ReadByte();

            switch (marker)
            {
                case AmfMarker.Undefined:
                    throw new NotImplementedException();

                case AmfMarker.Null:
                    return null;

                case AmfMarker.False:
                    return false;

                case AmfMarker.True:
                    return true;

                case AmfMarker.Integer:
                    return ReadPackedInteger();

                case AmfMarker.Double:
                    return ReadNetworkDouble();

                case AmfMarker.String:
                    return ReadString();

                case AmfMarker.XmlDoc:
                    throw new NotImplementedException();

                case AmfMarker.Date:
                    return ReadDate();

                case AmfMarker.Array:
                    return ReadArray();

                case AmfMarker.Object:
                    return ReadObject();

                case AmfMarker.Xml:
                    return ReadXml();

                case AmfMarker.ByteArray:
                    return ReadByteArray();

                default:
                    throw new NotImplementedException();
            }
        }

        string ReadString()
        {
            int argument;

            if (ReadFlaggedInteger(out argument))
            {
                byte[] data = _reader.ReadBytes(argument);

                if (data.Length < argument) throw new EndOfStreamException();

                string value = Encoding.UTF8.GetString(data);

                if (value != "") _stringTable.Add(value);

                return value;
            }
            else
            {
                return _stringTable[argument];
            }
        }

        DateTime ReadDate()
        {
            int argument;

            if (ReadFlaggedInteger(out argument))
            {
                double millisecondsSinceEpoch = ReadNetworkDouble();

                DateTime dateTime = _amfEpoch + TimeSpan.FromMilliseconds(millisecondsSinceEpoch);

                _objectTable.Add(dateTime);

                return dateTime;
            }
            else
            {
                return (DateTime)_objectTable[argument];
            }
        }

        Dictionary<string, object> ReadPropertyList()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            while (true)
            {
                string key = ReadString();

                if (key == "") break;

                object value = Read();

                dictionary[key] = value;
            }

            return dictionary;
        }

        AmfArray ReadArray()
        {
            int argument;

            if (ReadFlaggedInteger(out argument))
            {
                AmfArray array = new AmfArray();
                _objectTable.Add(array);

                Dictionary<string, object> associativeElements = ReadPropertyList();

                List<object> denseElements = new List<object>();

                for (int i = 0; i < argument; i++)
                {
                    object obj = Read();

                    denseElements.Add(obj);
                }

                foreach (KeyValuePair<string, object> pair in associativeElements)
                {
                    array.AssociativeElements.Add(pair);
                }

                foreach (object element in denseElements)
                {
                    array.DenseElements.Add(element);
                }

                return array;
            }
            else
            {
                return (AmfArray)_objectTable[argument];
            }
        }

        AmfTraits ReadObjectTraits(int argument)
        {
            bool isNonReferenceTrait = (argument & 1) == 1;

            int remainingArgument = argument >> 1;

            if (isNonReferenceTrait)
            {
                string className = ReadString();

                bool isExternalizable = (remainingArgument & 1) == 1;

                int traitArgument = remainingArgument >> 1;

                AmfTraits traits;

                if (isExternalizable)
                {
                    traits = new AmfTraits(className, AmfTraitsKind.Externalizable, new string[] { });
                }
                else
                {
                    bool isDynamic = (traitArgument & 1) == 1;
                    int memberCount = traitArgument >> 1;

                    string[] memberNames = new string[memberCount];

                    for (int i = 0; i < memberCount; i++)
                    {
                        memberNames[i] = ReadString();
                    }

                    traits = new AmfTraits(className, isDynamic ? AmfTraitsKind.Dynamic : AmfTraitsKind.Static, memberNames);
                }

                _traitsTable.Add(traits);

                return traits;
            }
            else
            {
                return _traitsTable[remainingArgument];
            }
        }

        object ReadObject()
        {
            int argument;

            if (ReadFlaggedInteger(out argument))
            {
                AmfTraits traits = ReadObjectTraits(argument);

                if (traits.Kind == AmfTraitsKind.Externalizable)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    IAmfClassDescriptor descriptor = _registry.GetByAlias(traits.ClassName);

                    object newObject = descriptor.BeginDeserialization(traits);

                    _objectTable.Add(newObject);

                    Dictionary<string, object> staticMembers = new Dictionary<string, object>();

                    foreach (string memberName in traits.MemberNames)
                    {
                        object value = Read();

                        staticMembers[memberName] = value;
                    }

                    Dictionary<string, object> dynamicMembers = null;

                    if (traits.Kind == AmfTraitsKind.Dynamic)
                    {
                        dynamicMembers = new Dictionary<string, object>();

                        while (true)
                        {
                            string key = ReadString();

                            if (key == "") break;

                            object value = Read();

                            dynamicMembers[key] = value;
                        }
                    }

                    descriptor.EndDeserialization(traits, newObject, staticMembers, dynamicMembers);

                    return newObject;
                }
            }
            else
            {
                return _objectTable[argument];
            }
        }

        XmlDocument ReadXml()
        {
            int argument;

            if (ReadFlaggedInteger(out argument))
            {
                byte[] data = _reader.ReadBytes(argument);

                if (data.Length < argument) throw new EndOfStreamException();

                string value = Encoding.UTF8.GetString(data);

                XmlDocument document = new XmlDocument();
                document.XmlResolver = null;

                document.LoadXml(value);

                _objectTable.Add(document);

                return document;
            }
            else
            {
                return (XmlDocument)_objectTable[argument];
            }
        }

        byte[] ReadByteArray()
        {
            int argument;

            if (ReadFlaggedInteger(out argument))
            {
                byte[] data = _reader.ReadBytes(argument);

                if (data.Length < argument) throw new EndOfStreamException();

                _objectTable.Add(data);

                return data;
            }
            else
            {
                return (byte[])_objectTable[argument];
            }
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }
    }
}
