using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using ObviousCode.Interlace.BitTunnelLibrary;
using System.Net;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using System.Threading;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using System.IO;
using System.Security.Cryptography;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.MessageInstances;

namespace TestBitTunnel
{   
    [TestFixture]
    public class FileModificationMessageTests
    {
        IMessage _serverMessage;
        IMessage _clientMessage;

        AppSettings _settings = new AppSettings();

        FileInfo _existingFile1;
        FileInfo _existingFile2;
        FileInfo _existingFile3;

        FileModificationMessage _serverModificationMessage;

        [SetUp]
        public void TestSetup()
        {
            _serverModificationMessage = null;
            _serverMessage = null;
        }

        [TestFixtureSetUp]
        public void SetUp()
        {
            _settings.Port = 1234;
            _settings.ServerAddress = IPAddress.Parse("127.0.0.1");
            _settings.ClientConnectionTimeout = 1000;

            _existingFile1 = CreateNewFile();
            _existingFile2 = CreateNewFile();
            _existingFile3 = CreateNewFile();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _existingFile1.Delete();
            _existingFile2.Delete();
            _existingFile3.Delete();
        }

        internal FileInfo CreateNewFile()
        {
            FileInfo file;

            do
            {
                file = new FileInfo(string.Format(@"C:\{0}.test", Guid.NewGuid().ToString().Replace("-", "")));
            }
            while (file.Exists); //highly unlikely

            using(StreamWriter writer = file.CreateText())
            {
                for (int i = 0; i < 10; i++)
                {
                    writer.WriteLine(DateTime.Now.Ticks.ToString());
                    Thread.Sleep(10);
                }
            }

            return new FileInfo(file.FullName);
        }

        #region Test FileName Modification Comms
        [Test]
        public void WhenFileModificationMessages_ModeNew_AreSentByClient_TheyShouldBeFullyReceivedByServer()
        {
            _serverMessage = null;

            FileDescriptor[] descriptors = null;

            string originId = "Not Set";

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.MessageReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.MessageEventArgs>(server_MessageReceived);

                server.Connect();

                while (!server.IsConnected) ;

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();

                    originId = client.ConnectionDetails.InstanceId;
                    
                    descriptors = new List<FileDescriptor>()
                    {
                        FileDescriptor.Create(_existingFile1, true),
                        FileDescriptor.Create(_existingFile2, true),
                        FileDescriptor.Create(_existingFile3, true)
                    }.ToArray();

                    _serverModificationMessage = null;

                    client.AddFiles(descriptors);

                    DateTime then = DateTime.Now;

                    while (_serverModificationMessage == null && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsNotNull(_serverModificationMessage);

                    Assert.AreEqual(descriptors[0].FileFullName, _serverModificationMessage.Modifications[0].FileFullName);
                    Assert.AreEqual(descriptors[1].FileFullName, _serverModificationMessage.Modifications[1].FileFullName);
                    Assert.AreEqual(descriptors[2].FileFullName, _serverModificationMessage.Modifications[2].FileFullName);

                    //Assert.AreEqual(descriptors[0].FileId, _serverModificationMessage.Modifications[0].FileId);
                    //Assert.AreEqual(descriptors[1].FileId, _serverModificationMessage.Modifications[1].FileId);
                    //Assert.AreEqual(descriptors[2].FileId, _serverModificationMessage.Modifications[2].FileId);

                    Assert.AreEqual(originId, _serverModificationMessage.Modifications[0].OriginId);
                    Assert.AreEqual(originId, _serverModificationMessage.Modifications[1].OriginId);
                    Assert.AreEqual(originId, _serverModificationMessage.Modifications[2].OriginId);

                    Assert.AreEqual(FileModificationMode.New, _serverModificationMessage.Modifications[0].Mode);
                    Assert.AreEqual(FileModificationMode.New, _serverModificationMessage.Modifications[1].Mode);
                    Assert.AreEqual(FileModificationMode.New, _serverModificationMessage.Modifications[2].Mode);
                }
            }            
        }

        [Test]
        public void WhenFileModificationMessages_ModeRemoved_AreSentByClient_TheyShouldBeFullyReceivedByServer()
        {            
            _serverMessage = null;

            FileDescriptor[] descriptors = null;

            string originId = "Not Set";

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.MessageReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.MessageEventArgs>(server_MessageReceived);

                server.Connect();

                while (!server.IsConnected) ;

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();

                    originId = client.ConnectionDetails.InstanceId;

                    descriptors = new List<FileDescriptor>()
                    {
                        FileDescriptor.Create(_existingFile1, true),
                        FileDescriptor.Create(_existingFile2, true),
                        FileDescriptor.Create(_existingFile3, true)
                    }.ToArray();

                    _serverModificationMessage = null;

                    client.RemoveFiles(descriptors);

                    DateTime then = DateTime.Now;

                    while (_serverModificationMessage == null && (DateTime.Now - then).TotalSeconds < 1) ;
                    
                    Assert.IsNotNull(_serverModificationMessage);

                    Assert.AreEqual(descriptors[0].FileFullName, _serverModificationMessage.Modifications[0].FileFullName);
                    Assert.AreEqual(descriptors[1].FileFullName, _serverModificationMessage.Modifications[1].FileFullName);
                    Assert.AreEqual(descriptors[2].FileFullName, _serverModificationMessage.Modifications[2].FileFullName);

                    //Assert.AreEqual(descriptors[0].FileId, _serverModificationMessage.Modifications[0].FileId);
                    //Assert.AreEqual(descriptors[1].FileId, _serverModificationMessage.Modifications[1].FileId);
                    //Assert.AreEqual(descriptors[2].FileId, _serverModificationMessage.Modifications[2].FileId);

                    Assert.AreEqual(originId, _serverModificationMessage.Modifications[0].OriginId);
                    Assert.AreEqual(originId, _serverModificationMessage.Modifications[1].OriginId);
                    Assert.AreEqual(originId, _serverModificationMessage.Modifications[2].OriginId);

                    Assert.AreEqual(FileModificationMode.Remove, _serverModificationMessage.Modifications[0].Mode);
                    Assert.AreEqual(FileModificationMode.Remove, _serverModificationMessage.Modifications[1].Mode);
                    Assert.AreEqual(FileModificationMode.Remove, _serverModificationMessage.Modifications[2].Mode);
                }
            }                        
        }
        
        #endregion

        #region Test FileName Hashing

        #region Not Exists
        [Test]
        public void WhenFileDescriptorIsCreatedForNonExistingFile_FileHashShouldBeNull()
        {
            FileDescriptor descriptor = FileDescriptor.Create(@"C:\NothingToSeeHere.txt", true);

            Assert.IsNull(descriptor.Hash);
        }

        [Test]
        public void WhenFileDescriptorIsCreatedForNonExistingFile_ExistsPropertyShouldBeFalse()
        {
            FileDescriptor descriptor = FileDescriptor.Create(@"C:\NothingToSeeHere.txt", true);

            Assert.IsFalse(descriptor.Exists);
        }

        [Test]
        public void WhenFileModificationDescriptorIsCreatedForNonExistingFile_FileHashShouldBeNull()
        {
            FileDescriptor descriptor = FileDescriptor.Create(@"C:\NothingToSeeHere.txt", true);
            FileModificationDescriptor modification = new FileModificationDescriptor(descriptor, FileModificationMode.Remove);

            Assert.IsNull(modification.Hash);
        }
        
        
        [Test]
        public void WhenFileModificationDescriptorIsCreatedForNonExistingFile_HashGenerated_ExistsPropertyShouldBeFalse()
        {
            FileDescriptor descriptor = FileDescriptor.Create(@"C:\NothingToSeeHere.txt", true);

            FileModificationDescriptor modification = new FileModificationDescriptor(descriptor, FileModificationMode.New);

            Assert.IsNull(descriptor.Hash);
        }

        [Test]
        public void WhenFileModificationDescriptorIsCreatedForNonExistingFile_NoHashGenerated_ExistsPropertyShouldBeFalse()
        {
            FileDescriptor descriptor = FileDescriptor.Create(@"C:\NothingToSeeHere.txt", false);

            FileModificationDescriptor modification = new FileModificationDescriptor(descriptor, FileModificationMode.New);

            Assert.IsNull(descriptor.Hash);
        }

        #endregion 
        
        #region Exists

        [Test]
        public void WhenFileDescriptorIsCreatedForExistingFile_HashGenerated_FileHashShouldNotBeNull()
        {
            FileDescriptor descriptor = FileDescriptor.Create(_existingFile1.FullName, true);

            Assert.IsNotNull(descriptor.Hash);
        }

        [Test]
        public void WhenFileDescriptorIsCreatedForExistingFile_NoHashGenerated_FileHashShouldBeNull()
        {
            FileDescriptor descriptor = FileDescriptor.Create(_existingFile1.FullName, false);

            Assert.IsNull(descriptor.Hash);
        }

        [Test]
        public void WhenFileDescriptorIsCreatedForExistingFile_HashGeneratedSeparately_HashShouldNotBeNull()
        {
            FileDescriptor descriptor = FileDescriptor.Create(_existingFile1.FullName, false);

            descriptor.GenerateHash();

            Assert.IsNotNull(descriptor.Hash);
        }

        
        [Test]
        public void WhenFileDescriptorIsCreatedForExistingFile_HashGenerated_ExistsPropertyShouldBeTrue()
        {
            FileDescriptor descriptor = FileDescriptor.Create(_existingFile1.FullName, true);

            Assert.IsTrue(descriptor.Exists);
        }

        [Test]
        public void WhenFileDescriptorIsCreatedForExistingFile_HashNotGenerated_ExistsPropertyShouldBeTrue()
        {
            FileDescriptor descriptor = FileDescriptor.Create(_existingFile1.FullName, true);

            Assert.IsTrue(descriptor.Exists);
        }
        
        [Test]
        public void WhenFileModificationDescriptorIsCreatedForExistingFile_FileDescriptorParameterWithHash_FileHashShouldNotBeNull()
        {
            FileDescriptor descriptor = FileDescriptor.Create(_existingFile3.FullName, true);

            FileModificationDescriptor modification = new FileModificationDescriptor(descriptor, FileModificationMode.New);

            Assert.IsNotNull(descriptor.Hash);
        }

        [Test]
        public void WhenFileModificationDescriptorIsCreatedForExistingFile_FileDescriptorParameterWithoutHash_FileHashShouldBeNull()
        {
            FileDescriptor descriptor = FileDescriptor.Create(_existingFile3.FullName, false);

            FileModificationDescriptor modification = new FileModificationDescriptor(descriptor, FileModificationMode.New);

            Assert.IsNull(descriptor.Hash);
        }
        
        [Test]
        public void WhenFileModificationDescriptorIsCreatedForExistingFile_FileDescriptorParameter_ExistsPropertyShouldNotBeFalse()
        {
            FileDescriptor descriptor = FileDescriptor.Create(_existingFile1.FullName, true);

            FileModificationDescriptor modification = new FileModificationDescriptor(descriptor, FileModificationMode.New);

            Assert.IsNotNull(descriptor.Hash);
        }

        //[Test]
        public void Test()
        {
            string file = @"C:\users\john\Downloads\FATHER_TED.ISO";
            int i=0;
            while (i < 1)
            {
                DateTime before = DateTime.Now;

                CreateMD5Hash(file);

                TimeSpan afterMD5 = DateTime.Now - before;
                before = DateTime.Now;

                CreateSHA1Hash(file);

                TimeSpan afterSHA1 = DateTime.Now - before;

                before = DateTime.Now;

                CreateSHA256Hash(file);

                TimeSpan afterSHA256 = DateTime.Now - before;
                Console.WriteLine("{0}----------", i);
                Console.WriteLine("MD5 = {0}", afterMD5.ToString());
                Console.WriteLine("SHA1 = {0}", afterSHA1.ToString());
                Console.WriteLine("SHA256 = {0}", afterSHA256.ToString());
            }
        }

        private static string CreateMD5Hash(string fileName)
        {
            FileInfo file = new FileInfo(fileName);

            if (file.Exists)
            {
                using (FileStream stream = file.OpenRead())
                {
                    using (MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider())
                    {
                        return BitConverter.ToString(
                            hasher.ComputeHash(stream)
                            );
                    }
                }
            }

            return null;
        }

        private static string CreateSHA1Hash(string fileName)
        {
            FileInfo file = new FileInfo(fileName);

            if (file.Exists)
            {
                using (FileStream stream = file.OpenRead())
                {
                    using (SHA1CryptoServiceProvider hasher = new SHA1CryptoServiceProvider())
                    {
                        return BitConverter.ToString(
                            hasher.ComputeHash(stream)
                            );
                    }
                }
            }

            return null;
        }

        private static string CreateSHA256Hash(string fileName)
        {
            FileInfo file = new FileInfo(fileName);

            if (file.Exists)
            {
                using (FileStream stream = file.OpenRead())
                {
                    using (SHA256CryptoServiceProvider hasher = new SHA256CryptoServiceProvider())
                    {
                        return BitConverter.ToString(
                            hasher.ComputeHash(stream)
                            );
                    }
                }
            }

            return null;
        }

        #endregion

        #endregion

        #region Test File Descriptor

        [Test]
        public void TestHashSpeed()
        {
            
        }

        [Test]
        public void WhenFileDescriptorCreatedWithFilePath_FileSimpleNameIsSetProperly()
        {
            FileDescriptor descriptor = FileDescriptor.Create(@"C:\DirectoryLevel1\DirectoryLevel2\MyFile.ext", true);

            Assert.AreEqual("MyFile.ext", descriptor.FileName);
        }

        #endregion

        [Test]
        public void WhenFirstClientConnectsToServer_ClientReceivesEmptyFileListMessage()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings, "Test Client"))
                {
                    client.MessageReceived += new EventHandler<MessageEventArgs>(client_MessageReceived);

                    client.Connect();

                    DateTime start = DateTime.Now;

                    while ((_clientMessage == null || _clientMessage.Key != MessageKeys.FileList) && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                    Assert.IsNotNull(_clientMessage);
                    Assert.AreEqual(MessageKeys.FileList, _clientMessage.Key);
                    Assert.AreEqual(0, (_clientMessage as FileListMessage).FileList.Count);
                }
            }
        }

        [Test]
        public void WhenFirstClientConnectsToServer_SendsNew_NonExisting_Files_SecondClientConnects_SecondClientShouldReceiveEmptytFileListMessage()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings, "Test Client 1"))
                {
                    client.MessageReceived += new EventHandler<MessageEventArgs>(client_MessageReceived);

                    client.Connect();

                    DateTime start = DateTime.Now;

                    while ((_clientMessage == null || _clientMessage.Key != MessageKeys.FileList) && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                    Assert.IsNotNull(_clientMessage);
                    Assert.AreEqual(MessageKeys.FileList, _clientMessage.Key);
                    Assert.AreEqual(0, (_clientMessage as FileListMessage).FileList.Count);

                    _clientMessage = null;

                    using (ClientInstance client2 = new ClientInstance(_settings, "Test Client 2"))
                    {
                        client2.MessageReceived += new EventHandler<MessageEventArgs>(client_MessageReceived);

                        client2.Connect();

                        FileDescriptor[] descriptors = new List<FileDescriptor>()
                        {
                            FileDescriptor.Create(@"C:\DoesNotExist.txt", true),
                            FileDescriptor.Create(@"C:\CompletelyFake.txt", true),
                            FileDescriptor.Create(@"C:\PureHumbug.txt", true)
                        }.ToArray();

                        client.AddFiles(descriptors);

                        start = DateTime.Now;

                        while ((_clientMessage == null || _clientMessage.Key != MessageKeys.FileListModifications) && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                        Assert.IsNotNull(_clientMessage as FileModificationMessage);
                        Assert.AreEqual(MessageKeys.FileListModifications, _clientMessage.Key);
                        Assert.AreEqual(0, (_clientMessage as FileModificationMessage).Modifications.Count);

                        _clientMessage = null;

                        client2.MessageReceived -= new EventHandler<MessageEventArgs>(client_MessageReceived);
                        client.MessageReceived += new EventHandler<MessageEventArgs>(client_MessageReceived);

                        descriptors = new List<FileDescriptor>()
                        {
                            FileDescriptor.Create(@"C:\DoesNotExist.txt", true),
                            FileDescriptor.Create(@"C:\CompletelyFake.txt", true),
                            FileDescriptor.Create(@"C:\PureHumbug.txt", true)
                        }.ToArray();

                        client.AddFiles(descriptors);

                        start = DateTime.Now;

                        while ((_clientMessage == null || _clientMessage.Key != MessageKeys.FileListModifications) && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                        Assert.IsNotNull(_clientMessage as FileModificationMessage);
                        Assert.AreEqual(MessageKeys.FileListModifications, _clientMessage.Key);
                        Assert.AreEqual(0, (_clientMessage as FileModificationMessage).Modifications.Count);

                    }
                }
            }
        }

        [Test]
        public void WhenFirstClientConnectsToServer_SendsNew_Existing_Files_SecondClientConnects_SecondClientShouldReceiveCorrectFileListMessage()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings, "Test Client 1"))
                {
                    _clientMessage = null;

                    client.MessageReceived += new EventHandler<MessageEventArgs>(client_MessageReceived);

                    client.Connect();
                    
                    DateTime start = DateTime.Now;

                    while ((_clientMessage == null || _clientMessage.Key != MessageKeys.FileList) && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                    Assert.IsNotNull(_clientMessage);
                    Assert.AreEqual(MessageKeys.FileList, _clientMessage.Key);
                    Assert.AreEqual(0, (_clientMessage as FileListMessage).FileList.Count);

                    client.MessageReceived -= new EventHandler<MessageEventArgs>(client_MessageReceived);

                    _clientMessage = null;

                    using (ClientInstance client2 = new ClientInstance(_settings, "Test Client 2"))
                    {
                        client.MessageReceived += new EventHandler<MessageEventArgs>(client_MessageReceived);

                        client2.Connect();

                        FileDescriptor[] descriptors = new List<FileDescriptor>()
                        {
                            FileDescriptor.Create(_existingFile1.FullName, true),
                            FileDescriptor.Create(_existingFile2.FullName, true),
                            FileDescriptor.Create(_existingFile3.FullName, true)
                        }.ToArray();

                        client2.AddFiles(descriptors);

                        start = DateTime.Now;

                        //Wait a second for the network activity
                        while ((_clientMessage == null || _clientMessage.Key != MessageKeys.FileListModifications) && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                        Assert.IsNotNull(_clientMessage as FileModificationMessage);
                        Assert.AreEqual(MessageKeys.FileListModifications, _clientMessage.Key);
                        Assert.AreEqual(3, (_clientMessage as FileModificationMessage).Modifications.Count);

                        //Side effect of both being on same server?
                        Assert.IsTrue((_clientMessage as FileModificationMessage).Modifications[0].Exists);
                        Assert.IsTrue((_clientMessage as FileModificationMessage).Modifications[1].Exists);
                        Assert.IsTrue((_clientMessage as FileModificationMessage).Modifications[2].Exists);

                        Assert.AreEqual(_existingFile1.FullName, (_clientMessage as FileModificationMessage).Modifications[0].FileFullName);
                        Assert.AreEqual(_existingFile2.FullName, (_clientMessage as FileModificationMessage).Modifications[1].FileFullName);
                        Assert.AreEqual(_existingFile3.FullName, (_clientMessage as FileModificationMessage).Modifications[2].FileFullName);

                        //Assert.AreEqual(descriptors[0].FileId, (_clientMessage as FileModificationMessage).Modifications[0].FileId);
                        //Assert.AreEqual(descriptors[1].FileId, (_clientMessage as FileModificationMessage).Modifications[1].FileId);
                        //Assert.AreEqual(descriptors[2].FileId, (_clientMessage as FileModificationMessage).Modifications[2].FileId);

                        Assert.AreEqual(descriptors[0].Hash, (_clientMessage as FileModificationMessage).Modifications[0].Hash);
                        Assert.AreEqual(descriptors[1].Hash, (_clientMessage as FileModificationMessage).Modifications[1].Hash);
                        Assert.AreEqual(descriptors[2].Hash, (_clientMessage as FileModificationMessage).Modifications[2].Hash);

                        Assert.AreEqual(FileModificationMode.New, (_clientMessage as FileModificationMessage).Modifications[0].Mode);
                        Assert.AreEqual(FileModificationMode.New, (_clientMessage as FileModificationMessage).Modifications[1].Mode);
                        Assert.AreEqual(FileModificationMode.New, (_clientMessage as FileModificationMessage).Modifications[2].Mode);

                        Assert.AreEqual(descriptors[0].OriginId, (_clientMessage as FileModificationMessage).Modifications[0].OriginId);
                        Assert.AreEqual(descriptors[1].OriginId, (_clientMessage as FileModificationMessage).Modifications[1].OriginId);
                        Assert.AreEqual(descriptors[2].OriginId, (_clientMessage as FileModificationMessage).Modifications[2].OriginId);
                    }
                }
            }
        }
        
        void client_MessageReceived(object sender, MessageEventArgs e)
        {
            _clientMessage = e.Message;          
        }

        void server_MessageReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.MessageEventArgs e)
        {
            _serverMessage = e.Message;            
            if (_serverMessage is FileModificationMessage)
            {
                _serverModificationMessage = _serverMessage as FileModificationMessage;
            }
        }
    }
}
