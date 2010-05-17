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

using Interlace.Collections;
using Interlace.PropertyLists;
using Interlace.Utilities;

#endregion

namespace Interlace.Tests.PropertyLists
{
    [TestFixture]
    public class TestParser
    {
        [Test]
        public void TestParsing()
        {
            // Empty dictionaries:
            PropertyDictionary empty =
                PropertyDictionary.FromString("{}");

            // Small, unquoted strings:
            PropertyDictionary smallDictionary =
                PropertyDictionary.FromString("{ foo = bar }");

            Assert.AreEqual("bar", smallDictionary.StringFor("foo"));
            Assert.AreEqual("moo", smallDictionary.StringFor("foo2", "moo"));

            // Extra characters:
            PropertyDictionary symbolDictionary =
                PropertyDictionary.FromString("{ _foo = baa-she_ep }");

            Assert.AreEqual("baa-she_ep", symbolDictionary.StringFor("_foo"));

            // Quoted strings, escaping and numbers:
            PropertyDictionary bigDictionary =
                PropertyDictionary.FromString("{ foo = \"The quick brown\\nfox!\\\"\"; bar = 342 }");

            Assert.AreEqual("The quick brown\nfox!\"", bigDictionary.StringFor("foo"));
            Assert.AreEqual(342, bigDictionary.IntegerFor("bar"));

            // Dictionaries in dictionaries:
            PropertyDictionary nestedDictionary =
                PropertyDictionary.FromString("{ foo = { 1 = 3; 2 = 9 } }");

            PropertyDictionary innerDictionary = nestedDictionary.DictionaryFor("foo");
            Assert.AreEqual(3, innerDictionary.IntegerFor(1));
            Assert.AreEqual(9, innerDictionary.IntegerFor(2));

            // Arrays in dictionaries:
            PropertyDictionary listInDictionary =
                PropertyDictionary.FromString("\n{ a = (1, 2, 3, \"moo\") }");
            PropertyArray array = listInDictionary.ArrayFor("a");

            Assert.AreEqual(4, array.Count);
            Assert.AreEqual(1, array.IntegerAt(0));
            Assert.AreEqual(2, array.IntegerAt(1));
            Assert.AreEqual(3, array.IntegerAt(2));
            Assert.AreEqual("moo", array.StringAt(3));

            // Comments in dictionaries:
            PropertyDictionary commentedDictionary =
                PropertyDictionary.FromString("{ foo = // MEOW! // !! \" \n 3; bar = 1 }");

            Assert.AreEqual(3, commentedDictionary.IntegerFor("foo"));
            Assert.AreEqual(1, commentedDictionary.IntegerFor("bar"));

            // Booleans in dictionaries:
            PropertyDictionary booleanDictionary =
                PropertyDictionary.FromString("{ hello = true; \r\nstuff = false;\r\nother-thing = \"true\"; }");

            Assert.AreEqual(true, booleanDictionary.BooleanFor("hello"));
            Assert.AreEqual(false, booleanDictionary.BooleanFor("stuff"));
            Assert.AreEqual("true", booleanDictionary.StringFor("other-thing"));

            // Doubles in dictionaries:
            PropertyDictionary doubleDictionary =
                PropertyDictionary.FromString("{ a = 1.2; b = -1.3; c = 1.3e10; d = 2; e = -3}");

            Assert.AreEqual(double.Parse("1.2"), doubleDictionary.DoubleFor("a"));
            Assert.AreEqual(double.Parse("-1.3"), doubleDictionary.DoubleFor("b"));
            Assert.AreEqual(double.Parse("1.3e10"), doubleDictionary.DoubleFor("c"));
            Assert.AreEqual(double.Parse("2.0"), doubleDictionary.DoubleFor("d"));
            Assert.AreEqual(double.Parse("-3.0"), doubleDictionary.DoubleFor("e"));
        }

        void AssertPropertyValuesAreEqual(object leftValue, object rightValue)
        {
            if (leftValue is PropertyDictionary)
            {
                Assert.IsInstanceOfType(typeof(PropertyDictionary), rightValue);

                AssertDictionariesEqual(leftValue as PropertyDictionary, rightValue as PropertyDictionary);
            }
            else if (leftValue is PropertyArray)
            {
                Assert.IsInstanceOfType(typeof(PropertyArray), rightValue);

                AssertArraysEqual(leftValue as PropertyArray, rightValue as PropertyArray);
            }
            else
            {
                Assert.AreEqual(leftValue, rightValue);
            }
        }

        void AssertArraysEqual(PropertyArray left, PropertyArray right)
        {
            Assert.AreEqual(left.Count, right.Count);

            for (int i = 0; i < left.Count; i++)
            {
                AssertPropertyValuesAreEqual(left[i], right[i]);
            }
        }

        void AssertDictionariesEqual(PropertyDictionary left, PropertyDictionary right)
        {
            Set<object> leftKeys = new Set<object>(left.Keys);
            Set<object> rightKeys = new Set<object>(right.Keys);

            Assert.AreEqual(leftKeys, rightKeys);

            foreach (object key in leftKeys)
            {
                object leftValue = left.ValueFor(key);
                object rightValue = right.ValueFor(key);

            }
        }

        void AssertRoundTrip(string originalDictionary)
        {
            // Get a property dictionary from a string (this is really just a convenience for the tests):
            PropertyDictionary firstStep = PropertyDictionary.FromString(originalDictionary);

            // Round trip it:
            string secondStep = firstStep.PersistToString();

            PropertyDictionary thirdStep;

            try
            {
                thirdStep = PropertyDictionary.FromString(secondStep);
            }
            catch (PropertyListException e)
            {
                throw new ApplicationException(string.Format(
                    "AssertRoundTrip failing with: {0}", secondStep), e);
            }

            // Test it:
            AssertDictionariesEqual(firstStep, thirdStep);
        }

        [Test]
        public void TestMarshalling()
        {
            AssertRoundTrip("{}");
            AssertRoundTrip("{ foo = bar }");
            AssertRoundTrip("{ foo = bar; bar = foo }");
            AssertRoundTrip("{ \"f=oo\" = bar; bar = 53 }");
            AssertRoundTrip("{ \"f=oo\" = bar; bar = { a = 1; b = 2 } }");

            AssertRoundTrip("{ numbers = (1, 2, 3) }");
            AssertRoundTrip("{ others = ({}, (), { a = b }, (\"foo\", bar)) }");
        }
    }
}
