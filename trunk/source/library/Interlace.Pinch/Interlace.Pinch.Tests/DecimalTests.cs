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
using Interlace.Pinch.TestsVersion3;

#endregion

namespace Interlace.Pinch.Tests
{
    [TestFixture]
    public class DecimalTests
    {
        [Test]
        public void TestRoundTripping()
        {
            decimal[] values = new decimal[] {
                0M,
                0.0M,
                1M,
                1.1M,
                -0M,
                -0.0M,
                -1M,
                -1.1M,
                decimal.MaxValue,
                decimal.MinValue,
                0.0000000000000000000000000001M,
                -0.0000000000000000000000000001M,
            };

            foreach (decimal value in values)
            {
                AssertRoundTrip(value);
            }

            decimal bitScanValue = 1;

            for (int i = 0; i < 95; i++)
            {
                AssertRoundTrip(bitScanValue);

                bitScanValue *= 2;
            }
        }

        public void AssertRoundTrip(decimal value)
        {
            RequiredDecimalStructure forEncoding = new RequiredDecimalStructure();
            forEncoding.Value = value;

            byte[] bytes = Pincher.Encode(forEncoding);

            RequiredDecimalStructure fromDecoding = Pincher.Decode<RequiredDecimalStructure>(bytes);

            Assert.AreEqual(fromDecoding.Value, forEncoding.Value);
        }

        [Test]
        public void TestEncodingAndDecoding()
        {
            AssertEncodesAndDecodesTo(0M, 0xc1, 0x82, 0, 0);
            AssertEncodesAndDecodesTo(1M, 0xc1, 0x83, 0, 0, 1);
            AssertEncodesAndDecodesTo(1.0M, 0xc1, 0x83, 1, 0, 10);
            AssertEncodesAndDecodesTo(-1.0M, 0xc1, 0x83, 1, 0x80, 10);

            AssertEncodesAndDecodesTo(decimal.MaxValue, 0xc1, 0x8e, 0, 0, 
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 
                0xff, 0xff, 0xff, 0xff);

            AssertEncodesAndDecodesTo(0.0000000000000000000000000001M, 0xc1, 0x83, 28, 0, 1);
            AssertEncodesAndDecodesTo(-0.0000000000000000000000000001M, 0xc1, 0x83, 28, 0x80, 1);
        }

        public void AssertEncodesAndDecodesTo(decimal value, params byte[] bytes)
        {
            AssertEncodesTo(value, bytes);
            AssertDecodesTo(value, bytes);
        }

        public void AssertEncodesTo(decimal value, params byte[] expectedBytes)
        {
            RequiredDecimalStructure decimalStructure = new RequiredDecimalStructure();
            decimalStructure.Value = value;

            byte[] actualBytes = Pincher.Encode(decimalStructure);

            ArrayAssert.AreEqual(expectedBytes, actualBytes);
        }
        
        public void AssertDecodesTo(decimal expectedValue, params byte[] bytes)
        {
            RequiredDecimalStructure decimalStructure = Pincher.Decode<RequiredDecimalStructure>(bytes);

            Assert.AreEqual(expectedValue, decimalStructure.Value);
        }
    }
}
