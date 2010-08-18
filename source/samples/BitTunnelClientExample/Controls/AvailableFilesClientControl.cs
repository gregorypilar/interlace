using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ExampleLibrary;
using BitTunnelExampleLibrary.File;

namespace BitTunnelClientExample.Controls
{
    public partial class AvailableFilesClientControl : UserControl, IClientTabControl
    {
        ClientInstance _client;
        ListViewHelper _helper;

        public AvailableFilesClientControl(ClientInstance client)
        {
            InitializeComponent();
            _helper = new ListViewHelper(_availableFilesView, ListViewHelper.ComparisonKey.Hash);
            Client = client;
            Client.ConnectionMade += new EventHandler(Client_ConnectionMade);
            Client.ConnectionTerminated += new EventHandler(Client_ConnectionTerminated);
            Client.LostConnection += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs>(Client_LostConnection);
        }
        
        #region IClientControl Members

        public ObviousCode.Interlace.BitTunnel.Connectivity.ClientInstance Client
        {
            get
            {
                return _client;
            }
            set
            {
                _client = value;

                _client.FileListUpdateReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileListModificationEventArgs>(_client_FileListUpdateReceived);
                _client.FullFileListReceived +=new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileListEventArgs>(_client_FullFileListReceived);
            }
        }

        void _client_FullFileListReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileListEventArgs e)
        {
            ReloadFullFileList(e.FileList);
        }

        private void ReloadFullFileList(IList<FileDescriptor> fileList)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                { ReloadFullFileList(fileList); });
            }
            else
            {
                _availableFilesView.Items.Clear();

                foreach (FileDescriptor file in fileList)
                {
                    AddFileToFileList(file);
                }
            }
        }

        void _client_FileListUpdateReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileListModificationEventArgs e)
        {
            foreach (FileModificationDescriptor modification in e.Modifications)
            {
                if (modification.Mode == FileModificationMode.New)
                {
                    AddFileToFileList(modification.ToFileDescriptor());
                }
                else if (modification.Mode == FileModificationMode.Renamed)
                {
                    RenameFile(modification.ToFileDescriptor());
                }
                else
                {
                    Invoke((MethodInvoker)delegate
                    {
                        _helper.RemoveFileFromListView(modification.Hash);
                    });
                }
            }
        }

        private void AddFileToFileList(FileDescriptor file)
        {
            Invoke((MethodInvoker)delegate
            {
                if (_helper.GetListViewItem(file.Hash) != null) return;

                AvailableFile availableFile = CreateAvailableFile(file);

                _helper.AddFileToList(
                    availableFile,
                        file.FileName,
                        GetSizeString(file.Size),                        
                        availableFile.State,
                        file.Hash
                    );
            });
        }

        private string GetSizeString(long sizeInBytes)
        {
            if (sizeInBytes < 1024)
            {
                return string.Format("{0} bytes", sizeInBytes);
            }
            else if (sizeInBytes < 1048576)
            {
                return string.Format("{0} KB", Math.Round((double)sizeInBytes / 1024, 2));
            }
            else if (sizeInBytes < 1073741824)
            {
                return string.Format("{0} MB", Math.Round((double)sizeInBytes / 1048576, 2));
            }
            else 
            {
                return string.Format("{0} GB", Math.Round((double)sizeInBytes / 1073741824, 2));
            }
        }

        private void RenameFile(FileDescriptor file)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)
                    delegate { RenameFile(file); }
                    );
            }
            else
            {
                ListViewItem item = _helper.GetListViewItem(file.Hash);

                item.Text = file.FileName;

                item.Tag = CreateAvailableFile(file);
            }
        }

        private AvailableFile CreateAvailableFile(FileDescriptor file)
        {
            AvailableFile availableFile = new AvailableFile(_client, file);

            availableFile.PropertyChanged += new PropertyChangedEventHandler(availableFile_PropertyChanged);

            return availableFile;
        }

        void availableFile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                SetViewState(sender as AvailableFile);
            }
        }

        private void SetViewState(AvailableFile availableFile)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { SetViewState(availableFile); });
            }
            else
            {
                ListViewItem item = _helper.GetListViewItem(availableFile.File.Hash);

                if (item == null) return;

                item.SubItems[2] = new ListViewItem.ListViewSubItem(item, availableFile.State);
            }
        }

        #endregion

        
        private void _availableFilesView_SelectedIndexChanged(object sender, EventArgs e)
        {
            _downloadButton.Enabled = _availableFilesView.SelectedItems.Count > 0;            
        }

        private void _refresh_Click(object sender, EventArgs e)
        {
            _client.RequestFullFileList();
        }

        #region IClientTabControl Members

        public string TabText
        {
            get { return "Available Files"; }
        }

        #endregion

        private void _downloadButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in _availableFilesView.SelectedItems)
            {
                if (!(item.Tag is AvailableFile)) throw new InvalidOperationException("Selected item tag is not Available File");

                AvailableFile file = item.Tag as AvailableFile;

                file.SetStateToRequesting();

                _client.RequestFile(file.File);
            }
        }

        //Maybe consider a Connection Changed event
        void Client_LostConnection(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.ExceptionEventArgs e)
        {
            EnableRefresh(false);            
        }

        void Client_ConnectionTerminated(object sender, EventArgs e)
        {
            EnableRefresh(false);
        }

        void Client_ConnectionMade(object sender, EventArgs e)
        {
            EnableRefresh(true);
        }

        private void EnableRefresh(bool enabled)
        {
            Invoke((MethodInvoker)delegate
            {
                _refresh.Enabled = enabled;
            });
        }

        private void _openTransferDirectory_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(_client.Settings.TransferPath.ToString());
        }


    }
}
