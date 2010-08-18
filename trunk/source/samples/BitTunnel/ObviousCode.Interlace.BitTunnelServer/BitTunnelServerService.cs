using System;
using System.Collections.Generic;
using System.Linq;
using Interlace.ReactorCore;
using Interlace.ReactorService;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using ObviousCode.Interlace.BitTunnelLibrary.Protocols;
using ObviousCode.Interlace.BitTunnelLibrary.Services;

namespace ObviousCode.Interlace.BitTunnelServer
{
    public class BitTunnelServerService : BitTunnelService
    {
        public event EventHandler<IdentificationEventArgs> ClientConnected;
        public event EventHandler<IdentificationEventArgs> ClientIdentified;
        public event EventHandler<FileRequestEventArgs> FileRequested;
        public event EventHandler<FileListModificationEventArgs> AvailableFilesUpdated;

        public event EventHandler ClientDisconnected;

        AppSettings _settings;
        List<ConnectorHandle> _handles;

        string _serverName;

        List<FileServerNegotiator> _activeNegotiators;

        public BitTunnelServerService(AppSettings settings, string serverName)
            : base(settings)
        {
            _handles = new List<ConnectorHandle>();

            _serverName = serverName;
            _settings = settings;

            _activeNegotiators = new List<FileServerNegotiator>();
        }

        public override void Close(IServiceHost host)
        {
            foreach (ConnectorHandle handle in _handles)
            {
                handle.Close();
            }
        }

        protected override void OnLostConnection(Exception e)
        {

        }

        protected override BitTunnelProtocolFactory CreateFactory()
        {
            BitTunnelServerProtocolFactory factory = new BitTunnelServerProtocolFactory(this, _serverName);

            factory.ClientConnected += new EventHandler<IdentificationEventArgs>(factory_ClientConnected);
            factory.ClientIdentified += new EventHandler<IdentificationEventArgs>(factory_ClientIdentified);
            factory.ConnectionTerminated += new EventHandler(factory_ConnectionTerminated);
            factory.AvailableFilesUpdated += new EventHandler<FileListModificationEventArgs>(factory_AvailableFilesUpdated);
            return factory;
        }

        void factory_AvailableFilesUpdated(object sender, FileListModificationEventArgs e)
        {
            if (AvailableFilesUpdated != null)
            {
                AvailableFilesUpdated(sender, e);
            }
        }

        void factory_ClientIdentified(object sender, IdentificationEventArgs e)
        {
            if (ClientIdentified != null)
            {
                ClientIdentified(sender, e);
            }
        }

        void factory_ConnectionTerminated(object sender, EventArgs e)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(sender, e);
            }
        }

        protected override void ConnectReactor(IServiceHost host)
        {
            if (Settings.ServerIsRemote)
            {
                _handles.Add(host.Reactor.ListenStream(ServerFactory, Settings.Port, Settings.ServerAddress));
            }
            else
            {
                _handles.Add(host.Reactor.ListenStream(ServerFactory, Settings.Port));
            }

            FireConnectionCompleted();
        }

        protected override void OnMessageReceived(IMessage message)
        {

        }

        protected override void OnMessageSending(IMessage message)
        {
            
        }

        void factory_ClientConnected(object sender, IdentificationEventArgs e)
        {
            if (ClientConnected != null)
            {
                ClientConnected(sender, e);
            }
        }   

        private BitTunnelServerProtocolFactory ServerFactory
        {
            get
            {
                return Factory as BitTunnelServerProtocolFactory;
            }
        }

        public int ConnectionCount
        {
            get
            {
                return ServerFactory.ConnectionCount;
            }
        }

        public override IInstance Instance { get; set; }

        internal void RequestFile(FileDescriptor file, long chunkIndex, Action<FileDescriptor, FileRequestMode> sendResponseCallback, Action<FileDescriptor, FileChunkMessage> sendChunkCallback)
        {
            FileRequestEventArgs e = new FileRequestEventArgs(file);

            //This is giving a chance for the Server application to globally block a file - it has nothing to do with the client's desire to server the file
            if (FileRequested != null)
            {
                FileRequested(this, e);
            }

            if (!e.Allow)
            {
                sendResponseCallback(file, FileRequestMode.NotAvailable);
                return;
            }

            if (ServerFactory.Protocols.Count(p => p.Files.Contains(file)) == 0)
            {
                sendResponseCallback(file, FileRequestMode.NotAvailable);
                return;
            }
            
            //Negotiate File Transfer
            _activeNegotiators.Add(
                new FileServerNegotiator(ServerFactory, file, sendResponseCallback, sendChunkCallback));

            _activeNegotiators[_activeNegotiators.Count - 1].TimedOut += new EventHandler(FileServiceNegotiator_TimedOut);
            _activeNegotiators[_activeNegotiators.Count - 1].Settings = _settings;
            _activeNegotiators[_activeNegotiators.Count - 1].Negotiate(chunkIndex);
        }

        internal void FileChunkReceived(FileChunkMessage chunkMessage)
        {
            FileServerNegotiator negotiator = _activeNegotiators.Where(n => n.Id == chunkMessage.Header.Id).FirstOrDefault();

            if (negotiator == null)
            {
                //Implement as event?
                throw new InvalidOperationException("File Chunk received, though no negotiator was available to accept it");
            }

            negotiator.ChunkReceived(chunkMessage);
            negotiator.TimedOut-= new EventHandler(FileServiceNegotiator_TimedOut);
            _activeNegotiators.Remove(negotiator);
        }

        void FileServiceNegotiator_TimedOut(object sender, EventArgs e)
        {
            (sender as FileServerNegotiator).TimedOut -= new EventHandler(FileServiceNegotiator_TimedOut);
            _activeNegotiators.Remove(sender as FileServerNegotiator);
        }
    }
}