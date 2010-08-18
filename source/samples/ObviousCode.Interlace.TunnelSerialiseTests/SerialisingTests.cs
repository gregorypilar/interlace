using System.IO;
using System.Linq;
using Interlace.PropertyLists;
using MbUnit.Framework;
using ObviousCode.Interlace.TunnelSerialiser;

namespace ObviousCode.Interlace.TunnelSerialiseTests
{
    [TestFixture]
    public class SerialisingTests
    {
        TunnelledClass _tunnelMe;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            _tunnelMe = new TunnelledClass();
            _tunnelMe.IntField = 42;
            _tunnelMe.StringField = "Hello World!";
            _tunnelMe.ADouble = 123.4d;
            _tunnelMe.EnumField = TunnelledClass.TunnelNum.Two;

            //Currently serialising of bytes not available
            //Speak to Ben about passing byte[] straight through

            //using (MemoryStream stream = new  MemoryStream())
            //{
            //    using (BinaryWriter writer = new BinaryWriter(stream))
            //    {
            //        writer.Write("This is some binary text");
            //        _tunnelMe.Bytes = stream.ToArray();
            //    }
            //}
        }

        [Test]
        public void TestDifferentTypes()
        {
            
        }


        [Test]
        public void WhenDecoratedObjectPersistedToDictionary_CorrectTypesShoudBeAvailable()
        {
            using (MemoryStream stream = new MemoryStream(Serialiser.Tunnel(_tunnelMe)))
            {
                PropertyDictionary dictionary = PropertyDictionary.FromStream(stream);

                Assert.IsTrue(dictionary.HasValueFor(new string[] { "IntField", "StringField", "Fred", "EnumField"/*, "Bytes"*/}));
            }            
        }

        [Test]
        public void WhenDecoratedObjectPersistedToDictionary_CorrectValuesShoudBeAvailable()
        {
            using (MemoryStream stream = new MemoryStream(Serialiser.Tunnel(_tunnelMe)))
            {
                PropertyDictionary dictionary = PropertyDictionary.FromStream(stream);

                Assert.AreEqual(dictionary.StringFor("StringField"), "Hello World!" );
                Assert.AreEqual(dictionary.IntegerFor("IntField"), 42);
                Assert.AreEqual(dictionary.ValueFor("Fred"), "123.4");
                Assert.AreEqual(dictionary.ValueFor("EnumField"), "Two");
             
            }
        }

        [Test]
        public void WhenDecoratedObjectPersistedToDictionary_CorrectObjectCountShoudBeAvailable()
        {
            using (MemoryStream stream = new MemoryStream(Serialiser.Tunnel(_tunnelMe)))
            {
                PropertyDictionary dictionary = PropertyDictionary.FromStream(stream);

                Assert.AreEqual(dictionary.Keys.Count(), 5);
            }
        }

        [Test]
        public void WhenDecoratedObjectWithPublicProperty_PrivateSetRestored_RestoreShouldSucceed()
        {
            PublicPropertyPrivatePropertySet privateProperty = new PublicPropertyPrivatePropertySet(42);

            byte[] serialised = Serialiser.Tunnel(privateProperty);

            PublicPropertyPrivatePropertySet restored = Serialiser.Restore<PublicPropertyPrivatePropertySet>(serialised);

            Assert.AreEqual(42, privateProperty.Number);
        }

        [Test]
        public void WhenDecoratedObjectWithPrivateProperty_RestoreShouldFailToSetPrivateProperty()
        {
            PrivateProperty privateProperty = new PrivateProperty(42);

            byte[] serialised = Serialiser.Tunnel(privateProperty);

            PrivateProperty restored = Serialiser.Restore<PrivateProperty>(serialised);

            Assert.AreEqual(0, restored.GetPrivateNumber());
        }
    }
}
