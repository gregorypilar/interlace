using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ObviousCode.Interlace.BitTunnel.Connectivity;

namespace BitTunnelClientExample.Controls
{
    public partial class LoggingControl : UserControl, IClientTabControl
    {
        public LoggingControl()
        {
            InitializeComponent();
        }

        public LoggingControl(ClientInstance client) : this()
        {
            Client = client;
            client.FileListUpdateReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileListModificationEventArgs>(client_FileListUpdateReceived);
            client.FileRequestReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileRequestEventArgs>(client_FileRequestReceived);
            client.FileRequestResponseReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileRequestResponseEventArgs>(client_FileRequestResponseReceived);
            client.FileTransferCompleted += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferCompletedEventArgs>(client_FileTransferCompleted);
            client.FileTransferFailed += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileDescriptorEventArgs>(client_FileTransferFailed);
            client.FileTransferInitiated += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferEventArgs>(client_FileTransferInitiated);
            client.FileTransferProgressed += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferEventArgs>(client_FileTransferProgressed);
            client.FullFileListReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileListEventArgs>(client_FullFileListReceived);
            client.ConnectionMade += new EventHandler(client_ConnectionMade);
            client.ConnectionTerminated += new EventHandler(client_ConnectionTerminated);
            client.LostConnection += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs>(client_LostConnection);
            client.MessageReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.MessageEventArgs>(client_MessageReceived);
        }

        void client_MessageReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.MessageEventArgs e)
        {
            Log(string.Format("{0} Message received", e.Message.Key));
        }

        void client_LostConnection(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs e)
        {
            Log(string.Format("Connection Lost : {0}", e.ThrownException.Message));
        }

        void client_ConnectionTerminated(object sender, EventArgs e)
        {
            Log("Connection exited gracefully");
        }

        void client_ConnectionMade(object sender, EventArgs e)
        {
            Log("Connected to Server");
        }

        void client_FullFileListReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileListEventArgs e)
        {
            Log("Server file list received");
        }

        void client_FileTransferProgressed(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferEventArgs e)
        {
            Log(string.Format("File ({0}) chunk {1} of {2} received", e.FileChunk.Header.Hash, e.FileChunk.Header.ChunkIndex, e.FileChunk.Header.ChunkCount));
        }

        void client_FileTransferInitiated(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferEventArgs e)
        {
            Log(string.Format("File ({0}) transfer initiated", e.FileChunk.Header.Hash));
        }

        void client_FileTransferFailed(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileDescriptorEventArgs e)
        {
            Log(string.Format("File {0} ({1}) transfer failed", e.File.FileName, e.File.Hash));
        }

        void client_FileTransferCompleted(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferCompletedEventArgs e)
        {
            Log(string.Format("File ({0}) transfer completed", e.Hash));
        }

        void client_FileRequestResponseReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileRequestResponseEventArgs e)
        {
            Log(string.Format("File {0} ({1}) request response received : {2}", e.File.FileName, e.File.Hash, e.Response));
        }

        void client_FileRequestReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileRequestEventArgs e)
        {
            Log(string.Format("File {0} ({1}) request received", e.File.FileName, e.File.Hash));
        }

        void client_FileListUpdateReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileListModificationEventArgs e)
        {
            Log("Server file update received");
        }

        private void Log(string text)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
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

        #region IClientControl Members

        public ClientInstance Client { get; set; }
        
        public string TabText
        {
            get
            {
                return "Logs";
            }
            
        }

        #endregion
    }
}
