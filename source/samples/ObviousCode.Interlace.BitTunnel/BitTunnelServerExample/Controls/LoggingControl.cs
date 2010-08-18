using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelServer;
using ObviousCode.Interlace.BitTunnelLibrary.Identification;
using ObviousCode.Interlace.BitTunnelLibrary.File;

namespace BitTunnelServerExample
{
    public partial class LoggingControl : UserControl, IServerControl
    {
        ServerInstance _server;

        public LoggingControl()
        {
            InitializeComponent();
        }

        public LoggingControl(ServerInstance server, AppSettings settings) : this()
        {
            Settings = settings;
            Server = server;            
        }

        void server_ClientConnected(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.IdentificationEventArgs e)
        {
            Log(string.Format("Client {0} Connected from {1} ({2})", e.Client.PublicName, e.Client.IPAddress, e.Client.InstanceId));
        }

        void server_ClientIdentified(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.IdentificationEventArgs e)
        {
            Log(string.Format("Client {0} returned identification from {1} ({2})", e.Client.PublicName, e.Client.IPAddress, e.Client.InstanceId));
        }

        void server_MessageReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.MessageEventArgs e)
        {
            Log(string.Format("{0} Message received", e.Message.Key));
        }

        void server_LostConnection(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs e)
        {            
            Log(string.Format("Connection Lost: {0}", e.ThrownException.Message));
        }

        void server_ConnectionTerminated(object sender, EventArgs e)
        {
            Log("Connection Terminated gracefully");
        }

        void server_ConnectionMade(object sender, EventArgs e)
        {
            Log("Server Ready and waiting for connections ...");
        }

        void server_FileRequested(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileRequestEventArgs e)
        {
            Log(string.Format("File {0} requested ({1})", e.File.FileName, e.File.Hash));
        }

        private void Log(string text)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker) delegate
                {
                    Log(text);
                });
            }
            else
            {
                _log.Text += string.Format("{0} {1} {2} {3} {4}",
                               DateTime.Now.ToShortDateString(),
                               DateTime.Now.ToLongTimeString(), 
                               " ",
                               text,
                               "\r\n");

                _log.SelectionStart = _log.Text.Length - 1;

                _log.ScrollToCaret();
            }
        }
        public ServerInstance Server
        {
            get
            { 
                return _server; 
            }
            set
            {
                if (value == null) return;//will be sent null by InitializeComponent();

                _server = value;

                _server.FileRequested += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileRequestEventArgs>(server_FileRequested);
                _server.ConnectionMade += new EventHandler(server_ConnectionMade);
                _server.ConnectionTerminated += new EventHandler(server_ConnectionTerminated);
                _server.ClientDisconnected += new EventHandler(_server_ClientDisconnected);
                _server.LostConnection += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs>(server_LostConnection);
                _server.MessageReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.MessageEventArgs>(server_MessageReceived);
                _server.ClientConnected += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.IdentificationEventArgs>(server_ClientConnected);
                _server.ClientIdentified += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.IdentificationEventArgs>(server_ClientIdentified);
                _server.AvailableFilesUpdated += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileListModificationEventArgs>(server_AvailableFilesUpdated);
            }
        }

        void server_AvailableFilesUpdated(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileListModificationEventArgs e)
        {
            foreach (FileModificationDescriptor modification in e.Modifications)
            {
                FileDescriptor file = modification.ToFileDescriptor();
                Log(string.Format("The file \"{0}\" ({1}) on client {2} was {3}", file.FileName, file.Hash, modification.OriginId, modification.Mode == FileModificationMode.New ? "Added" : "Removed"));
            }
        }
        

        void _server_ClientDisconnected(object sender, EventArgs e)
        {
            ConnectedClient client = (sender as BitTunnelServerProtocol).ClientDetails;

            Log(string.Format("Client {0} Disconnected from {1} ({2})", client.PublicName, client.IPAddress, client.InstanceId));
        }
        public AppSettings Settings { get; set; }

        #region IServerControl Members

        public Image MenuIcon
        {
            get { return null; }
        }

        public string MenuText
        {
            get { return "Log"; }
        }

        #endregion
    }
}
