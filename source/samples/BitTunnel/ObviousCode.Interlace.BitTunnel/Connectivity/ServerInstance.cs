using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelServer;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;

namespace ObviousCode.Interlace.BitTunnel.Connectivity
{
    public class ServerInstance : Instance, IServerInstance
    {
        public event EventHandler<IdentificationEventArgs> ClientConnected;
        public event EventHandler<IdentificationEventArgs> ClientIdentified;
        public event EventHandler<FileRequestEventArgs> FileRequested;
        public event EventHandler<FileListModificationEventArgs> AvailableFilesUpdated;
        public event EventHandler ClientDisconnected;

        public ServerInstance(AppSettings settings)
            : base(settings, ConnectionType.Server)
        {
            ServerService.FileRequested += new EventHandler<FileRequestEventArgs>(ServerService_FileRequested);
            ServerService.ClientConnected += new EventHandler<IdentificationEventArgs>(ServerService_ClientConnected);
            ServerService.ClientIdentified += new EventHandler<IdentificationEventArgs>(ServerService_ClientIdentified);
            ServerService.ClientDisconnected += new EventHandler(ServerService_ClientDisconnected);
            ServerService.AvailableFilesUpdated += new EventHandler<FileListModificationEventArgs>(ServerService_AvailableFilesUpdated);
        }

        void ServerService_AvailableFilesUpdated(object sender, FileListModificationEventArgs e)
        {
            if (AvailableFilesUpdated != null)
            {
                AvailableFilesUpdated(sender, e);
            }
        }

        void ServerService_ClientIdentified(object sender, IdentificationEventArgs e)
        {
            if (ClientIdentified!=null)
            {
                ClientIdentified(sender, e);
            }
        }

        void ServerService_ClientDisconnected(object sender, EventArgs e)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(sender, e);
            }
        }

        void ServerService_ClientConnected(object sender, IdentificationEventArgs e)
        {
            if (ClientConnected != null)
            {
                ClientConnected(sender, e);
            }
        }


        void ServerService_FileRequested(object sender, FileRequestEventArgs e)
        {            
            if (FileRequested != null)
            {
                FileRequested(sender, e);
            }
        }

        public int ConnectionCount
        {
            get
            {
                return ServerService.ConnectionCount;
            }
        }

        private BitTunnelServerService ServerService
        {
            get
            {
                return Connection.Service as BitTunnelServerService;
            }
        }

        protected override void OnMessageReceived(ObviousCode.Interlace.BitTunnelLibrary.Interfaces.IMessage message)
        {
            
        }

        protected override void OnConnectionLost(Exception e)
        {
            
        }

        protected override void OnDispose()
        {
            
        }
    }
}
