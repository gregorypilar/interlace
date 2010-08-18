using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using ObviousCode.Interlace.TunnelSerialiser;
using Interlace.PropertyLists;
using System.IO;

namespace ObviousCode.Interlace.TunnelSerialiseTests
{
    [TestFixture]
    public class InheritanceTests
    {
        SubClass _subClass;
        SubTunnel _subTunnel;

        [TestFixtureSetUp]
        public void Setup()
        {
            _subTunnel = new SubTunnel();

            _subTunnel.ADouble = 123.4d;
            _subTunnel.DidThisWork = true;
            _subTunnel.DontTunnelMe = 12;
            _subTunnel.IntField = 42;
            _subTunnel.StringField = "Hello World";
            _subTunnel.EnumField = TunnelledClass.TunnelNum.Two;

            _subClass = new SubClass();

            _subClass.ADouble = 123.4d;
            _subClass.DidThisWork = true;
            _subClass.DontTunnelMe = 12;
            _subClass.IntField = 42;
            _subClass.StringField = "Hello World";
            _subClass.EnumField = TunnelledClass.TunnelNum.Two;
        }

        #region subClass

        #region subClass - No Inheritance
        [Test]
        [ExpectedException(typeof(InvalidOperationException), "Only objects decorated with the Tunnel Attribute may be serialised.")]
        public void WhenInheritedClass_NoAttribute_NoInheritanceAllowedInTunnel_TunnelShouldFail()
        {
            byte[] bytes = Serialiser.Tunnel(_subClass, false);

        } 
        #endregion

        #region subClass Allow Inheritance
        [Test]
        public void WhenInheritedClass_NoAttribute_InheritanceAllowedInTunnel_TunnelShouldSucceed()
        {
            byte[] bytes = Serialiser.Tunnel(_subClass, true);
        }

        [Test]
        public void WhenInheritedClass_NoAttribute_InheritanceAllowedInTunnel_AllFieldsShouldBeSerialised()
        {
            using (MemoryStream stream = new MemoryStream(Serialiser.Tunnel(_subClass, true)))
            {
                PropertyDictionary dictionary = PropertyDictionary.FromStream(stream);

                Assert.AreEqual(6, dictionary.Keys.Count());
            }
        } 
        #endregion 

        #endregion

        #region subTunnel

        #region No Inheritance

        [Test]        
        public void WhenInheritedTunnel_Attribute_NoInheritanceAllowedInTunnel_TunnelShouldSucceed()
        {
            byte[] bytes = Serialiser.Tunnel(_subTunnel, false);
        }

        [Test]
        public void WhenInheritedTunnel_Attribute_NoInheritanceAllowedInTunnel_TunnelShouldSucceed_OnlySubTunnelFieldsStored()
        {            
            using (MemoryStream stream = new MemoryStream(Serialiser.Tunnel(_subTunnel, false)))
            {
                PropertyDictionary dictionary = PropertyDictionary.FromStream(stream);

                Assert.AreEqual(1, dictionary.Keys.Count());
            }
        }

        [Test]
        public void WhenInheritedTunnel_Attribute_NoInheritanceAllowedInTunnel_TunnelShouldSucceed_OnlySubTunnelFieldsShouldBeRestored()
        {
            SubTunnel tunnel = Serialiser.Restore<SubTunnel>(Serialiser.Tunnel(_subTunnel, false));

            Assert.AreEqual(0d, tunnel.ADouble);
            Assert.AreEqual(true, tunnel.DidThisWork);
            Assert.AreEqual(0, tunnel.DontTunnelMe);
            Assert.AreEqual(0, tunnel.IntField);
            Assert.AreEqual(TunnelledClass.TunnelNum.One, tunnel.EnumField);
            Assert.IsNull(tunnel.StringField);
        }

        #region Allow Inheritance

        [Test]
        public void WhenInheritedTunnel_Attribute_InheritanceAllowedInTunnel_PrivateSet_TunnelShouldSucceed_AllFieldsShouldBeRestored()
        {
            SubTunnel tunnel = Serialiser.Restore<SubTunnel>(Serialiser.Tunnel(_subTunnel, true), true);

            Assert.AreEqual(123.4d, tunnel.ADouble);
            Assert.AreEqual(true, tunnel.DidThisWork);
            Assert.AreEqual(0, tunnel.DontTunnelMe);
            Assert.AreEqual(42, tunnel.IntField);
            Assert.AreEqual(TunnelledClass.TunnelNum.Two, tunnel.EnumField);
            Assert.AreEqual("Hello World", tunnel.StringField);
        }
        #endregion

        #endregion

        #endregion
    }

}
