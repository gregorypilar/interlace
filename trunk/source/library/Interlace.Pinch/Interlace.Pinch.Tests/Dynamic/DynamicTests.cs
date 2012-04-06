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
