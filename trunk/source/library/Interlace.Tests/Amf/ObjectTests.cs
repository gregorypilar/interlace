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
    public class ObjectTests
    {
        [AmfClass("bitplantation.EmptyTest")]
        class BitPlantationEmptyTest
        {
            int _b;

            [AmfProperty("_b")]
            public int B
            { 	 
               get { return _b; }
               set { _b = value; }
            }
        }

        [AmfClass("bitplantation.Test")]
        class BitPlantationTest
        {
            string _a;
            int _b;

            [AmfProperty]
            public string A
            { 	 
                get { return _a; }
                set { _a = value; }
            }

            [AmfProperty("_b")]
            public int B
            { 	 
                get { return _b; }
                set { _b = value; }
            }
        }

        [AmfClass("bitplantation.DynTest")]
        class BitPlantationDynTest : AmfObject
        {
            string _a;
            int _b;

            [AmfProperty]
            public string A
            { 	 
                get { return _a; }
                set { _a = value; }
            }

            [AmfProperty("_b")]
            public int B
            { 	 
                get { return _b; }
                set { _b = value; }
            }
        }

        public object HelperDeserialize(byte[] data)
        {
            AmfRegistry registry = new AmfRegistry();

            registry.RegisterClassAlias(typeof(BitPlantationEmptyTest));
            registry.RegisterClassAlias(typeof(BitPlantationTest));
            registry.RegisterClassAlias(typeof(BitPlantationDynTest));

            return AmfReader.Read(registry, data);
        }

        [Test]
        public void TestObjects()
        {
            BitPlantationEmptyTest emptyTest = HelperDeserialize(
    			new byte[] {0x0a, 0x13, 0x2f, 0x62, 0x69, 0x74, 0x70, 0x6c, 
    			  0x61, 0x6e, 0x74, 0x61, 0x74, 0x69, 0x6f, 0x6e, 
    			  0x2e, 0x45, 0x6d, 0x70, 0x74, 0x79, 0x54, 0x65, 
    			  0x73, 0x74, 0x05, 0x5f, 0x62, 0x04, 0x02}) as BitPlantationEmptyTest;

            Assert.AreEqual(emptyTest.B, 2);

            BitPlantationTest test = HelperDeserialize(
    			new byte[] {0x0a, 0x23, 0x25, 0x62, 0x69, 0x74, 0x70, 0x6c, 
    			  0x61, 0x6e, 0x74, 0x61, 0x74, 0x69, 0x6f, 0x6e, 
    			  0x2e, 0x54, 0x65, 0x73, 0x74, 0x03, 0x41, 0x05, 
    			  0x5f, 0x62, 0x06, 0x07, 0x63, 0x61, 0x74, 0x04, 
    			  0x02}) as BitPlantationTest;

            Assert.AreEqual(test.A, "cat");
            Assert.AreEqual(test.B, 2);

            BitPlantationDynTest dynTest = HelperDeserialize(
                new byte[] {0x0a, 0x2b, 0x2b, 0x62, 0x69, 0x74, 0x70, 0x6c, 
    			  0x61, 0x6e, 0x74, 0x61, 0x74, 0x69, 0x6f, 0x6e, 
    			  0x2e, 0x44, 0x79, 0x6e, 0x54, 0x65, 0x73, 0x74, 
    			  0x03, 0x41, 0x05, 0x5f, 0x62, 0x06, 0x07, 0x63, 
    			  0x61, 0x74, 0x04, 0x02, 0x1d, 0x4f, 0x74, 0x68, 
    			  0x65, 0x72, 0x41, 0x74, 0x74, 0x72, 0x69, 0x62, 
    			  0x75, 0x74, 0x65, 0x04, 0x01, 0x21, 0x44, 0x79, 
    			  0x6e, 0x61, 0x6d, 0x69, 0x63, 0x41, 0x74, 0x74, 
    			  0x72, 0x69, 0x62, 0x75, 0x74, 0x65, 0x06, 0x09, 
    			  0x74, 0x65, 0x73, 0x74, 0x01}) as BitPlantationDynTest;

            Assert.AreEqual(dynTest.A, "cat");
            Assert.AreEqual(dynTest.B, 2);
            Assert.AreEqual(dynTest.Properties["DynamicAttribute"], "test");
            Assert.AreEqual(dynTest.Properties["OtherAttribute"], 1);
        }

        T RoundTrip<T>(T obj)
        {
            AmfRegistry registry = new AmfRegistry();

            registry.RegisterClassAlias(typeof(BitPlantationEmptyTest));
            registry.RegisterClassAlias(typeof(BitPlantationTest));
            registry.RegisterClassAlias(typeof(BitPlantationDynTest));

            byte[] data = AmfWriter.Write(registry, obj);
            return (T)AmfReader.Read(registry, data);
        }

        [Test]
        public void TestRoundTripping()
        {
            // Test an empty class:
            BitPlantationEmptyTest beforeEmpty = new BitPlantationEmptyTest();
            beforeEmpty.B = 2;

            BitPlantationEmptyTest afterEmpty = RoundTrip(beforeEmpty);

            Assert.AreEqual(afterEmpty.B, 2);

            // Test a static class:
            BitPlantationTest before = new BitPlantationTest();

            before.A = "cat";
            before.B = 2;

            BitPlantationTest after = RoundTrip(before);

            // Test a dynamic class:
            BitPlantationDynTest beforeDyn = new BitPlantationDynTest();
            beforeDyn.A = "cat";
            beforeDyn.B = 2;
            beforeDyn.Properties["DynamicAttribute"] = "test";
            beforeDyn.Properties["OtherAttribute"] = 1;

            BitPlantationDynTest afterDyn = RoundTrip(beforeDyn);

            Assert.AreEqual(afterDyn.A, "cat");
            Assert.AreEqual(afterDyn.B, 2);
            Assert.AreEqual(afterDyn.Properties["DynamicAttribute"], "test");
            Assert.AreEqual(afterDyn.Properties["OtherAttribute"], 1);
        }
    }

    [AmfClass("bitplantation.SinglePropertyTest")]
    public class BitPlantationSinglePropertyTest
    {
        string _a;

        public BitPlantationSinglePropertyTest()
        {
        }

        public BitPlantationSinglePropertyTest(string a)
        {
            _a = a;
        }

        [AmfProperty]
        public string A
        { 	 
           get { return _a; }
           set { _a = value; }
        }

        public override bool Equals(object obj)
        {
            BitPlantationSinglePropertyTest rhs = obj as BitPlantationSinglePropertyTest;

            if (rhs == null) return false;

            return object.Equals(_a, rhs._a);
        }

        public override int GetHashCode()
        {
            if (_a == null) return 0;

            return _a.GetHashCode();
        }
    }

}
