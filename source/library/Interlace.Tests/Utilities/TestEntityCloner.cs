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
using EntrustDal.HelperClasses;

using MbUnit.Framework;

using SD.LLBLGen.Pro.ORMSupportClasses;

using Interlace.Utilities;

#endregion

namespace Interlace.Tests
{
    [TestFixture]
    public class TestEntityCloner
    {
        EntityCloner<TaskEntity> _taskCloner;
        EntityCloner<UserEntity> _userCloner;

        TaskEntity _task;
        UserEntity _user;

        [SetUp]
        public void SetUp()
        {
            // Create a user cloner:
            _userCloner = new EntityCloner<UserEntity>();

            _userCloner.Copied.Add(
                UserFields.Name,
                UserFields.Username,
                UserFields.Password,
                UserFields.WindowsDomainAndUsername,
                UserFields.Telephone,
                UserFields.CompanyName,
                UserFields.EmailAddress,
                UserFields.CultureName,
                UserFields.GrantedSecurityLevel);

            _userCloner.IgnoredRelations.Add(
                UserEntity.PrefetchPathCertificate,
                UserEntity.PrefetchPathCreatedDocuments,
                UserEntity.PrefetchPathEntryCollectionViaTimesheetEntry,
                UserEntity.PrefetchPathModifiedDocuments,
                UserEntity.PrefetchPathNotifications,
                UserEntity.PrefetchPathProjects,
                UserEntity.PrefetchPathSigningRequest,
                UserEntity.PrefetchPathTaskCollectionViaTimesheetEntry,
                UserEntity.PrefetchPathTasks,
                UserEntity.PrefetchPathActivities,
                UserEntity.PrefetchPathWatches);

            // Create a task cloner:
            _taskCloner = new EntityCloner<TaskEntity>();

            _taskCloner.Copied.Add(
                TaskFields.Title,
                TaskFields.Status,
                TaskFields.AssignedToId,
                TaskFields.Priority,
                TaskFields.ReplacedById,
                TaskFields.FirstEntryDateUtc,
                TaskFields.LastEntryDateUtc);

            _taskCloner.Ignored.Add(
                TaskFields.DueAtUtc,
                TaskFields.DueAtPrecision,
                TaskFields.DueAtReason,
                TaskFields.OptimisticEffortEstimate,
                TaskFields.LikelyEffortEstimate,
                TaskFields.PessimisticEffortEstimate,
                TaskFields.ActualEffort);

            _taskCloner.IgnoredRelations.Add(
                TaskEntity.PrefetchPathEntries,
                TaskEntity.PrefetchPathToLinks,
                TaskEntity.PrefetchPathFromLinks,
                TaskEntity.PrefetchPathProjectAssignments,
                TaskEntity.PrefetchPathReplaces,
                TaskEntity.PrefetchPathTimesheetEntry,
                TaskEntity.PrefetchPathProjects,
                TaskEntity.PrefetchPathWatchers);

    		_taskCloner.IgnoredRelations.Add(
                TaskEntity.PrefetchPathEntryCollectionViaTimesheetEntry,
    		    TaskEntity.PrefetchPathUserCollectionViaTimesheetEntry);

            _taskCloner.CopiedRelations.Add(TaskEntity.PrefetchPathAssignedTo, _userCloner);
            _taskCloner.CopiedRelations.Add(TaskEntity.PrefetchPathReplacedBy, _taskCloner);

            EntityCloner<WatchEntity> watchCloner = new EntityCloner<WatchEntity>();

            watchCloner.Copied.Add(
                WatchFields.TaskId,
                WatchFields.UserId);

            watchCloner.IsRecursive = false;

            _taskCloner.CopiedRelations.Add(TaskEntity.PrefetchPathWatches, watchCloner);

            // Create a task:
            _task = new TaskEntity();

            _task.Id = 12;
            _task.Title = "Buy Jessica some flowers to show appreciation for many a coffee";
            _task.Status = "open";
            _task.Priority = 150;
            _task.DueAtUtc = DateTime.UtcNow;

            // Create a user:
            _user = new UserEntity();
            _user.Id = 123;
            _user.Name = "Julie Von Twinkle";

            _task.AssignedTo = _user;
        }

        [Test]
        public void TestSimpleCloning()
        {
            TaskEntity copiedTask = _taskCloner.Clone(_task);

            Assert.AreEqual(_task.Title, copiedTask.Title);
            Assert.AreEqual(_task.Status, copiedTask.Status);
            Assert.AreEqual(_task.Priority, copiedTask.Priority);
            Assert.AreNotEqual(_task.DueAtUtc, copiedTask.DueAtUtc);
        }

        [Test]
        public void TestOneToOneCloning()
        {
            TaskEntity copiedTask = _taskCloner.Clone(_task);
            UserEntity copiedUser = copiedTask.AssignedTo;

            Assert.IsNotNull(copiedTask.AssignedTo);
        }

        [Test]
        public void TestThatFieldsAreNotShared()
        {
            TaskEntity copiedTask = _taskCloner.Clone(_task);

            Assert.AreEqual(_task.Title, copiedTask.Title);

            copiedTask.Title = copiedTask.Title + " (This title has been modified)";

            Assert.AreNotEqual(_task.Title, copiedTask.Title);
        }

        [Test]
        [ExpectedException(typeof(EntityClonerException))]
        public void TestPrimaryKeyInCopiedException()
        {
            _taskCloner.Copied.Add(TaskFields.Id);

            _taskCloner.Clone(_task);
        }

        [Test]
        [ExpectedException(typeof(EntityClonerException))]
        public void TestPrimaryKeyInIgnoredException()
        {
            _taskCloner.Ignored.Add(TaskFields.Id);

            _taskCloner.Clone(_task);
        }

        [Test]
        [ExpectedException(typeof(EntityClonerException))]
        public void TestUnrelatedField()
        {
            _taskCloner.Ignored.Add(LicenceFields.ActivatedAt);

            _taskCloner.Clone(_task);
        }

        [Test]
        [ExpectedException(typeof(EntityClonerException))]
        public void TestUnrelatedRelation()
        {
            _taskCloner.IgnoredRelations.Add(LicenceEntity.PrefetchPathLicensee);

            _taskCloner.Clone(_task);
        }

        [Test]
        [ExpectedException(typeof(EntityClonerException))]
        public void TestIntersectingFields()
        {
            _taskCloner.Ignored.Add(TaskFields.Title);

            _taskCloner.Clone(_task);
        }

        [Test]
        [ExpectedException(typeof(EntityClonerException))]
        public void TestIntersectingRelations()
        {
            _taskCloner.IgnoredRelations.Add(TaskEntity.PrefetchPathAssignedTo);

            _taskCloner.Clone(_task);
        }

        [Test]
        public void TestEqualityAxoimsRequiredForCloner()
        {
            EntityClonerField firstField = new EntityClonerField(TaskFields.Title);
            EntityClonerField secondField = new EntityClonerField(TaskFields.Title);
            EntityClonerField otherField = new EntityClonerField(TaskFields.Status);

            Assert.IsTrue(firstField.Equals(secondField));
            Assert.AreEqual(firstField.GetHashCode(), secondField.GetHashCode());

            Assert.IsTrue(!firstField.Equals(otherField));
        }

        [Test]
        public void TestNonTreeGraphs()
        {
            _task.ReplacedBy = _task;

            _taskCloner.Clone(_task);

            Assert.AreSame(_task, _task.ReplacedBy);
        }
    }
}
