using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.Identification;
using ObviousCode.Interlace.BitTunnelServer;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ExampleLibrary;

namespace BitTunnelServerExample.Controls
{
    public partial class AvailableFilesControl : UserControl, IServerControl
    {
        ServerInstance _server;
        ListViewHelper _helper;
        Dictionary<string, ConnectedClient> _clientDetails;

        public AvailableFilesControl()
        {
            InitializeComponent();
        
            _helper = new ListViewHelper(_availableFilesView, ListViewHelper.ComparisonKey.Hash);

            _clientDetails = new Dictionary<string, ConnectedClient>();
        }

        public AvailableFilesControl(ServerInstance server) : this()
        {            
            Server = server;
        }

        public ServerInstance Server
        {
            set
            {
                if (value == null) return;

                _server = value;               
                _server.ClientDisconnected += new EventHandler(_server_ClientDisconnected);                
                _server.ClientIdentified +=new EventHandler<IdentificationEventArgs>(_server_ClientIdentified);
                _server.AvailableFilesUpdated += new EventHandler<FileListModificationEventArgs>(_server_AvailableFilesUpdated);
            }
            get
            {
                return _server;
            }
        }

        void _server_AvailableFilesUpdated(object sender, FileListModificationEventArgs e)
        {
            BitTunnelServerProtocol server = sender as BitTunnelServerProtocol;

            foreach (FileModificationDescriptor modification in e.Modifications)
            {
                if (modification.Mode == FileModificationMode.New)
                {
                    AddModification(server, modification);
                }
                else
                {
                    RemoveModification(server, modification);
                }                
            }
        }

        private void AddModification(BitTunnelServerProtocol protocol, FileModificationDescriptor modification)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    AddModification(protocol, modification);
                });
            }
            else
            {
                FileDescriptor descriptor = modification.ToFileDescriptor();

                ListViewItem item = GetListViewItem(descriptor.Hash);
                string client = "";

                if (item == null)
                {
                    item = new ListViewItem(new string[]{
                        descriptor.FileName,
                        "",
                        string.Format("{0} kb", Math.Round((double)descriptor.Size / 1024, 2)),
                        modification.Hash
                    });
                    
                    item.Tag = new List<FileDescriptor>();                    

                    _availableFilesView.Items.Add(item);

                    client = protocol.ClientDetails.PublicName;

                    item.ImageIndex = 0;
                }

                (item.Tag as List<FileDescriptor>).Add(descriptor);
                SetClientText(item);                
            }
        }

        private void SetClientText(ListViewItem item)
        {
            List<FileDescriptor> files = item.Tag as List<FileDescriptor>;

            if (files == null) throw new InvalidOperationException("Tag on available server file not List of File Descriptors");

            StringBuilder clients = new StringBuilder();

            foreach (FileDescriptor file in files)
            {
                clients.AppendFormat("{0}{1}", clients.Length == 0 ? "" : ", ", _clientDetails[file.OriginId].PublicName);
            }

            item.SubItems[1].Text = clients.ToString();
        }

        private void RemoveModification(BitTunnelServerProtocol protocol, FileModificationDescriptor modification)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    RemoveModification(protocol, modification);
                });                
            }
            else
            {
                FileDescriptor file = modification.ToFileDescriptor();

                ListViewItem item = GetListViewItem(file.Hash);

                List<FileDescriptor> files = item.Tag as List<FileDescriptor>;

                if (files == null) throw new InvalidOperationException("Tag on available server file not List of File Descriptors");

                files.Remove(file);

                if (files.Count == 0)
                {
                    _availableFilesView.Items.Remove(item);
                }
                else
                {
                    item.Tag = files;

                    SetClientText(item);
                }
            }
        }

        private ListViewItem GetListViewItem(string hash)
        {
            foreach (ListViewItem item in _availableFilesView.Items)
            {
                if ((item.Tag as List<FileDescriptor>)[0].Hash == hash)
                {
                    return item;
                }
            }

            return null;
        }
        
        void _server_ClientDisconnected(object sender, EventArgs e)
        {
            BitTunnelServerProtocol disconnectingProtocol = sender as BitTunnelServerProtocol;

            _clientDetails.Remove(disconnectingProtocol.ClientDetails.InstanceId);
        }

        void _server_ClientIdentified(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.IdentificationEventArgs e)
        {
            _clientDetails[e.Client.InstanceId] = e.Client;
        }        

        #region IServerControl Members

        public Image MenuIcon
        {
            get { return null; }
        }

        public string MenuText
        {
            get { return "Available Files"; }
        }

        #endregion
    }
}
