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

using MbUnit.Framework;

using Interlace.Utilities;

#endregion

namespace Interlace.Tests.Utilities
{
    [TestFixture]
    public class TestNaturalParsers
    {
        [Test]
        public void TestPercentages()
        {
            Assert.AreEqual(0.01, NaturalParsers.ParsePercentage("1"));
            Assert.AreEqual(0.01, NaturalParsers.ParsePercentage("1%"));
            Assert.AreEqual(0.01, NaturalParsers.ParsePercentage(" 1 %"));
            Assert.AreEqual(0.01, NaturalParsers.ParsePercentage(" 1. %"));
            Assert.AreEqual(0.01, NaturalParsers.ParsePercentage(" 1.0 %"));
            Assert.AreEqual(0.012, NaturalParsers.ParsePercentage(" 1.2 % "));
            Assert.AreEqual(1.002, NaturalParsers.ParsePercentage(" 100.2 % "));
            Assert.AreEqual(-1.002, NaturalParsers.ParsePercentage(" - 100.2 % "));
        }

        public void TestMoney()
        {
            Assert.AreEqual(1.0M, NaturalParsers.ParseMoney("1"));
            Assert.AreEqual(1.0M, NaturalParsers.ParseMoney("$1"));
            Assert.AreEqual(1.0M, NaturalParsers.ParseMoney("$ 1. "));
            Assert.AreEqual(1.2M, NaturalParsers.ParseMoney("$ 1.2"));
            Assert.AreEqual(1.25M, NaturalParsers.ParseMoney("$ 1.25"));
            Assert.AreEqual(1.25M, NaturalParsers.ParseMoney(" $ 1.25 "));
            Assert.AreEqual(1.25M, NaturalParsers.ParseMoney(" 1.25 "));
            Assert.AreEqual(1000000.25M, NaturalParsers.ParseMoney(" 1,000,000.25 "));

            Assert.AreEqual(-1.25M, NaturalParsers.ParseMoney(" $ -1.25 "));
            Assert.AreEqual(-1.25M, NaturalParsers.ParseMoney(" -$ 1.25 "));
            Assert.AreEqual(-1.25M, NaturalParsers.ParseMoney(" - 1.25 "));
        }

        [Test]
        public void TestTime()
        {
            // Try a couple of spot tests:
            Assert.AreEqual(new TimeSpan(15, 12, 0), NaturalParsers.TryParseTime("3.12"));
            Assert.AreEqual(new TimeSpan(3, 12, 0), NaturalParsers.TryParseTime("3:12a"));
            Assert.AreEqual(new TimeSpan(3, 12, 0), NaturalParsers.TryParseTime("  3:12a  "));

            Assert.AreEqual(new TimeSpan(0, 59, 0), NaturalParsers.TryParseTime("0059"));

            Assert.AreEqual(new TimeSpan(0, 0, 0), NaturalParsers.TryParseTime("0"));

            Assert.AreEqual(new TimeSpan(13, 0, 0), NaturalParsers.TryParseTime("1"));
            Assert.AreEqual(new TimeSpan(18, 0, 0), NaturalParsers.TryParseTime("6"));

            Assert.AreEqual(new TimeSpan(7, 0, 0), NaturalParsers.TryParseTime("7"));
            Assert.AreEqual(new TimeSpan(11, 0, 0), NaturalParsers.TryParseTime("11"));
            Assert.AreEqual(new TimeSpan(12, 0, 0), NaturalParsers.TryParseTime("12"));

            Assert.IsNull(NaturalParsers.TryParseTime("12 trailing junk"));
            Assert.IsNull(NaturalParsers.TryParseTime("junk"));
            Assert.IsNull(NaturalParsers.TryParseTime("0pm"));
            Assert.IsNull(NaturalParsers.TryParseTime("14a"));

            // Followed by an exhaustive one:
            string[] formatStrings = new string[] { "h:mmt", "hh:mmt", "  h.mmtt", "HHmm" };

            for (int h = 0; h < 24; h++)
            {
                TimeSpan firstTime = new TimeSpan(h, 0, 0);
                TimeSpan secondTime = new TimeSpan(h, 59, 0);

                foreach (string formatString in formatStrings)
                {
                    Assert.AreEqual(firstTime, NaturalParsers.TryParseTime((DateTime.Today + firstTime).ToString(formatString)));
                    Assert.AreEqual(secondTime, NaturalParsers.TryParseTime((DateTime.Today + secondTime).ToString(formatString)));
                }
            }
        }

        public void TestTry()
        {
            Assert.IsNull(NaturalParsers.TryParseMoney("a"));
            Assert.IsNull(NaturalParsers.TryParsePercentage("a"));
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestPercentageException()
        {
            NaturalParsers.ParsePercentage("a");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestMoneyException()
        {
            NaturalParsers.ParseMoney("a");
        }
    }
}
