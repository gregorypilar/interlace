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
using System.Text;

using MbUnit.Framework;

using Interlace.Amf;

#endregion

namespace Interlace.Tests.Amf
{
    [TestFixture]
    public class CodecTests
    {
        public static void AssertListsAreEqual<T>(IList<T> lhs, IList<T> rhs)
        {
            Assert.AreEqual(lhs.Count, rhs.Count);

            for (int i = 0; i < lhs.Count; i++)
            {
                AssertEquality(lhs[i], rhs[i]);
            }
        }

        public static void AssertDictionariesAreEqual<K, V>(IDictionary<K, V> lhs, IDictionary<K, V> rhs)
        {
            Assert.AreEqual(lhs.Count, rhs.Count);

            foreach (KeyValuePair<K, V> pair in lhs)
            {
                Assert.IsTrue(rhs.ContainsKey(pair.Key));

                Assert.AreEqual(pair.Value, rhs[pair.Key]);
            }
        }

        public static void AssertEquality(object expectedValue, object actualValue)
        {
            if (expectedValue is byte[])
            {
                ArrayAssert.AreEqual(expectedValue as byte[], actualValue as byte[]);
            }
            else if (expectedValue is AmfArray)
            {
                AmfArray lhs = expectedValue as AmfArray;
                AmfArray rhs = actualValue as AmfArray;

                AssertListsAreEqual(lhs.DenseElements, rhs.DenseElements);
                AssertDictionariesAreEqual(lhs.AssociativeElements, rhs.AssociativeElements);
            }
            else if (expectedValue != null && expectedValue.GetType().Equals(typeof(AmfObject)))
            {
                AmfObject lhs = expectedValue as AmfObject;
                AmfObject rhs = actualValue as AmfObject;

                AssertDictionariesAreEqual(lhs.Properties, rhs.Properties);
            }
            else
            {
                Assert.AreEqual(expectedValue, actualValue);
            }
        }

        public static void AssertRoundTrip(object expectedValue, byte[] encodedBytes)
        {
            AmfRegistry registry = new AmfRegistry();

            registry.RegisterClassAlias(typeof(BitPlantationSinglePropertyTest));

            object actualValue = AmfReader.Read(registry, encodedBytes);

            AssertEquality(actualValue, expectedValue);

            // Test the other way:
            byte[] firstTrip = AmfWriter.Write(registry, expectedValue);
            object secondTrip = AmfReader.Read(registry, firstTrip);

            AssertEquality(expectedValue, secondTrip);
        }

        [Test]
        public void Test()
        {
            AssertRoundTrip(null, new byte[] {1});
            AssertRoundTrip(false, new byte[] {2});
            AssertRoundTrip(true, new byte[] {3});

            AssertRoundTrip(0, new byte[] {4, 0x00});
            AssertRoundTrip(1, new byte[] {4, 0x01});
            AssertRoundTrip(0x7f, new byte[] {4, 0x7f});
            AssertRoundTrip(0x80, new byte[] {4, 0x81, 0x00});
            AssertRoundTrip(0x3fff, new byte[] {4, 0xff, 0x7f});
            AssertRoundTrip(0x4000, new byte[] {4, 0x81, 0x80, 0x00});
            AssertRoundTrip(0x001fffff, new byte[] {4, 0xff, 0xff, 0x7f});

            AssertRoundTrip(0.5, new byte[] {0x05, 0x3f, 0xe0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
            AssertRoundTrip(2535301200456458802993406410752.0, new byte[] {0x05, 0x46, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});

            AssertRoundTrip("", new byte[] {0x06, 0x01});
            AssertRoundTrip("Hello", new byte[] {0x06, 0x0b, 0x48, 0x65, 0x6c, 0x6c, 0x6f});

            AssertRoundTrip(new DateTime(1970, 1, 1, 0, 0, 0), new byte[] {0x08, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
            AssertRoundTrip(new DateTime(2001, 10, 10, 21, 22, 0), new byte[] {0x08, 0x01, 0x42, 0x6d, 0x2f, 0x0f, 0xc8, 0x18, 0x00, 0x00});

            AssertRoundTrip(new AmfArray(), new byte[] {0x09, 0x01, 0x01});

            AssertRoundTrip(AmfArray.Dense(1, "test"), new byte[] {0x09, 0x05, 0x01, 0x04, 0x01, 0x06, 0x09, 0x74, 0x65, 0x73, 0x74});

            AmfArray array = AmfArray.Dense(1, 2);
            array["test"] = "case";
            array[6] = "six";

            AssertRoundTrip(array, 
            	new byte[] {0x09, 0x05, 0x03, 0x36, 0x06, 0x07, 0x73, 0x69, 
            	  0x78, 0x09, 0x74, 0x65, 0x73, 0x74, 0x06, 0x09, 
            	  0x63, 0x61, 0x73, 0x65, 0x01, 0x04, 0x01, 0x04, 
            	  0x02});

            AssertRoundTrip(new byte[] {}, new byte[] {0x0c, 0x01});
            AssertRoundTrip(new byte[] {0x00, 0x80, 0xff}, new byte[] {0x0c, 0x07, 0x00, 0x80, 0xff});
        }

        [Test]
        public void TestDynamicObjects()
        {
            AssertRoundTrip(new AmfObject(), 
        		new byte[] {0x0a, 0x0b, 0x01, 0x01});

            AmfObject attributedObject = new AmfObject();
            attributedObject.Properties["DynamicAttribute"] = "test";
		    attributedObject.Properties["OtherAttribute"] = 1;

            AssertRoundTrip(attributedObject, 
    			new byte[] {0x0a, 0x0b, 0x01, 0x1d, 0x4f, 0x74, 0x68, 0x65, 
    			  0x72, 0x41, 0x74, 0x74, 0x72, 0x69, 0x62, 0x75, 
    			  0x74, 0x65, 0x04, 0x01, 0x21, 0x44, 0x79, 0x6e, 
    			  0x61, 0x6d, 0x69, 0x63, 0x41, 0x74, 0x74, 0x72, 
    			  0x69, 0x62, 0x75, 0x74, 0x65, 0x06, 0x09, 0x74, 
    			  0x65, 0x73, 0x74, 0x01});
        }
    }
}
