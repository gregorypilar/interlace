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
using System.Text;

using MbUnit.Framework;

using Interlace.Amf;

#endregion

namespace Interlace.Tests.Amf
{
    [TestFixture]
    class TableTests
    {
        [Test]
        public void TestStringTables()
        {
            string quickBrown = "The quick brown fox jumped over the lazy dog.";

            AmfArray stringArray = AmfArray.Dense("", quickBrown, "", quickBrown, "Literal", "Test", "Test");

            CodecTests.AssertRoundTrip(stringArray, new byte[] {
                0x09, 0x0f, 0x01, 0x06, 0x01, 0x06, 0x5b, 0x54, 
                0x68, 0x65, 0x20, 0x71, 0x75, 0x69, 0x63, 0x6b, 
                0x20, 0x62, 0x72, 0x6f, 0x77, 0x6e, 0x20, 0x66, 
                0x6f, 0x78, 0x20, 0x6a, 0x75, 0x6d, 0x70, 0x65, 
                0x64, 0x20, 0x6f, 0x76, 0x65, 0x72, 0x20, 0x74, 
                0x68, 0x65, 0x20, 0x6c, 0x61, 0x7a, 0x79, 0x20, 
                0x64, 0x6f, 0x67, 0x2e, 0x06, 0x01, 0x06, 0x00, 
                0x06, 0x0f, 0x4c, 0x69, 0x74, 0x65, 0x72, 0x61, 
                0x6c, 0x06, 0x09, 0x54, 0x65, 0x73, 0x74, 0x06, 
                0x04 });
        }

        [Test]
        public void TestArrayTable()
        {
            AmfArray arrayArray = AmfReader.Read(new AmfRegistry(), new byte[] {
                0x09, 0x09, 0x01, 0x09, 0x01, 0x01, 0x09, 0x03, 0x01, 0x04, 0x042, 0x09, 0x01, 0x01, 0x09, 0x04}) as AmfArray;

            Assert.AreEqual(arrayArray.DenseElements.Count, 4);
            Assert.AreEqual((arrayArray.DenseElements[0] as AmfArray).DenseElements.Count, 0);
            Assert.AreEqual((arrayArray.DenseElements[1] as AmfArray).DenseElements.Count, 1);
            Assert.AreEqual((arrayArray.DenseElements[1] as AmfArray).DenseElements[0], 0x42);
            Assert.AreEqual((arrayArray.DenseElements[2] as AmfArray).DenseElements.Count, 0);
            Assert.AreEqual((arrayArray.DenseElements[3] as AmfArray).DenseElements.Count, 1);
            Assert.AreEqual((arrayArray.DenseElements[3] as AmfArray).DenseElements[0], 0x42);
        }

        [Test]
        public void TestDateArray()
        {
            AmfArray dateArray = AmfArray.Dense(
                new DateTime(2001, 2, 1, 0, 20, 1, DateTimeKind.Utc),
                new DateTime(2008, 2, 1, 0, 20, 1, DateTimeKind.Utc),
                new DateTime(2001, 2, 1, 0, 20, 1, DateTimeKind.Utc),
                new DateTime(2008, 2, 1, 0, 20, 1, DateTimeKind.Utc)
                );

            CodecTests.AssertRoundTrip(dateArray, new byte[] {
                0x09, 0x09, 0x01, 0x08, 0x01, 0x42, 0x6c, 0x8c, 0xeb, 0xd5, 
                0x6d, 0x00, 0x00, 0x08, 0x01, 0x42, 0x71, 0x7d, 0x25, 0xd3, 
                0xb6, 0x80, 0x00, 0x08, 0x02, 0x08, 0x04 });
        }

        [Test]
        public void TestObjectArray()
        {
            BitPlantationSinglePropertyTest firstObject = new BitPlantationSinglePropertyTest("First");
            BitPlantationSinglePropertyTest secondObject = new BitPlantationSinglePropertyTest("Second");

            AmfArray objectArray = AmfArray.Dense(firstObject, secondObject, firstObject, secondObject);

            CodecTests.AssertRoundTrip(objectArray, new byte[] {
                0x09, 0x09, 0x01, 0x0a, 0x13, 0x41, 0x62, 0x69, 0x74, 0x70, 
                0x6c, 0x61, 0x6e, 0x74, 0x61, 0x74, 0x69, 0x6f, 0x6e, 0x2e, 
                0x53, 0x69, 0x6e, 0x67, 0x6c, 0x65, 0x50, 0x72, 0x6f, 0x70, 
                0x65, 0x72, 0x74, 0x79, 0x54, 0x65, 0x73, 0x74, 0x03, 0x41, 
                0x06, 0x0b, 0x46, 0x69, 0x72, 0x73, 0x74, 0x0a, 0x01, 0x06, 
                0x0d, 0x53, 0x65, 0x63, 0x6f, 0x6e, 0x64, 0x0a, 0x02, 0x0a, 
                0x04 });
        }

        [Test]
        public void TestByteArrayArray()
        {
            byte[] firstArray = new byte[] { };
            byte[] secondArray = new byte[] { 0x42, 0x00, 0x99 };

            AmfArray byteArrayArray = AmfArray.Dense(firstArray, secondArray, firstArray, secondArray);

            CodecTests.AssertRoundTrip(byteArrayArray, new byte[] {
                0x09, 0x09, 0x01, 0x0c, 0x01, 0x0c, 0x07, 0x42, 0x00, 0x99,
                0x0c, 0x02, 0x0c, 0x04 });
        }
    }
}
