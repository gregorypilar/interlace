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
using System.Reflection;
using System.Text;

using MbUnit.Framework;

#endregion

namespace RuntimeProxies
{
    [TestFixture]
    public class ProxyTests
    {
        public void OneRef(ref int a)
        {
            a = a + 1;
        }

        public void TwoOuts(out int a, out string b)
        {
            object[] array = new object[] { };
            a = (int)array[0];
            b = "b";
        }

        public struct TestSmallStruct
        {
            public int value;
        }

        public struct TestBigStruct
        {
            public int x;
            public int y;
            public int width;
            public int height;
        }

        public enum ProxyTestEnum
        {
            Foo = 0,
            Bar = 1,
            Big = 0x7fffffff
        }

        public interface ITest2
        {
            void Test();
        }

        public interface ITest
        {
            void TestVoidVoid();
            void TestVoidString(string a);
            void TestVoidStringString(string a, string b);
            string TestStringString(string a);
            string TestStringInt(int foo);
            TestSmallStruct AddOnes(TestSmallStruct s);
            TestBigStruct AddOnes(TestBigStruct s);
            int DoubleInt(int i);
            ProxyTestEnum ConvertEnum(ProxyTestEnum e);
            int SumNumbers(string method, params int[] numbers);
            void TestOut(out string a, out int b, out ProxyTestEnum e, out int[] arr, out TestSmallStruct s);
        }

        public class Test : ITest
        {
            public void TestVoidVoid()
            {
            }

            public void TestVoidString(string a)
            {
            }

            public void TestVoidStringString(string a, string b)
            {
            }

            public string TestStringString(string a)
            {
                return "TestStringString(string a)";
            }

            public string TestStringInt(int foo)
            {
                return "TestStringInt(int foo)";
            }

            public TestSmallStruct AddOnes(TestSmallStruct s)
            {
                TestSmallStruct copy = s;
                copy.value++;

                return copy;
            }

            public TestBigStruct AddOnes(TestBigStruct s)
            {
                TestBigStruct copy = s;
                copy.width++;
                copy.height++;
                copy.x++;
                copy.y++;

                return copy;
            }

            public int DoubleInt(int i)
            {
                return i * 2;
            }

            public ProxyTestEnum ConvertEnum(ProxyTestEnum e)
            {
                return e;
            }

            public int SumNumbers(string method, params int[] numbers)
            {
                int sum = 0;

                foreach (int n in numbers)
                {
                    sum += n;
                }

                return sum;
            }

            public void TestOut(out string a, out int b, out ProxyTestEnum e, out int[] arr, out TestSmallStruct s)
            {
                a = "a";
                b = 42;
                e = ProxyTestEnum.Bar;
                arr = new int[] { 1, 42, 3 };
                s = new TestSmallStruct();
            }
        }

        public class PrintProxyHandler : IProxyHandler
        {
            public object Invoke(object proxyObject, MethodInfo method, object[] arguments)
            {
                Console.WriteLine(method.Name);

                foreach (object argument in arguments)
                {
                    Console.Write("    ");
                    Console.WriteLine(argument);
                }

                Test test = new Test();

                object result = method.Invoke(test, arguments);

                return result;
            }
        }

        [Test]
        public void TextProxy()
        {
            ProxyGeneratorCore generator = new ProxyGeneratorCore();
            ProxyFactory<ITest> testFactory = generator.GenerateProxy<ITest>();
            ITest test = testFactory.Create(new PrintProxyHandler());

            test.TestVoidVoid();
            test.TestVoidString("a");
            test.TestVoidString(null);
            test.TestVoidStringString("a", "b");
            test.TestVoidStringString(null, "b");
            string firstResult = test.TestStringString("a");
            string secondResult = test.TestStringInt(42);
            TestSmallStruct thirdResult = test.AddOnes(new TestSmallStruct());
            TestBigStruct fourthResult = test.AddOnes(new TestBigStruct());
            int fifthResult = test.DoubleInt(42);
            ProxyTestEnum sixthResult = test.ConvertEnum(ProxyTestEnum.Bar);
            int firstSum = test.SumNumbers("test");
            int secondSum = test.SumNumbers("test", 1);
            int thirdSum = test.SumNumbers("test", 1, 2);

            string outA;
            int outB;
            ProxyTestEnum outE;
            int[] outArr;
            TestSmallStruct outS;

            test.TestOut(out outA, out outB, out outE, out outArr, out outS);

            Assert.AreEqual("a", outA);
            Assert.AreEqual(42, outB);
            Assert.AreEqual(ProxyTestEnum.Bar, outE);
            ArrayAssert.AreEqual(new int[] { 1, 42, 3 }, outArr);

            ProxyFactory<ITest> testA = Proxies.MakeProxyFactory<ITest>();
            ProxyFactory<ITest> testNextA = Proxies.MakeProxyFactory<ITest>();
            ProxyFactory<ITest2> testB = Proxies.MakeProxyFactory<ITest2>();

            Assert.IsTrue(object.ReferenceEquals(testA, testNextA));
            Assert.IsTrue(!object.ReferenceEquals(testA, testB));
        }
    }
}
