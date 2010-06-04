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

using Interlace.Pinch.Implementation;
using Interlace.Pinch.Tests;

#endregion

namespace Interlace.Pinch.Tests
{
    [TestFixture]
    public class EncodingTest
    {
        void AssertTypesStructuresAreEqual(TypesStructure a, TypesStructure b)
        {
    	    Assert.AreEqual(a.ReqFloat32, b.ReqFloat32);
    	    Assert.AreEqual(a.ReqFloat64, b.ReqFloat64);
            Assert.AreEqual(a.ReqInt8, b.ReqInt8);
            Assert.AreEqual(a.ReqInt16, b.ReqInt16);
            Assert.AreEqual(a.ReqInt32, b.ReqInt32);
            Assert.AreEqual(a.ReqInt64, b.ReqInt64);
            Assert.AreEqual(a.ReqBool, b.ReqBool);
            Assert.AreEqual(a.ReqString, b.ReqString);
            ArrayAssert.AreEqual(a.ReqBytes, b.ReqBytes);
            Assert.AreEqual(a.ReqEnumeration, b.ReqEnumeration);
            Assert.AreEqual(a.ReqStructure, b.ReqStructure);
            CollectionAssert.AreEqual(a.ReqListOfEnum, b.ReqListOfEnum);
    	    Assert.AreEqual(a.OptFloat32, b.OptFloat32);
    	    Assert.AreEqual(a.OptFloat64, b.OptFloat64);
            Assert.AreEqual(a.OptInt8, b.OptInt8);
            Assert.AreEqual(a.OptInt16, b.OptInt16);
            Assert.AreEqual(a.OptInt32, b.OptInt32);
            Assert.AreEqual(a.OptInt64, b.OptInt64);
            Assert.AreEqual(a.OptBool, b.OptBool);
            Assert.AreEqual(a.OptString, b.OptString);
            CollectionAssert.AreEqual(a.OptBytes, b.OptBytes);
            Assert.AreEqual(a.OptStructure, b.OptStructure);
            CollectionAssert.AreEqual(a.OptListOfEnum, b.OptListOfEnum);
        }

        [Test]
        public void TestSimple()
        {
            TypesStructure before = new TypesStructure();

    	    before.ReqFloat32 = 1.1f;
    	    before.ReqFloat64 = 1.1;
            before.ReqInt8 = 12;
            before.ReqInt16 = -1;
            before.ReqInt32 = 0x7fffffff;
            before.ReqInt64 = -1L;
            before.ReqDecimal = 1M;
            before.ReqBool = true;
            before.ReqString = "The quick brown fox.";
            before.ReqBytes = new byte[] { 1, 2, 3 };
            before.ReqEnumeration = TypesEnumeration.B;
            before.ReqStructure = new SmallStructure();
            before.ReqStructure.Test = 2;
            before.ReqListOfEnum.Add(new SmallStructure());
            
    	    before.OptFloat32 = null;
    	    before.OptFloat64 = null;
            before.OptInt8 = null;
            before.OptInt16 = null;
            before.OptInt32 = null;
            before.OptInt64 = null;
            before.OptDecimal = null;
            before.OptBool = null;
            before.OptString = null;
            before.OptBytes = null;
            before.OptEnumeration = null;
            before.OptStructure = null;
            before.OptListOfEnum.Add(null);

            byte[] result = Pincher.Encode(before);

            using (FileStream stream = new FileStream(@"C:\pinchtest.pinch", FileMode.Create))
            {
                Pincher.Encode(before, stream);
            }

            TypesStructure after = Pincher.Decode<TypesStructure>(result);

            AssertTypesStructuresAreEqual(before, after);
        }

        [Test]
        public void TestOptionals()
        {
            TypesStructure before = new TypesStructure();

    	    before.ReqFloat32 = -1.1f;
    	    before.ReqFloat64 = -1.1;
            before.ReqInt8 = 12;
            before.ReqInt16 = 0x7fff;
            before.ReqInt32 = -1;
            before.ReqInt64 = 0x7fffffffffffffffL;
            before.ReqDecimal = decimal.MaxValue;
            before.ReqBool = false;
            before.ReqString = "";
            before.ReqBytes = new byte[] { };
            before.ReqEnumeration = TypesEnumeration.A;
            before.ReqStructure = new SmallStructure();
            before.ReqStructure.Test = 0;
            before.ReqListOfEnum.Add(new SmallStructure());
            
    	    before.OptFloat32 = 1.234f;
    	    before.OptFloat64 = 0.001234;
            before.OptInt8 = 123;
            before.OptInt16 = -1;
            before.OptInt32 = -1;
            before.OptInt64 = -1;
            before.OptDecimal = decimal.MaxValue;
            before.OptBool = true;
            before.OptString = "Foo";
            before.OptBytes = new byte[] { 1 };
            before.OptEnumeration = TypesEnumeration.C;
            before.OptStructure = new SmallStructure();
            before.OptStructure.Test = 3;
            before.OptListOfEnum.Add(null);
            before.OptListOfEnum.Add(new SmallStructure());

            byte[] result = Pincher.Encode(before);

            TypesStructure after = Pincher.Decode<TypesStructure>(result);

            AssertTypesStructuresAreEqual(before, after);
        }

        [Test]
        public void TestChoice()
        {
            ChoiceMessage before = new ChoiceMessage();
            before.Choice = new SmallStructure();
            before.Choice.Small.Test = 123;

            byte[] result = Pincher.Encode(before);

            ChoiceMessage after = Pincher.Decode<ChoiceMessage>(result);

            Assert.AreEqual(ChoiceStructureKind.Small, after.Choice.ValueKind);
            Assert.AreEqual(123, after.Choice.Small.Test);

            Assert.IsNull(before.Choice.OptionalDecimal);
            Assert.IsNull(before.Choice.RequiredDecimal);
            Assert.IsNull(before.Choice.Versioning);
        }

        [Test]
        public void TestOtherChoice()
        {
            ChoiceMessage before = new ChoiceMessage();
            before.Choice = new OptionalDecimalStructure();
            before.Choice.OptionalDecimal.Value = 42;

            byte[] result = Pincher.Encode(before);

            ChoiceMessage after = Pincher.Decode<ChoiceMessage>(result);

            Assert.AreEqual(ChoiceStructureKind.OptionalDecimal, after.Choice.ValueKind);
            Assert.AreEqual(42, after.Choice.OptionalDecimal.Value);

            Assert.IsNull(before.Choice.Small);
            Assert.IsNull(before.Choice.RequiredDecimal);
            Assert.IsNull(before.Choice.Versioning);
        }
    }
}
