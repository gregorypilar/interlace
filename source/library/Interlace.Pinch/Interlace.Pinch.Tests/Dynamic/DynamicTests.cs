using System;
using System.Collections.Generic;
using System.Text;
using MbUnit.Framework;
using Interlace.Pinch.TestsVersion3;
using Interlace.Pinch.Implementation;
using Interlace.Pinch.Dynamic;

namespace Interlace.Pinch.Tests.Dynamic
{
    [TestFixture]
    public class DynamicTests
    {
        [Test]
        public void TestDynamicDecodingWithTypes()
        {
            DynamicPincher pincher = new DynamicPincher(@"..\..\TestVersion3.pinch");

            TypesStructure sample = new TypesStructure();

            sample.ReqFloat32 = 1.2f;
            sample.ReqFloat64 = 3.4;
            sample.ReqInt8 = 5;
            sample.ReqInt16 = 6;
            sample.ReqInt32 = 7;
            sample.ReqInt64 = 8;
            sample.ReqDecimal = 9.10M;
            sample.ReqBool = true;
            sample.ReqString = "Eleven";
            sample.ReqBytes = new byte[] { 1, 2 };
            sample.ReqEnumeration = TypesEnumeration.B;
            sample.ReqStructure = new SmallStructure();
            sample.ReqStructure.Test = 13;

            SmallStructure firstSmall = new SmallStructure();
            firstSmall.Test = 14;

            SmallStructure secondSmall = new SmallStructure();
            secondSmall.Test = 15;

            sample.ReqListOfEnum.Add(firstSmall);
            sample.ReqListOfEnum.Add(secondSmall);

            byte[] data = Pincher.Encode(sample);

            DynamicStructure structure = 
                pincher.Decode("Interlace.Pinch.TestsVersion3.TypesStructure", data);
        }

        [Test]
        public void TestDynamicDecoding()
        {
            DynamicPincher pincher = new DynamicPincher(@"..\..\TestVersion3.pinch");

            VersioningStructure sample = new VersioningStructure();
            sample.ReqScalar = 1;
            sample.ReqPointer = "Two";
            sample.ReqStructure = new SmallStructure();
            sample.ReqStructure.Test = 3;

            sample.AddedReqScalar = 4;
            sample.AddedReqPointer = "Five";
            sample.AddedReqStructure = new SmallStructure();
            sample.AddedReqStructure.Test = 6;

            byte[] data = Pincher.Encode(sample);

            DynamicStructure structure = 
                pincher.Decode("Interlace.Pinch.TestsVersion3.VersioningStructure", data);

            Assert.AreEqual("Five", structure.Members["AddedReqPointer"].Value);

        	ChoiceMessage choiceSample = new ChoiceMessage();
            choiceSample.Test = 1234;
            choiceSample.Choice = new SmallStructure();
            choiceSample.Choice.Small.Test = 5;

            byte[] choiceData = Pincher.Encode(choiceSample);

            DynamicStructure choiceStructure = 
                pincher.Decode("Interlace.Pinch.TestsVersion3.ChoiceMessage", choiceData);

            Assert.AreEqual(1234, choiceStructure["Test"]);

            DynamicStructure assertStructure = (DynamicStructure)choiceStructure["Choice"];

            Assert.AreEqual(5, assertStructure["Test"]);
        }
    }
}
