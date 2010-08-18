using System;
using System.Collections.Generic;
using Interlace.ReactorService;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Identification;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary.Protocols;
using ObviousCode.Interlace.BitTunnelLibrary.Services;

namespace ObviousCode.Interlace.BitTunnelClient
{
    public class BitTunnelClientService : BitTunnelService
    {
        FileRebuilderService _rebuilderService;

        public event EventHandler<FileListEventArgs> FullFileListReceived;
        public event EventHandler<FileListModificationEventArgs> FileListModificationsReceived;
        public event EventHandler<FileRequestEventArgs> FileRequestReceived;
        public event EventHandler<FileRequestResponseEventArgs> FileRequestResponseReceived;
        public event EventHandler<FileTransferEventArgs> FileTransferInitiated;
        public event EventHandler<FileTransferEventArgs> FileTransferProgressed;
        public event EventHandler<FileDescriptorEventArgs> FileTransferFailed;
        public event EventHandler<FileTransferCompletedEventArgs> FileTransferCompleted;
        string _clientName;

        IClientInstance _instance;

        Dictionary<string, FileDescriptor> _availableFiles;

        ServiceHost _host;
        
        public BitTunnelClientService(AppSettings settings, string clientName) : base(settings)
        {
            _availableFiles = new Dictionary<string, FileDescriptor>();

            _clientName = clientName;
        }

        public ConnectedClient ConnectionDetails
        {
            get
            {
                return ClientFactory.ConnectionDetails;
            }
        }

        #region IService Members        

        protected override void OnServiceOpen(IServiceHost host)
        {
            _rebuilderService = new FileRebuilderService(Settings);            

            _host = new ServiceHost();

            _host.AddService(_rebuilderService);

            _host.StartServiceHost();
            _host.OpenServices();
        }

        void _rebuilderService_NextChunkRequested(object sender, FileRequestEventArgs e)
        {
            ClientFactory.OnNextFileChunkRequested(e);
        }
        
        public override void Close(IServiceHost host)
        {            
            ClientFactory.LoseConnection();

            _host.CloseServices();
            _host.StopServiceHost();
        }
       
        #endregion

        public override IInstance Instance
        {
            get { return _instance; }
            set 
            {
                if (!(value is IClientInstance)) throw new InvalidOperationException();
                
                _instance = value as IClientInstance;
            }
        }
        
        protected override void OnLostConnection(Exception e)
        {            
        }

        protected override BitTunnelProtocolFactory CreateFactory()
        {
            BitTunnelClientProtocolFactory factory = new BitTunnelClientProtocolFactory(Settings, _clientName);

            factory.FullFileListReceived += new EventHandler<FileListEventArgs>(factory_FullFileListReceived);
            factory.FileListModificationsReceived += new EventHandler<FileListModificationEventArgs>(factory_FileListModificationsReceived);
            factory.FileRequestReceived += new EventHandler<FileRequestEventArgs>(factory_FileRequestReceived);
            factory.FileRequestResponseReceived += new EventHandler<FileRequestResponseEventArgs>(factory_FileRequestResponseReceived);

            factory.FileTransferInitiated += new EventHandler<FileTransferEventArgs>(factory_FileTransferInitiated);
            factory.FileTransferProgressed += new EventHandler<FileTransferEventArgs>(factory_FileTransferProgressed);
            factory.FileTransferFailed += new EventHandler<FileDescriptorEventArgs>(factory_FileTransferFailed);
            
            _rebuilderService.TransferCompleted += new EventHandler<FileTransferCompletedEventArgs>(_rebuilderService_TransferCompleted);
            _rebuilderService.NextChunkRequested += new EventHandler<FileRequestEventArgs>(_rebuilderService_NextChunkRequested);

            return factory;
        }

        void _rebuilderService_TransferCompleted(object sender, FileTransferCompletedEventArgs e)
        {
            if (FileTransferCompleted != null)
            {
                FileTransferCompleted(sender, e);
            }
        }

        void factory_FileTransferFailed(object sender, FileDescriptorEventArgs e)
        {
            throw new NotImplementedException();
        }

        void factory_FileTransferProgressed(object sender, FileTransferEventArgs e)
        {
            if (FileTransferProgressed != null)
            {
                FileTransferProgressed(sender, e);
            }

            _rebuilderService.ReceiveChunk(e.FileChunk);
        }

        void factory_FileTransferInitiated(object sender, FileTransferEventArgs e)
        {
            if (FileTransferInitiated != null)
            {
                FileTransferInitiated(sender, e);
            }
        }

        void factory_FileRequestReceived(object sender, FileRequestEventArgs e)
        {
            if (FileRequestReceived != null)
            {
                FileRequestReceived(sender, e);
            }
        }

        void factory_FileRequestResponseReceived(object sender, FileRequestResponseEventArgs e)
        {
            if (FileRequestResponseReceived!= null)
            {
                FileRequestResponseReceived(sender, e);
            }
        }        

        void factory_FullFileListReceived(object sender, FileListEventArgs e)
        {
            if (FullFileListReceived != null)
            {
                FullFileListReceived(sender, e);
            }
        }

        void factory_FileListModificationsReceived(object sender, FileListModificationEventArgs e)
        {
            if (FileListModificationsReceived != null)
            {
                FileListModificationsReceived(sender, e);
            }
        }

        protected override void ConnectReactor(IServiceHost host)
        {
            ClientFactory.ProtocolMadeConnection += new EventHandler(ClientFactory_ProtocolMadeConnection);
            
            host.Reactor.ConnectStream(Factory, Settings.ServerAddress, Settings.Port);                    
        }

        void ClientFactory_ProtocolMadeConnection(object sender, EventArgs e)
        {
            FireConnectionCompleted();
        }      

        private BitTunnelClientProtocolFactory ClientFactory
        {
            get
            {
                return Factory as BitTunnelClientProtocolFactory;
            }
        }

        public void SendFileListModifications(List<FileModificationDescriptor> modifiedFiles)
        {
            //We are simply sending notification of this to server. Server will respond back to all clients, 
            //at which time we will perform the necessary actions. 
            //
            //This will assist in keeping things in sync in case of a comms error of some sort 
            //(unlikely, unless the server goes down all together, in which case we have bigger problems, 
            //and keeping in sync will be irrelevant, be lets be elegant anyway)

            ClientFactory.SendFileListModifications(modifiedFiles);
        }

        public void RequestFile(FileDescriptor file)
        {
            ClientFactory.SendInitialFileRequest(file);
        }

        public void RequestFullFileList()
        {
            ClientFactory.RequestFullFileList();
        }

        protected override void OnMessageReceived(IMessage message)
        {
            
        }

        protected override void OnMessageSending(IMessage message)
        {
            
        }        
    }
}
