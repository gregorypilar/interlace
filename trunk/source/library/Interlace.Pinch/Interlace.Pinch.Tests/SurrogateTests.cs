using System;
using System.Collections.Generic;
using System.Text;

using MbUnit.Framework;

using Interlace.Pinch.Test;
using Interlace.Pinch.Implementation;

namespace Interlace.Pinch.Tests
{
    [TestFixture]
    public class SurrogateTests
    {
        [Test]
        public void TestSurrogates()
        {
            BucketOfSurrogates bucket = new BucketOfSurrogates();

            DateTime time = new DateTime(2003, 11, 22, 12, 32, 11);

            bucket.RequiredDateTime = time;
            bucket.OptionalDateTime = time;
            bucket.RequiredUri = new Uri("http://www.google.com/");
            bucket.OptionalUri = new Uri("http://www.google.com/");

            byte[] data = Pincher.Encode(bucket);

            BucketOfSurrogates returnBucket = Pincher.Decode<BucketOfSurrogates>(data);

            Assert.AreEqual(time, returnBucket.RequiredDateTime);
            Assert.AreEqual(time, returnBucket.OptionalDateTime);
            Assert.AreEqual(new Uri("http://www.google.com/"), returnBucket.RequiredUri);
            Assert.AreEqual(new Uri("http://www.google.com/"), returnBucket.OptionalUri);
        }
    }
}

namespace Interlace.Pinch.Test
{
    public partial class MyUriSurrogate
    {
        static MyUriSurrogate ValueToSurrogate(Uri uri)
        {
            MyUriSurrogate surrogate = new MyUriSurrogate();

            surrogate.Address = uri.ToString();

            return surrogate;
        }

        static Uri SurrogateToValue(MyUriSurrogate surrogate)
        {
            return new Uri(surrogate.Address);
        }
    }
    public partial class MyDateTimeSurrogate
    {
        static MyDateTimeSurrogate ValueToSurrogate(DateTime dateTime)
        {
            MyDateTimeSurrogate surrogate = new MyDateTimeSurrogate();

            surrogate.Ticks = dateTime.Ticks;

            return surrogate;
        }

        static DateTime SurrogateToValue(MyDateTimeSurrogate surrogate)
        {
            return new DateTime(surrogate.Ticks);
        }
    }
}
