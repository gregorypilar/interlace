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

using EntrustDal;
using EntrustDal.DatabaseSpecific;
using EntrustDal.EntityClasses;
using EntrustDal.HelperClasses;

using MbUnit.Framework;

using SD.LLBLGen.Pro.ORMSupportClasses;

using Interlace.DatabaseManagement;
using Interlace.Utilities;

#endregion

namespace Interlace.Tests.Utilities
{
    [TestFixture]
    public class TestEnityGraphIsDirtyWatcher
    {
        EntityGraphIsDirtyWatcher _taskWatcher = new EntityGraphIsDirtyWatcher();
        EntityGraphIsDirtyWatcher _userWatcher = new EntityGraphIsDirtyWatcher();

        TaskEntity _task;
        UserEntity _user;

        DataAccessAdapter _adapter;

        [SetUp]
        public void SetUp()
        {
            SetUpDatabaseConnection();

            FetchPopulatedTaskFromDatabase();
            FetchPopulatedUserFromDatabase();

            _taskWatcher.BoundTo = _task;
            _userWatcher.BoundTo = _user;
        }

        [TearDown]
        public void TearDown()
        {
            _adapter.Dispose();
        }

        private void SetUpDatabaseConnection()
        {
            DatabaseConnectionString connectionString = new DatabaseConnectionString();
               
            connectionString.ServerName = "SERVER14";
            connectionString.DatabaseName = "Entrust";
            connectionString.Username = "tmsplus";
            connectionString.Password = "cool";
            connectionString.UseIntegratedAuthentication = false;

            _adapter = new DataAccessAdapter(connectionString.ToString());
        }

        [Test]
        public void TestBoundToChangedEvent()
        {
            Assert.IsFalse(_taskWatcher.IsDirty);

            _taskWatcher.BoundTo = null;

            Assert.IsFalse(_taskWatcher.IsDirty);

            _taskWatcher.BoundTo = _task;

            Assert.IsFalse(_taskWatcher.IsDirty);
        }

        [Test]
        public void TestIsDirtyChangedEvent()
        {
            Assert.IsFalse(_taskWatcher.IsDirty);

            _task.Title = "Testing";

            Assert.IsTrue(_taskWatcher.IsDirty);
        }

        [Test]
        public void TestIsDirtyChangedEventOnAddToRelationCollection()
        {
            Assert.IsFalse(_taskWatcher.IsDirty);

            AddNewRelatedEntityToRoot();
            
            Assert.IsTrue(_taskWatcher.IsDirty);
        }

        [Test]
        public void TestIsDirtyChangedEventOnModifyRelationCollectionEntity()
        {
            Assert.IsFalse(_taskWatcher.IsDirty);

            AddNewRelatedEntityToRoot();

            Assert.IsTrue(_taskWatcher.IsDirty);

            MakeRandomModificationToRelatedEntity();

            Assert.IsTrue(_taskWatcher.IsDirty);
        }

        [Test]
        public void TestIsDirtyChangedEventOnRemoveRelationCollectionEntity()
        {
            Assert.IsFalse(_taskWatcher.IsDirty);

            AddNewRelatedEntityToRoot();

            Assert.IsTrue(_taskWatcher.IsDirty);

            RemoveRandomRelatedEntity();

            Assert.IsTrue(_taskWatcher.IsDirty);
        }

        [Test]
        public void TestIsDirtyChangedEventOnRemoveRelationCollectionEntity_MultipleEntities()
        {
            Assert.IsFalse(_taskWatcher.IsDirty);

            for (int i = 0; i < 10; i++)
            {
                AddNewRelatedEntityToRoot();
            }

            Assert.IsTrue(_taskWatcher.IsDirty);

            RemoveRandomRelatedEntity();

            Assert.IsTrue(_taskWatcher.IsDirty);
        }

        [Test]
        public void TestRemovingOneToManyRelatedEntities()
        {
            Assert.IsFalse(_taskWatcher.IsDirty);

            _task.AssignedTo.Tasks.Remove(_task);

            Assert.IsTrue(_taskWatcher.IsDirty);
        }

        #region Test Helper Methods

        private void AddNewRelatedEntityToRoot()
        {
            _task.Entries.Add(CreateNewEntryEntity());
        }

        private EntryEntity CreateNewEntryEntity()
        {
            EntryEntity relatedEntity = new EntryEntity();

            return relatedEntity;
        }

        private void MakeRandomModificationToRelatedEntity()
        {
            GetRandomEntityFromRoot().Modifications = Guid.NewGuid().ToString();
        }

        private EntryEntity GetRandomEntityFromRoot()
        {
            if (_task.Entries.Count == 0)
                throw new Exception("No related entities added to root.");

            Random random = new Random();

            int index = random.Next(_task.Entries.Count);

            return GetEntityFromRoot(index);
        }

        private EntryEntity GetEntityFromRoot(int index)
        {
            return _task.Entries[index];
        }

        private void RemoveRandomRelatedEntity()
        {
            _task.Entries.Remove(GetRandomEntityFromRoot());
        }

        private TaskEntity CreateNewRootEntity()
        {
            return new TaskEntity();
        }

        private void FetchPopulatedTaskFromDatabase()
        {
            EntityCollection<TaskEntity> tasks = new EntityCollection<TaskEntity>();

            RelationPredicateBucket filter = new RelationPredicateBucket(TaskFields.AssignedToId != DBNull.Value);

            // Join to ensure that we have at least one entry.
            filter.Relations.Add(TaskEntity.Relations.EntryEntityUsingTaskId);

            PrefetchPath2 path = new PrefetchPath2(EntityType.TaskEntity);
            path.Add(TaskEntity.PrefetchPathAssignedTo);
            path.Add(TaskEntity.PrefetchPathEntries);

            _adapter.FetchEntityCollection(tasks, filter, 1, null, path);

            _task = tasks[0];
        }

        private void FetchPopulatedUserFromDatabase()
        {
            EntityCollection<UserEntity> users = new EntityCollection<UserEntity>();

            // Join to ensure they have a certificate and at least one task.
            RelationPredicateBucket filter = new RelationPredicateBucket();
            filter.Relations.Add(UserEntity.Relations.UserCertificateEntityUsingUserId);
            filter.Relations.Add(UserEntity.Relations.TaskEntityUsingAssignedToId);

            PrefetchPath2 path = new PrefetchPath2(EntityType.UserEntity);
            path.Add(UserEntity.PrefetchPathCertificate);
            path.Add(UserEntity.PrefetchPathTasks);

            _adapter.FetchEntityCollection(users, filter, 1, null, path);

            _user = users[0];
        }

        #endregion
    }
}
