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

using Interlace.Pinch.Implementation;
using System.IO;

#endregion

namespace Interlace.Pinch.Tests
{
    [TestFixture]
    public class VersioningTests
    {
        [Test]
        public void TestUpgrade()
        {
            Interlace.Pinch.TestsVersion1.VersioningStructure oldStructure = new Interlace.Pinch.TestsVersion1.VersioningStructure();
            
            oldStructure.ReqScalar = 101;
            oldStructure.ReqPointer = "Hello, World.";
            oldStructure.ReqStructure = new Interlace.Pinch.TestsVersion1.SmallStructure();
            oldStructure.ReqStructure.Test = 102;
            oldStructure.OptScalar = 103;
            oldStructure.OptPointer = "Goodbye!";
            oldStructure.OptStructure = new Interlace.Pinch.TestsVersion1.SmallStructure();
            oldStructure.OptStructure.Test = 104;
            oldStructure.RemovedOptScalar = 105;
            oldStructure.RemovedOptPointer = "OhNoes";
            oldStructure.RemovedOptStructure = new Interlace.Pinch.TestsVersion1.SmallStructure();
            oldStructure.RemovedOptStructure.Test = 106;

            byte[] encoded = Pincher.Encode(oldStructure);

            Interlace.Pinch.TestsVersion3.VersioningStructure newStructure = 
                Pincher.Decode<Interlace.Pinch.TestsVersion3.VersioningStructure>(encoded);

            Assert.AreEqual(oldStructure.ReqScalar, newStructure.ReqScalar);
            Assert.AreEqual(oldStructure.ReqPointer, newStructure.ReqPointer);
            Assert.AreEqual(oldStructure.ReqStructure.Test, newStructure.ReqStructure.Test);
            Assert.AreEqual(oldStructure.OptScalar, newStructure.OptScalar);
            Assert.AreEqual(oldStructure.OptPointer, newStructure.OptPointer);
            Assert.AreEqual(oldStructure.OptStructure.Test, newStructure.OptStructure.Test);

            Assert.AreEqual("Added1", newStructure.AddedOptPointer);
            Assert.AreEqual(2, newStructure.AddedOptScalar);
            Assert.AreEqual(3, newStructure.AddedOptStructure.Test);
            Assert.AreEqual("Added4", newStructure.AddedReqPointer);
            Assert.AreEqual(5, newStructure.AddedReqScalar);
            Assert.AreEqual(6, newStructure.AddedReqStructure.Test);
        }

        [Test]
        public void TestDowngrade()
        {
            Interlace.Pinch.TestsVersion3.VersioningStructure newStructure = new Interlace.Pinch.TestsVersion3.VersioningStructure();
            Interlace.Pinch.TestsVersion3.VersioningStructure emptyStructure = new Interlace.Pinch.TestsVersion3.VersioningStructure();

            newStructure.ReqScalar = 1;
            newStructure.ReqPointer = "Two";
            newStructure.ReqStructure = new Interlace.Pinch.TestsVersion3.SmallStructure();
            newStructure.ReqStructure.Test = 3;
            newStructure.OptScalar = 4;
            newStructure.OptPointer = "Five";
            newStructure.OptStructure = new Interlace.Pinch.TestsVersion3.SmallStructure();
            newStructure.OptStructure.Test = 6;
            newStructure.AddedOptPointer = "Seven";
            newStructure.AddedOptScalar = 8;
            newStructure.AddedOptStructure = new Interlace.Pinch.TestsVersion3.SmallStructure();
            newStructure.AddedOptStructure.Test = 9;
            newStructure.AddedReqPointer = "Ten";
            newStructure.AddedReqScalar = 11;
            newStructure.AddedReqStructure = new Interlace.Pinch.TestsVersion3.SmallStructure();
            newStructure.AddedReqStructure.Test = 12;

            emptyStructure.ReqStructure = new Interlace.Pinch.TestsVersion3.SmallStructure();
            emptyStructure.ReqPointer = "";
            emptyStructure.AddedReqStructure = new Interlace.Pinch.TestsVersion3.SmallStructure();
            emptyStructure.AddedReqPointer = "";

            MemoryStream encoded = new MemoryStream();

            Pincher.Encode(emptyStructure, encoded);
            Pincher.Encode(newStructure, encoded);

            encoded.Seek(0, SeekOrigin.Begin);

            Interlace.Pinch.TestsVersion1.VersioningStructure oldEmptyStructure =
                Pincher.Decode<Interlace.Pinch.TestsVersion1.VersioningStructure>(encoded);

            Interlace.Pinch.TestsVersion1.VersioningStructure oldStructure =
                Pincher.Decode<Interlace.Pinch.TestsVersion1.VersioningStructure>(encoded);

            Assert.AreEqual(oldStructure.ReqScalar, newStructure.ReqScalar);
            Assert.AreEqual(oldStructure.ReqPointer, newStructure.ReqPointer);
            Assert.AreEqual(oldStructure.ReqStructure.Test, newStructure.ReqStructure.Test);
            Assert.AreEqual(oldStructure.OptScalar, newStructure.OptScalar);
            Assert.AreEqual(oldStructure.OptPointer, newStructure.OptPointer);
            Assert.AreEqual(oldStructure.OptStructure.Test, newStructure.OptStructure.Test);
        }
    }
}
