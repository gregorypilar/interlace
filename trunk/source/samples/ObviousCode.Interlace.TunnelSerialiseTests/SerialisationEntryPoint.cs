using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;
using ObviousCode.Interlace.TunnelSerialiser;

namespace ObviousCode.Interlace.TunnelSerialiseTests
{
    [TestFixture]
    public class Tests
    {
        TunnelledClass _tunnelMe;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            //byte[] bytes = new byte[512];

            //for (int i = 0; i < 256; i++)
            //{
            //    bytes[i] = (byte)i;
            //}

            //for (int i = 255; i >= 0; i--)
            //{
            //    bytes[511 - i] = (byte)i;
            //}

            _tunnelMe = new TunnelledClass(123);
            _tunnelMe.IntField = 42;
            _tunnelMe.StringField = "Hello World!";
            _tunnelMe.ADouble = 12.54d;
            _tunnelMe.EnumField = TunnelledClass.TunnelNum.Two;
            //_tunnelMe.Bytes = bytes;            
        }

        [Test]
        public void EntryPoint()
        {
            byte[] serialised = Serialiser.Tunnel(_tunnelMe);
            TunnelledClass roundTrip = TunnelSerialiser.Serialiser.Restore<TunnelledClass>(serialised);            
        }
    }

    [Tunnel]
    public class PublicPropertyPrivatePropertySet
    {
        public PublicPropertyPrivatePropertySet()
        {

        }

        public PublicPropertyPrivatePropertySet(int number)
        {
            Number = number;
        }
        
        [Tunnel]
        public int Number { get; private set; }
    }

    [Tunnel]
    public class PrivateProperty
    {
        public PrivateProperty()
        {

        }

        public PrivateProperty(int number)
        {
            Number = number;
        }

        [Tunnel]
        private int Number { get; set; }

        internal object GetPrivateNumber()
        {
            return Number;
        }
    }

    [Tunnel]
    public class TunnelledClass
    {
        public TunnelledClass()
        {
            
        }
        public TunnelledClass(int anotherInt)
        {
            AnotherIntNumber = anotherInt;
        }
        public enum TunnelNum {One, Two, Three}
        [Tunnel]
        public TunnelNum EnumField { get; set; }
        [Tunnel]
        public int IntField { get; set; }
        [Tunnel]
        public string StringField { get; set; }
        [Tunnel("Fred")]
        public double ADouble { get; set; }
        [Tunnel]
        public int AnotherIntNumber { get; set; }
        public int DontTunnelMe { get; set; }
        //[Tunnel]
        //public byte[] Bytes { get; set; }
    }

    [Tunnel]
    public class SubTunnel : TunnelledClass
    {
        [Tunnel]
        public bool DidThisWork { get; set; }
    }

    public class SubClass : TunnelledClass
    {
        [Tunnel]
        public bool DidThisWork { get; set; }
    }
}
