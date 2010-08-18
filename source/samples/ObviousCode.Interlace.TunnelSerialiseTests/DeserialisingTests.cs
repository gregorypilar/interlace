using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using ObviousCode.Interlace.TunnelSerialiser;

namespace ObviousCode.Interlace.TunnelSerialiseTests
{
    [TestFixture]
    public class DeserialisingTests
    {
        byte[] _data;

        [TestFixtureSetUp]
        public void Setup()
        {
            TunnelledClass tunnelMe = new TunnelledClass();
            
            tunnelMe.IntField = 42;
            tunnelMe.StringField = "Hello World!";
            tunnelMe.ADouble = 123.6d;
            tunnelMe.EnumField = TunnelledClass.TunnelNum.Three;

            _data = Serialiser.Tunnel(tunnelMe);
        }

        [Test]
        public void WhenClassTunneled_RestoreDataShouldBeCorrect()
        {
            TunnelledClass restored = Serialiser.Restore<TunnelledClass>(_data);

            Assert.AreEqual(42, restored.IntField);
            Assert.AreEqual(123.6d, restored.ADouble);
            Assert.AreEqual("Hello World!", restored.StringField);
            Assert.AreEqual(TunnelledClass.TunnelNum.Three, restored.EnumField);
        }
    }
}
