using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using System.IO;
using ObviousCode.Interlace.BitTunnelLibrary;
using System.Net;
using System.Threading;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Events;

namespace TestBitTunnel
{
    [TestFixture]
    public class FileListTests
    {
        AppSettings _settings = new AppSettings();

        FileInfo _existingFile1;
        FileInfo _existingFile2;
        FileInfo _existingFile3;

        bool _fileListReceived;
        bool _fileListUpdateReceived;

        FileDescriptor[] _existingFiles;

        [SetUp]
        public void TestSetup()
        {
            _fileListReceived = false;
            _fileListUpdateReceived = false;
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _settings.Port = 1234;
            _settings.ServerAddress = IPAddress.Parse("127.0.0.1");
            _settings.ClientConnectionTimeout = 1000;

            _existingFile1 = CreateNewFile();
            _existingFile2 = CreateNewFile();
            _existingFile3 = CreateNewFile();

            _existingFiles = new FileDescriptor[] { 
                        FileDescriptor.Create(_existingFile1),
                        FileDescriptor.Create(_existingFile2), 
                        FileDescriptor.Create(_existingFile3) };
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

            using (StreamWriter writer = file.CreateText())
            {
                for (int i = 0; i < 10; i++)
                {
                    writer.WriteLine(DateTime.Now.Ticks.ToString());
                    Thread.Sleep(10);
                }
            }

            return new FileInfo(file.FullName);
        }

        [Test]
        public void WhenFirstClientConnectsToServer_FileListReceivedEventShouldFire()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.FullFileListReceived += new EventHandler<FileListEventArgs>(client_FullFileListReceived);

                    client.Connect();
                    
                    DateTime start = DateTime.Now;
                    //Give it a second for the file list to be retrieved from server
                    while (!_fileListReceived && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                    Assert.IsTrue(_fileListReceived);
                }
            }
        }

        [Test]
        public void WhenOnlyClientConnectsToServer_ClientsFilesShouldBeEmpty()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.FullFileListReceived += new EventHandler<FileListEventArgs>(client_FullFileListReceived);

                    client.Connect();
                    
                    DateTime start = DateTime.Now;
                    //Give it a second for the file list to be retrieved from server
                    while (!_fileListReceived && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                    Assert.IsTrue(_fileListReceived);

                    Assert.AreEqual(0, client.AvailableFiles.GetCurrentUniqueFileList().Count());
                }
            }
        }

        [Test]
        public void WhenSecondClientConnectsToServerWithFiles_ClientFileCountShouldReflectFirstClientFileCount()
        {
            using(ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client1 = new ClientInstance(_settings))
                {
                    client1.FullFileListReceived += new EventHandler<FileListEventArgs>(client_FullFileListReceived);

                    client1.Connect();

                    client1.AddFiles(new FileDescriptor[] { 
                        FileDescriptor.Create(_existingFile1),
                        FileDescriptor.Create(_existingFile2), 
                        FileDescriptor.Create(_existingFile3) });

                    DateTime start = DateTime.Now;

                    while (!_fileListReceived && (DateTime.Now - start).TotalMilliseconds < 1000) ;
                    
                    _fileListReceived = false;

                    using (ClientInstance client2 = new ClientInstance(_settings))
                    {
                        client2.FullFileListReceived +=new EventHandler<FileListEventArgs>(client_FullFileListReceived);

                        client2.Connect();

                        while (!_fileListReceived && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                        Assert.IsTrue(_fileListReceived);

                        Assert.AreEqual(3, client2.AvailableFiles.GetCurrentUniqueFileList().Count());
                    }
                }
            }
        }

        [Test]
        public void WhenSecondClientConnectsToServerWithFiles_ClientFileCountShouldReflectFirstClientFiles()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client1 = new ClientInstance(_settings))
                {
                    client1.FullFileListReceived += new EventHandler<FileListEventArgs>(client_FullFileListReceived);

                    client1.Connect();                    

                    client1.AddFiles(_existingFiles);

                    DateTime start = DateTime.Now;

                    while (!_fileListReceived && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                    _fileListReceived = false;

                    using (ClientInstance client2 = new ClientInstance(_settings))
                    {
                        client2.FullFileListReceived += new EventHandler<FileListEventArgs>(client_FullFileListReceived);

                        client2.Connect();

                        while (!_fileListReceived && (DateTime.Now - start).TotalMilliseconds < 1000) ;

                        Assert.IsTrue(_fileListReceived);

                        Assert.IsTrue(client2.AvailableFiles.Contains(_existingFiles));
                        Assert.IsTrue(client2.AvailableFiles.Contains(client1.AvailableFiles.GetCurrentUniqueFileList().ToArray()));
                    }
                }
            }
        }

        [Test]
        public void WhenClientConnectsToServer_AddsFiles_ClientShouldFireFilesUpdateEvent()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    client.Connect();

                    client.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(_fileListUpdateReceived);
                }
            }
        }        

        [Test]
        public void WhenSecondClientConnectsToServer_AddsFiles_ClientShouldFireFilesUpdateEvent()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client1 = new ClientInstance(_settings))
                {
                    client1.FileListUpdateReceived +=new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    client1.Connect();

                    using (ClientInstance client2 = new ClientInstance(_settings))
                    {
                        client2.Connect();

                        client2.AddFiles(_existingFiles);

                        DateTime then = DateTime.Now;

                        while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListUpdateReceived);
                    }
                }
            }
        }

        [Test]
        public void WhenSecondClientConnectsToServer_SecondClientAddsFile_FirstClientFilesShouldReflectServerFiles()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    client.Connect();

                    using(ClientInstance client2 = new ClientInstance(_settings))
                    {
                        client2.Connect();

                        client2.AddFiles(_existingFiles);

                        DateTime then = DateTime.Now;

                        while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListUpdateReceived);
                        
                        Assert.IsTrue(client.AvailableFiles.Contains(_existingFiles));
                    }
                }
            }
        }        

        [Test]
        public void WhenSecondClientConnectsToServerWithFiles_SecondClientAddsDifferentFiles_FirstClientFilesShouldReflectServerFilesBeingASuperSetOfBothFileLists()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    client.Connect();

                    client.AddFiles(new FileDescriptor[] { FileDescriptor.Create(_existingFile1) });

                    DateTime then = DateTime.Now;

                    while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(_fileListUpdateReceived);
                    Assert.AreEqual(1, client.AvailableFiles.UniqueFileCount);
                    Assert.IsTrue(client.AvailableFiles.Contains(FileDescriptor.Create(_existingFile1)));

                    _fileListUpdateReceived = false;

                    using (ClientInstance client2 = new ClientInstance(_settings))
                    {
                        client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                        client2.Connect();

                        client2.AddFiles(new FileDescriptor[]
                            {
                                FileDescriptor.Create(_existingFile2),
                                FileDescriptor.Create(_existingFile3)
                            }
                            );

                        then = DateTime.Now;

                        while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListUpdateReceived);
                        Assert.AreEqual(3, client.AvailableFiles.UniqueFileCount);
                        Assert.IsTrue(client.AvailableFiles.Contains(_existingFiles));
                    }
                }
            }
        }

        [Test]
        public void WhenSecondClientConnectsToServerWithFiles_SecondClientAddsSomeOfTheSameFiles_FirstClientFilesShouldBeASingleInstanceOfAllThree()
        {
            using(ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using(ClientInstance client = new ClientInstance(_settings))
                {
                    client.FileListUpdateReceived +=new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    client.Connect();

                    client.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(_fileListUpdateReceived);
                    Assert.AreEqual(3, client.AvailableFiles.UniqueFileCount);
                    Assert.AreEqual(3, client.AvailableFiles.FileInstanceCount);
                    Assert.IsTrue(client.AvailableFiles.Contains(_existingFiles));

                    _fileListUpdateReceived = false;

                    using (ClientInstance client2 = new ClientInstance(_settings))
                    {

                        client2.Connect();

                        client2.AddFiles(new FileDescriptor[] {
                            FileDescriptor.Create(_existingFile1),
                            FileDescriptor.Create(_existingFile3)}
                            );

                        then = DateTime.Now;

                        while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListUpdateReceived);
                        Assert.AreEqual(3, client.AvailableFiles.UniqueFileCount);
                        Assert.AreEqual(3, client.AvailableFiles.FileInstanceCount);
                        Assert.IsTrue(client.AvailableFiles.Contains(_existingFiles));

                        Assert.AreEqual(1, client.AvailableFiles.CountOf(FileDescriptor.Create(_existingFile1).Hash));
                        Assert.AreEqual(1, client.AvailableFiles.CountOf(FileDescriptor.Create(_existingFile2)));
                        Assert.AreEqual(1, client.AvailableFiles.CountOf(FileDescriptor.Create(_existingFile3)));
                    }
                }
            }
        }

        [Test]
        public void WhenSecondClientConnectsToServerWithFiles_FirstClientRemovesFiles_SecondClientFilesShouldReflectNoServerFiles()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    client.Connect();

                    client.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(_fileListUpdateReceived);
                    Assert.AreEqual(3, client.AvailableFiles.UniqueFileCount);
                    Assert.AreEqual(3, client.AvailableFiles.FileInstanceCount);

                    _fileListUpdateReceived = false;

                    client.FileListUpdateReceived -= new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    using (ClientInstance client2 = new ClientInstance(_settings, "Client2"))
                    {

                        client2.FullFileListReceived +=new EventHandler<FileListEventArgs>(client_FullFileListReceived);
                        client2.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                        client2.Connect();

                        then = DateTime.Now;

                        while (!_fileListReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListReceived);
                        Assert.AreEqual(3, client2.AvailableFiles.UniqueFileCount);
                        Assert.AreEqual(3, client2.AvailableFiles.FileInstanceCount);
                        Assert.IsTrue(client2.AvailableFiles.Contains(_existingFiles));

                        _fileListUpdateReceived = false;

                        client.RemoveFiles(_existingFiles);

                        then = DateTime.Now;

                        while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListUpdateReceived);
                        Assert.AreEqual(0, client2.AvailableFiles.UniqueFileCount);
                    }
                }
            }
        }

        //Should Test (though fix is not implemented) for Client receiving remove messages BEFORE file list received - should buffer removes for n seconds

        [Test]
        public void WhenSecondClientTriesToRemoveFirstClientFiles_ShouldHaveNoEffect()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings, "Test Client 1"))
                {
                    client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    client.Connect();

                    client.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(_fileListUpdateReceived);
                    Assert.AreEqual(3, client.AvailableFiles.UniqueFileCount);

                    _fileListUpdateReceived = false;

                    client.FileListUpdateReceived -= new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    using (ClientInstance client2 = new ClientInstance(_settings, "Test Client 2"))
                    {
                        client2.FullFileListReceived +=new EventHandler<FileListEventArgs>(client_FullFileListReceived);
                        client2.FileListUpdateReceived +=new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                        client2.Connect();

                        then = DateTime.Now;

                        while (!_fileListReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListReceived);
                        Assert.AreEqual(3, client2.AvailableFiles.UniqueFileCount);

                        _fileListReceived = false;

                        client2.RemoveFiles(_existingFiles);

                        then = DateTime.Now;

                        while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListUpdateReceived);
                        Assert.AreEqual(3, client2.AvailableFiles.UniqueFileCount);
                        Assert.IsTrue(client2.AvailableFiles.Contains(_existingFiles));
                    }
                }
            }
        }

        [Test]
        public void WhenFirstClientConnectsToServer_AddsFiles_SecondClientAddsSameFiles_FirstClientRemovesOneFile_ClientsShouldHaveOneInstanceOfAllFiles()
        {
            using(ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using(ClientInstance client = new ClientInstance(_settings, "Client 1"))
                {
                    client.Connect();
                    
                    client.FileListUpdateReceived+=new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    client.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(_fileListUpdateReceived);

                    client.FileListUpdateReceived -= new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                    _fileListUpdateReceived = false;

                    using (ClientInstance client2 = new ClientInstance(_settings, "Client 2"))
                    {
                        client2.Connect();

                        client2.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdated);

                        client2.AddFiles(_existingFiles);

                        then = DateTime.Now;

                        while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListUpdateReceived);

                        _fileListUpdateReceived = false;

                        client.RemoveFiles(
                            new FileDescriptor[]
                            {
                                _existingFiles[1]
                            }
                            );

                        then = DateTime.Now;

                        while (!_fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(_fileListUpdateReceived);

                        Assert.AreEqual(3, client2.AvailableFiles.UniqueFileCount);
                        Assert.AreEqual(3, client2.AvailableFiles.FileInstanceCount);
                        Assert.AreEqual(1, client2.AvailableFiles.CountOf(FileDescriptor.Create(_existingFile1)));
                        Assert.AreEqual(1, client2.AvailableFiles.CountOf(FileDescriptor.Create(_existingFile2)));
                        Assert.AreEqual(1, client2.AvailableFiles.CountOf(FileDescriptor.Create(_existingFile3)));

                        //These should have been set by now, as client2 is sent file updates after client, however if race conditions still exist
                        //add a second event handler for the file received on the first client and a second boolean to check
                        Assert.AreEqual(3, client.AvailableFiles.UniqueFileCount);
                        Assert.AreEqual(3, client.AvailableFiles.FileInstanceCount);
                        Assert.AreEqual(1, client.AvailableFiles.CountOf(FileDescriptor.Create(_existingFile1)));
                        Assert.AreEqual(1, client.AvailableFiles.CountOf(FileDescriptor.Create(_existingFile2)));
                        Assert.AreEqual(1, client.AvailableFiles.CountOf(FileDescriptor.Create(_existingFile3)));
                    }
                }
            }
        }

        [Test]
        public void WhenFilesAddedLocally_ShouldBeAvailableInLocalFiles()
        {
            using(ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();

                    client.AddFiles(_existingFiles);

                    Assert.AreEqual(3, client.LocalFiles.UniqueFileCount);
                    Assert.AreEqual(3, client.LocalFiles.FileInstanceCount);
                }
            }
        }

        [Test]
        public void WhenFilesAddedLocally_AndRemovedLocally_ShouldBeRemovedFromLocalFiles()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();

                    client.AddFiles(_existingFiles);
                    client.RemoveFiles(_existingFiles);

                    Assert.AreEqual(0, client.LocalFiles.UniqueFileCount);
                    Assert.AreEqual(0, client.LocalFiles.FileInstanceCount);
                }                
            }
        }

        [Test]
        public void WhenFilesAddedLocally_AndSomeRemovedLocally_OnlyRemovedFilesShouldBeRemovedFromLocalFiles()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();

                    client.AddFiles(_existingFiles);
                    client.RemoveFiles(new FileDescriptor[]{_existingFiles[0], _existingFiles[2]} );

                    Assert.AreEqual(1, client.LocalFiles.UniqueFileCount);
                    Assert.AreEqual(1, client.LocalFiles.FileInstanceCount);
                    Assert.IsTrue(client.LocalFiles.Contains(_existingFiles[1]));
                }
            }
        }

        [Test]
        public void WhenFilesAddedLocally_AndRemovedRemotely_ShouldNotBeRemovedFromLocalFiles()
        {
            using(ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using(ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();

                    client.AddFiles(_existingFiles);

                    using (ClientInstance client2 = new ClientInstance(_settings))
                    {
                        client2.Connect();

                        client2.RemoveFiles(_existingFiles);
                    }

                    Assert.AreEqual(3, client.LocalFiles.UniqueFileCount);
                    Assert.AreEqual(3, client.LocalFiles.FileInstanceCount);
                }
            }
        }

        [Test]
        public void WhenFilesAddedLocally_Twice_ShouldOnlyBeReflectedOnceInLocalFiles()
        {
            using(ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();

                    client.AddFiles(_existingFiles);
                    client.AddFiles(_existingFiles);

                    Assert.AreEqual(3, client.LocalFiles.UniqueFileCount);
                    Assert.AreEqual(3, client.LocalFiles.FileInstanceCount);
                }
            }
        }

        [Test]
        public void WhenFilesAddedLocally_Twice_RemovedOnce_ShouldBeRemovedFromLocalFiles()
        {
            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    client.Connect();

                    client.AddFiles(_existingFiles);
                    client.AddFiles(_existingFiles);

                    client.RemoveFiles(_existingFiles);

                    Assert.AreEqual(0, client.LocalFiles.UniqueFileCount);
                    Assert.AreEqual(0, client.LocalFiles.FileInstanceCount);
                }
            }
        }

        void client_FullFileListReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileListEventArgs e)
        {
            _fileListReceived = true;
        }

        void client_FileListUpdated(object sender, FileListModificationEventArgs e)
        {
            _fileListUpdateReceived = true;
        }
    }
}
