using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using System.IO;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using System.Threading;
using System.Net;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelUtilities;
using ObviousCode.Interlace.BitTunnelUtilities.Messages;

namespace TestBitTunnel
{
    [TestFixture]
    public class FileRequestTests
    {
        AppSettings _settings = new AppSettings();

        FileInfo _existingFile1;
        FileInfo _existingFile2;
        FileInfo _existingFile3;
        
        FileDescriptor[] _existingFiles;

        Random _random;
        
        const int FILESIZE = 10000000;        

        MessageLogger _logger;

        Action<FileTransferCompletedEventArgs> _fileTransferCompletedCallback;
        Action<FileRequestEventArgs> _fileRequestReceivedCallback;
        Action<FileRequestResponseEventArgs> _fileRequestResponseCallback;

        Action _fileTransferInitiatedCallback;        
        Action _transferProgressCallback;        
        Action _fileListUpdateCallback;
        Action _fullFileListReceivedCallback;
        

        [SetUp]
        public void TestSetup()
        {
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            _logger = new MessageLogger(new DirectoryInfo(@"C:\Logs"));

            _random = new Random(DateTime.Now.Millisecond);

            _settings.Port = 1234;
            _settings.ServerAddress = IPAddress.Parse("127.0.0.1");
            _settings.ClientConnectionTimeout = 1000;
            _settings.Logger = _logger;
            _settings.WorkingPath = new DirectoryInfo(@"C:\BitTunnelWorking");

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

            _logger.Flush();           
        }

        internal FileInfo CreateNewFile()
        {
            FileInfo file;

            do
            {
                file = new FileInfo(string.Format(@"C:\{0}.test", Guid.NewGuid().ToString().Replace("-", "")));
            }
            while (file.Exists); //highly unlikely

            using (FileStream stream = file.Create())
            {
                byte[] bytes = new byte[FILESIZE];
                _random.NextBytes(bytes);
                stream.Write(bytes, 0, bytes.Length);                
            }

            return new FileInfo(file.FullName);
        }        

        [Test]
        public void WhenFileIsRequestedFromClient_FileExists_FileInitiatedEventShouldFire()
        {
            bool fileListUpdateReceived = false;
            bool fullFileListReceived = false;
            bool fileTransferInitiatedEventReceived = false;

            _fileListUpdateCallback = new Action(delegate()
                {
                    fileListUpdateReceived = true;
                });

            _fullFileListReceivedCallback = new Action(delegate()
                {
                    fullFileListReceived = true;
                });

            _fileTransferInitiatedCallback = new Action(delegate()
                {
                    fileTransferInitiatedEventReceived = true;
                });

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance clientOwning = new ClientInstance(_settings, "Owning Client"))
                {                    
                    clientOwning.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);

                    clientOwning.Connect();

                    clientOwning.AddFiles(_existingFiles);

                    using (ClientInstance clientRequesting = new ClientInstance(_settings, "Requesting Client"))
                    {
                        clientRequesting.FullFileListReceived += new EventHandler<FileListEventArgs>(client_FullFileListReceived);
                        clientRequesting.FileTransferInitiated += new EventHandler<FileTransferEventArgs>(client_FileTransferInitiated);
                        clientRequesting.Connect();

                        DateTime then = DateTime.Now;

                        while (!fullFileListReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(fullFileListReceived);

                        clientRequesting.RequestFile(_existingFiles[1]);

                        then = DateTime.Now;

                        while (!fileTransferInitiatedEventReceived && (DateTime.Now - then).TotalSeconds < 60) ;

                        Assert.IsTrue(fileTransferInitiatedEventReceived);                        
                    }
                }
            }
        }

        [Test]
        public void WhenFileIsRequestedFromClient_FileExists_RequestingClientConnectsFirst_FileShouldBeServedInFull()
        {
            bool fileListUpdateReceived = false;
            bool fullFileListReceived = false;
            bool requestedFileReceived = false;

            FileInfo requestedFile = null;

            _fullFileListReceivedCallback = new Action(delegate()
                {
                    fullFileListReceived = true;
                });

            _fileListUpdateCallback = new Action(delegate()
            {
                fileListUpdateReceived = true;
            });
            
            _fileTransferCompletedCallback = new Action<FileTransferCompletedEventArgs>(delegate(FileTransferCompletedEventArgs args)
                {
                    requestedFile = new FileInfo(args.Location);
                    requestedFileReceived = true;
                });



            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance clientRequesting = new ClientInstance(_settings, "Requesting Client"))
                {
                    clientRequesting.FullFileListReceived += new EventHandler<FileListEventArgs>(client_FullFileListReceived);
                    clientRequesting.FileTransferCompleted += new EventHandler<FileTransferCompletedEventArgs>(client_FileTransferCompleted);
                    clientRequesting.Connect();

                    DateTime then = DateTime.Now;

                    while (!fullFileListReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(fullFileListReceived);

                    
                    using (ClientInstance clientOwning = new ClientInstance(_settings, "Owning Client"))
                    {
                        clientOwning.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);

                        clientOwning.Connect();

                        clientOwning.AddFiles(_existingFiles);                                                
                        
                        clientRequesting.RequestFile(_existingFiles[1]);

                        then = DateTime.Now;

                        while (!requestedFileReceived && (DateTime.Now - then).TotalMinutes < 10) ;

                        Assert.IsTrue(fileListUpdateReceived);
                        Assert.IsTrue(requestedFileReceived);
                        Assert.IsNotNull(requestedFile);
                        Assert.IsTrue(FilesIsEqualToRequestedFile(_existingFiles[1], requestedFile));
                    }
                }
            }
        }

        [Test]
        public void WhenFileIsRequestedFromClient_FileExists_RequestingClientConnectsSecond_FileShouldBeServedInFull()
        {
            bool fileListUpdateReceived = false;
            bool fullFileListReceived = false;
            bool requestedFileReceived = false;

            FileInfo requestedFile = null;

            _fullFileListReceivedCallback = new Action(delegate()
            {
                fullFileListReceived = true;
            });

            _fileListUpdateCallback = new Action(delegate()
            {
                fileListUpdateReceived = true;
            });

            _fileTransferCompletedCallback = new Action<FileTransferCompletedEventArgs>(delegate(FileTransferCompletedEventArgs args)
            {                
                requestedFile = new FileInfo(args.Location);
                requestedFileReceived = true;
            });


            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance clientOwning = new ClientInstance(_settings, "Owning Client"))
                {
                    clientOwning.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);

                    clientOwning.Connect();

                    clientOwning.AddFiles(_existingFiles);

                    using (ClientInstance clientRequesting = new ClientInstance(_settings, "Requesting Client"))
                    {
                        clientRequesting.FullFileListReceived += new EventHandler<FileListEventArgs>(client_FullFileListReceived);
                        clientRequesting.FileTransferCompleted += new EventHandler<FileTransferCompletedEventArgs>(client_FileTransferCompleted);
                        clientRequesting.Connect();

                        DateTime then = DateTime.Now;

                        while (!fullFileListReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(fullFileListReceived);

                        clientRequesting.RequestFile(_existingFiles[1]);

                        then = DateTime.Now;

                        while (!requestedFileReceived && (DateTime.Now - then).TotalMinutes < 10) ;

                        Assert.IsTrue(fileListUpdateReceived);
                        Assert.IsTrue(requestedFileReceived);
                        Assert.IsNotNull(requestedFile);
                        Assert.IsTrue(FilesIsEqualToRequestedFile(_existingFiles[1], requestedFile));
                    }
                }
            }
        }               

        [Test]
        public void WhenFileIsRequestedFromClient_NoFileExists_FailResponseIsReceived()
        {            
            bool fileRequestResponseReceived = false;            
            bool fileRequestFailed = false;
            string expectedHash = _existingFiles[0].Hash; ;

            _fileRequestReceivedCallback = new Action<FileRequestEventArgs>(delegate(FileRequestEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        e.Allow = true;
                    }
                });

            _fileRequestResponseCallback = new Action<FileRequestResponseEventArgs>(
                delegate(FileRequestResponseEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        if (e.Response == FileRequestMode.NotAvailable)
                        {
                            fileRequestFailed = true;
                            fileRequestResponseReceived = true;
                        }
                    }
                }
                );

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                server.FileRequested += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);

                using (ClientInstance client = new ClientInstance(_settings, "Client 1"))
                {                   
                    fileRequestFailed = false;                    

                    client.FileRequestResponseReceived +=new EventHandler<FileRequestResponseEventArgs>(client_FileRequestResponseReceived);

                    client.Connect();

                    client.RequestFile(_existingFiles[0]);

                    DateTime then = DateTime.Now;

                    while (!fileRequestResponseReceived && (DateTime.Now - then).TotalSeconds < 1) ;
                    
                    Assert.IsTrue(fileRequestResponseReceived);
                    Assert.IsTrue(fileRequestFailed);
                }
            }
        }

        [Test]
        public void WhenFileIsRequestedFromClient_ServerBlocksRequest_FailResponseIsReceived()
        {
            bool fileListUpdateReceived = false;
            bool fileRequestResponseReceived = false;
            bool fileRequestFailed = false;
            string expectedHash = _existingFiles[0].Hash;

            _fileListUpdateCallback = new Action(delegate()
                {
                    fileListUpdateReceived = true;
                });

            _fileRequestReceivedCallback = new Action<FileRequestEventArgs>(delegate(FileRequestEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        e.Allow = false;
                    }
                });

            _fileRequestResponseCallback = new Action<FileRequestResponseEventArgs>(delegate(FileRequestResponseEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {                        
                        if (e.Response == FileRequestMode.NotAvailable)
                        {
                            fileRequestFailed = true;
                            fileRequestResponseReceived = true;
                        }
                    }                    

                });

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();                

                server.FileRequested += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);

                using (ClientInstance client = new ClientInstance(_settings))
                {
                    fileListUpdateReceived = false;
                    
                    client.Connect();

                    client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);

                    client.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(fileListUpdateReceived);

                    using (ClientInstance client2 = new ClientInstance(_settings, "Client2"))
                    {
                        client2.FileRequestResponseReceived +=new EventHandler<FileRequestResponseEventArgs>(client_FileRequestResponseReceived);

                        client2.Connect();

                        client2.RequestFile(_existingFiles[0]);

                        then = DateTime.Now;

                        while (!fileRequestFailed && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(fileRequestResponseReceived);
                        Assert.IsTrue(fileRequestFailed);
                    }
                }
            }
        }

        [Test]
        public void WhenFileIsRequestedFromClient_FileOwningClientBlocksRequest_FailResponseIsReceived()
        {
            string expectedHash = _existingFiles[0].Hash;

            bool fileListUpdateReceived = false;            
            bool fileRequestFailed = false;
            bool fileRequestResponseReceived = false;

            _fileListUpdateCallback = new Action(delegate()
                {
                    fileListUpdateReceived = true;
                }                
                );

            _fileRequestResponseCallback = new Action<FileRequestResponseEventArgs>(delegate(FileRequestResponseEventArgs e)
            {
                if (e.File.Hash == expectedHash)
                {                    
                    if (e.Response == FileRequestMode.NotAvailable)
                    {
                        fileRequestFailed = true;
                        fileRequestResponseReceived = true;
                    }
                }
            });

            _fileRequestReceivedCallback = new Action<FileRequestEventArgs>(delegate(FileRequestEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        e.Allow = false;
                    }
                });

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings, "Client1"))
                {
                    client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);
                    client.FileRequestReceived += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);
                    client.Connect();

                    client.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(fileListUpdateReceived);

                    using (ClientInstance client2 = new ClientInstance(_settings, "Client2"))
                    {
                        client2.FileRequestResponseReceived += new EventHandler<FileRequestResponseEventArgs>(client_FileRequestResponseReceived);

                        client2.Connect();
                        
                        client2.RequestFile(_existingFiles[0]);

                        then = DateTime.Now;

                        //Allow for the server timeout of 1 second - wait 2 seconds here
                        while (!fileRequestFailed && (DateTime.Now - then).TotalSeconds < 2) ;

                        Assert.IsTrue(fileRequestResponseReceived);
                        Assert.IsTrue(fileRequestFailed);
                    }
                }
            }
        }

        [Test]
        public void WhenFileIsRequestedFromClient_RequestAllowed_AvailableResponseIsReceived()
        {
            bool fileListUpdateReceived = false;
            bool requestedFileAvailable = false;
            bool fileRequestResponseReceived = false;
            bool fileRequestReceived = false;
            string expectedHash = _existingFiles[0].Hash;

            _fileListUpdateCallback = new Action(delegate()
                {
                    fileListUpdateReceived = true;
                }
                );

            _fileRequestReceivedCallback = new Action<FileRequestEventArgs>(delegate(FileRequestEventArgs e)
            {
                if (e.File.Hash == expectedHash)
                {
                    e.Allow = true;
                    fileRequestReceived = true;                    
                }                
            });

            _fileRequestResponseCallback = new Action<FileRequestResponseEventArgs>(delegate(FileRequestResponseEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        if (e.Response == FileRequestMode.Available)
                        {                            
                            requestedFileAvailable = true;
                            fileRequestResponseReceived = true;
                        }                        
                    }
                });

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings, "Client1"))
                {
                    client.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);
                    client.FileRequestReceived += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);
                    
                    client.Connect();

                    client.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(fileListUpdateReceived);

                    using (ClientInstance client2 = new ClientInstance(_settings, "Client2"))
                    {                        
                        client2.Connect();
                        client2.FileRequestResponseReceived +=new EventHandler<FileRequestResponseEventArgs>(client_FileRequestResponseReceived);
                        
                        client2.RequestFile(_existingFiles[0]);

                        then = DateTime.Now;
                        
                        //Allow for the server timeout of 1 second - wait 2 seconds here
                        while (!fileRequestResponseReceived && (DateTime.Now - then).TotalSeconds < 60) ;

                        Assert.IsTrue(fileRequestReceived);
                        Assert.IsTrue(fileRequestResponseReceived);
                        Assert.IsTrue(requestedFileAvailable);                        
                    }
                }
            }
        }

        [Test]
        public void WhenFileOwnedByTwoClients_ClientOneRefusesRequest_FileShouldBeAvailableFromClientTwo()
        {
            bool allowRequest = false;
            bool fileListUpdateReceived = false;
            bool requestedFileAvailable = false;
            bool fileRequestResponseReceived = false;

            string expectedHash = _existingFiles[1].Hash;

            _fileListUpdateCallback = new Action(delegate()
                {
                    fileListUpdateReceived = true;
                });

            _fileRequestReceivedCallback = new Action<FileRequestEventArgs>(delegate(FileRequestEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        e.Allow = allowRequest;
                        allowRequest = !allowRequest;
                    }
                });

            _fileRequestResponseCallback = new Action<FileRequestResponseEventArgs>(delegate(FileRequestResponseEventArgs e)
                {
                    requestedFileAvailable = e.Response == FileRequestMode.Available;
                    fileRequestResponseReceived = true;
                });

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client1 = new ClientInstance(_settings, "Client 1"))
                {
                    client1.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);
                    client1.FileRequestReceived +=new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);

                    client1.Connect();

                    client1.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(fileListUpdateReceived);

                    fileListUpdateReceived = false;

                    using (ClientInstance client2 = new ClientInstance(_settings, "Client 2"))
                    {
                        client2.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);
                        client2.FileRequestReceived += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);

                        client2.Connect();

                        client2.AddFiles(_existingFiles);

                        then = DateTime.Now;

                        while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(fileListUpdateReceived);

                        fileListUpdateReceived = false;

                        using (ClientInstance client3 = new ClientInstance(_settings, "Client 3"))
                        {
                            client3.FileRequestResponseReceived += new EventHandler<FileRequestResponseEventArgs>(client_FileRequestResponseReceived);

                            client3.Connect();

                            client3.RequestFile(_existingFiles[1]);

                            then = DateTime.Now;

                            while (!fileRequestResponseReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                            Assert.IsTrue(fileRequestResponseReceived);
                            Assert.IsTrue(requestedFileAvailable);
                        }
                    }
                }
            }
        }

        [Test]
        public void WhenFileOwnedByTwoClients_ClientTwoRefusesRequest_FileShouldBeAvailableFromClientOne()
        {
            //Test intended to show that order of client connection not contributing factor in download rather that testing which client file was retrieved from
            bool fileListUpdateReceived = false;
            bool requestedFileAvailable = false;
            bool fileRequestResponseReceived = false;

            string expectedHash = _existingFiles[1].Hash;

            bool allowRequest = true;

            _fileRequestReceivedCallback = new Action<FileRequestEventArgs>(delegate(FileRequestEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        e.Allow = allowRequest;
                        allowRequest = !allowRequest;
                    }
                });

            _fileListUpdateCallback = new Action(delegate()
                {
                    fileListUpdateReceived = true;
                });

            _fileRequestResponseCallback = new Action<FileRequestResponseEventArgs>(delegate(FileRequestResponseEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        if (!requestedFileAvailable)
                        {
                            requestedFileAvailable = e.Response == FileRequestMode.Available;
                        }
                        fileRequestResponseReceived = true;
                    }
                });

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client1 = new ClientInstance(_settings, "Client 1"))
                {
                    client1.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);
                    client1.FileRequestReceived += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);

                    client1.Connect();

                    client1.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(fileListUpdateReceived);

                    fileListUpdateReceived = false;

                    using (ClientInstance client2 = new ClientInstance(_settings, "Client 2"))
                    {
                        client2.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);
                        client2.FileRequestReceived += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);

                        client2.Connect();

                        client2.AddFiles(_existingFiles);

                        then = DateTime.Now;

                        while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(fileListUpdateReceived);

                        fileListUpdateReceived = false;

                        using (ClientInstance client3 = new ClientInstance(_settings, "Client 3"))
                        {
                            client3.FileRequestResponseReceived += new EventHandler<FileRequestResponseEventArgs>(client_FileRequestResponseReceived);

                            client3.Connect();

                            client3.RequestFile(_existingFiles[1]);

                            then = DateTime.Now;

                            while (!fileRequestResponseReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                            Assert.IsTrue(fileRequestResponseReceived);
                            Assert.IsTrue(requestedFileAvailable);
                        }
                    }
                }
            }
        }

        [Test]
        public void WhenFileOwnedByTwoClients_BothClientsRefusesRequest_FileShouldBeNotAvailableForRetrieval()
        {
            //Test intended to show that order of client connection not contributing factor in download rather that testing which client file was retrieved from
            bool fileListUpdateReceived = false;
            bool requestedFileAvailable = false;
            bool fileRequestResponseReceived = false;            

            string expectedHash = _existingFiles[1].Hash;

            _fileListUpdateCallback = new Action(delegate()
                {
                    fileListUpdateReceived = true;
                });

            _fileRequestReceivedCallback = new Action<FileRequestEventArgs>(delegate(FileRequestEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        e.Allow = false;
                    }
                });

            _fileRequestResponseCallback = new Action<FileRequestResponseEventArgs>(delegate(FileRequestResponseEventArgs e)
                {
                    if (e.File.Hash == expectedHash)
                    {
                        if (!requestedFileAvailable)
                        {
                            requestedFileAvailable = e.Response == FileRequestMode.Available;
                            fileRequestResponseReceived = true;
                        }
                    }
                });

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance client1 = new ClientInstance(_settings, "Client 1"))
                {
                    client1.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);
                    client1.FileRequestReceived += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);

                    client1.Connect();

                    client1.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(fileListUpdateReceived);

                    fileListUpdateReceived = false;

                    using (ClientInstance client2 = new ClientInstance(_settings, "Client 2"))
                    {
                        client2.FileListUpdateReceived += new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);
                        client2.FileRequestReceived += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);

                        client2.Connect();

                        client2.AddFiles(_existingFiles);

                        then = DateTime.Now;

                        while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                        Assert.IsTrue(fileListUpdateReceived);

                        fileListUpdateReceived = false;

                        using (ClientInstance client3 = new ClientInstance(_settings, "Client 3"))
                        {
                            client3.FileRequestResponseReceived += new EventHandler<FileRequestResponseEventArgs>(client_FileRequestResponseReceived);

                            client3.Connect();

                            client3.RequestFile(_existingFiles[1]);

                            then = DateTime.Now;

                            while (!fileRequestResponseReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                            Assert.IsTrue(fileRequestResponseReceived);
                            Assert.IsFalse(requestedFileAvailable);
                        }
                    }
                }
            }
        }

        [Test]
        public void WhenFileIsRequested_FileProgressTransferEventsShouldFire()
        {
            int progressCalls = 0;
            bool fullFileListReceived = false;
            bool fileListUpdateReceived = false;
            bool fileTransferCompleted = false;
            string expectedHash = _existingFiles[2].Hash;

            int backup = _settings.FileChunkSize;
            _settings.FileChunkSize = 1024;
            
            _transferProgressCallback = new Action(delegate()
                {
                    progressCalls++;
                }
                );

            _fullFileListReceivedCallback = new Action(delegate()
                {
                    fullFileListReceived = true;
                });

            _fileListUpdateCallback = new Action(delegate()
                {
                    fileListUpdateReceived = true;
                });

            _fileTransferCompletedCallback = new Action<FileTransferCompletedEventArgs>(delegate(FileTransferCompletedEventArgs e)
                {
                    if (e.Hash == expectedHash)
                    {
                        fileTransferCompleted = true;
                    }
                });

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.Connect();

                using (ClientInstance owningClient = new ClientInstance(_settings, "Owning"))
                {
                    owningClient.FileListUpdateReceived +=new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);

                    owningClient.Connect();

                    owningClient.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 60) ;

                    Assert.IsTrue(fileListUpdateReceived);

                    using (ClientInstance requestingClient = new ClientInstance(_settings, "Requesting"))
                    {
                        progressCalls = 0;

                        fullFileListReceived = false;

                        requestingClient.FullFileListReceived += new EventHandler<FileListEventArgs>(client_FullFileListReceived);

                        requestingClient.FileTransferProgressed += new EventHandler<FileTransferEventArgs>(client_FileTransferProgressed);
                        requestingClient.FileTransferCompleted +=new EventHandler<FileTransferCompletedEventArgs>(client_FileTransferCompleted);
                        requestingClient.Connect();

                        then = DateTime.Now;
                        while (!fullFileListReceived && (DateTime.Now - then).TotalSeconds < 60) ;

                        Assert.IsTrue(fullFileListReceived);

                        then = DateTime.Now;

                        requestingClient.RequestFile(_existingFiles[2]);

                        while (!fileTransferCompleted && (DateTime.Now - then).TotalMinutes < 10) ;

                        Assert.IsTrue(fileTransferCompleted);
                        Assert.AreEqual(Math.Ceiling((double)FILESIZE / _settings.FileChunkSize), progressCalls);
                    }
                }
            }
            _settings.FileChunkSize = backup;
        }

        [Test]
        public void WhenFileIsRequestedFromClientThatContainsFile_FileShouldBeServedInstantlyWithoutCallingServer()
        {
            bool fileListUpdateReceived = false;
            bool fileRequestReceived = false;
            FileInfo requestedFile = null;
            string expectedHash = _existingFiles[2].Hash;

            _fileRequestReceivedCallback = new Action<FileRequestEventArgs>(delegate(FileRequestEventArgs e)
                {
                    fileRequestReceived = true;
                });

            _fileTransferCompletedCallback = new Action<FileTransferCompletedEventArgs>(delegate (FileTransferCompletedEventArgs e)
                {
                    if (expectedHash == e.Hash)
                    {
                        requestedFile = new FileInfo(e.Location);
                    }                    
                }
                );

            _fileListUpdateCallback = new Action(delegate()
                {
                    fileListUpdateReceived = true;
                }
                );

            using (ServerInstance server = new ServerInstance(_settings))
            {
                server.FileRequested += new EventHandler<FileRequestEventArgs>(event_FileRequestReceived);

                server.Connect();

                using (ClientInstance client = new ClientInstance(_settings))
                {   
                    client.FileTransferCompleted +=new EventHandler<FileTransferCompletedEventArgs>(client_FileTransferCompleted);
                    client.FileListUpdateReceived +=new EventHandler<FileListModificationEventArgs>(client_FileListUpdateReceived);

                    client.Connect();

                    client.AddFiles(_existingFiles);

                    DateTime then = DateTime.Now;

                    while (!fileListUpdateReceived && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsTrue(fileListUpdateReceived);

                    client.RequestFile(_existingFiles[2]);

                    then = DateTime.Now;

                    while (requestedFile == null && (DateTime.Now - then).TotalSeconds < 1) ;

                    Assert.IsNotNull(requestedFile);
                    Assert.IsFalse(fileRequestReceived);
                    Assert.AreEqual(_existingFiles[2].FileFullName, requestedFile.FullName);
                    FilesIsEqualToRequestedFile(_existingFiles[2], requestedFile);
                }
            }
        }

        [Test]
        public void WhenFileIsRequested_FileHashShouldBeUsedOnEachFileChunkSent()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void WhenFileIsRequestedFromClient_RequestAllowed_FileIsRemovedMidTransfer_TransferFailedResponseIsReceived()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void WhenFileIsRequestedFromClient_RequestAllowed_OwningClientDoesNotRespond_TransferFailedResponseIsReceivedDueToTimeout()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void WhenFileIsRequestedFromClient_RequestAllowed_RequestIsBlockedMidTransfer_TransferFailedResponseIsReceived()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void WhenFileIsRequestedFromClient_RequestAllowed_FileIsRemovedMidTransfer_FileExistsOnAnotherClient_TransferContinuesWithoutException()
        {
            throw new NotImplementedException();
        }
                       
        private bool FilesIsEqualToRequestedFile(FileDescriptor fileDescriptor, FileInfo requestedFile)
        {
            try
            {
                byte[] requestedFileStream;
                byte[] originalFileStream;

                using (FileStream stream = requestedFile.OpenRead())
                {
                    requestedFileStream = new byte[stream.Length];

                    stream.Read(requestedFileStream, 0, (int)stream.Length);
                }

                using (FileStream stream = new FileStream(fileDescriptor.FileFullName, FileMode.Open, FileAccess.Read))
                {
                    originalFileStream = new byte[stream.Length];

                    stream.Read(originalFileStream, 0, (int)stream.Length);
                }

                if (requestedFileStream.Length != originalFileStream.Length) return false;

                for (int i = 0; i < requestedFileStream.Length; i++)
                {
                    if (requestedFileStream[i] != originalFileStream[i]) return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void event_FileRequestReceived(object sender, FileRequestEventArgs e)
        {            
            if (_fileRequestReceivedCallback != null)
            {
                _fileRequestReceivedCallback(e);
            }
        }

        void client_FileTransferInitiated(object sender, FileTransferEventArgs e)
        {
            if (_fileTransferInitiatedCallback != null)
            {
                _fileTransferInitiatedCallback();
            }
        }        
        
        void client_FileRequestResponseReceived(object sender, FileRequestResponseEventArgs e)
        {            
            if (_fileRequestResponseCallback != null)
            {
                _fileRequestResponseCallback(e);
            }
        }

        void client_FileListUpdateReceived(object sender, FileListModificationEventArgs e)
        {
            if (_fileListUpdateCallback != null)
            {
                _fileListUpdateCallback();
            }
        }

        void client_FullFileListReceived(object sender, FileListEventArgs e)
        {
            if (_fullFileListReceivedCallback != null)
            {
                _fullFileListReceivedCallback();
            }
        }

        void client_FileTransferCompleted(object sender, FileTransferCompletedEventArgs e)
        {
            if (_fileTransferCompletedCallback != null)
            {
                _fileTransferCompletedCallback(e);
            }
        }

        void client_FileTransferProgressed(object sender, FileTransferEventArgs e)
        {
            if (_transferProgressCallback != null)
            {
                _transferProgressCallback();
            }
        }
    }
}
