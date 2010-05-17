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
using System.Text;

using EntrustDal.EntityClasses;

using MbUnit.Framework;

using Interlace.Collections;

#endregion

namespace Interlace.Tests.Collections
{
    [TestFixture]
    public class BucketSet
    {
        List<TaskEntity> _tasks;
        BucketSet<TaskEntity> _buckets;

        DateTime _testTime;

        [SetUp]
        public void CreateDefaultData()
        {
            _tasks = new List<TaskEntity>();
            _buckets = new BucketSet<TaskEntity>("FirstEntryDate", BucketLumpType.IncludeItemsLessThanComparison);

            Random rng = new Random();

            for (int i = 0; i < 100; i++)
            {
                TaskEntity task = new TaskEntity();

                task.FirstEntryDate = DateTime.Now.AddDays(rng.Next(-365, 365));

                _tasks.Add(task);
            }

            _testTime = DateTime.Now;

            _buckets.AddRemainderBucket();
            _buckets.AddBucket(_testTime);
            _buckets.AddBucket(_testTime.AddDays(100));
            _buckets.AddBucket(_testTime.AddDays(200));
        }

        [Test]
        public void Go()
        {
            _buckets.FillBuckets(_tasks);

            // Make sure our buckets are in the right order.
            Assert.AreEqual(_buckets[0].ComparisonObject, _testTime);
            Assert.AreEqual(_buckets[1].ComparisonObject, _testTime.AddDays(100));
            Assert.AreEqual(_buckets[2].ComparisonObject, _testTime.AddDays(200));
            Assert.IsTrue(_buckets[3] is RemainderBucket<TaskEntity>);

            // And make sure that the buckets picked up what they should have.
            foreach (Bucket<TaskEntity> bucket in _buckets)
            {
                foreach (TaskEntity task in bucket.Items)
                {
                    // Make sure the remainder bucket really did pick up the remainder.
                    if (bucket is RemainderBucket<TaskEntity>)
                    {
                        Assert.IsTrue(task.FirstEntryDate > _testTime.AddDays(200));
                    }
                    else
                    {
                        Assert.IsTrue(task.FirstEntryDate <= (bucket.ComparisonObject as DateTime?));
                    }
                }
            }
        }
    }
}
