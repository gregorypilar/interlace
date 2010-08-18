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
using ObviousCode.Interlace.BitTunnelLibrary.File;
using System.IO;
using ExampleLibrary;
using BitTunnelExampleLibrary.File;

namespace BitTunnelClientExample.Controls
{
    public partial class SharedFilesControl : UserControl, IClientTabControl
    {
        ClientInstance _client;
        ListViewHelper _helper;
        List<FileSystemWatcher> _watchers;

        public SharedFilesControl(ClientInstance client)
        {
            InitializeComponent();
            Client = client;

            _helper = new ListViewHelper(_sharedFilesView, ListViewHelper.ComparisonKey.Hash);
            _watchers = new List<FileSystemWatcher>();
        }

        #region IClientTabControl Members

        public string TabText
        {
            get 
            {
                return "Locally Shared Files";
            }
        }

        
        public ObviousCode.Interlace.BitTunnel.Connectivity.ClientInstance Client
        {
            get
            {
                return _client;
            }
            set
            {
                _client = value;
                
                _client.FileListUpdateReceived +=new EventHandler<FileListModificationEventArgs>(_client_FileListUpdateReceived);
                _client.FullFileListReceived += new EventHandler<FileListEventArgs>(_client_FullFileListReceived);
                _client.ConnectionTerminated += new EventHandler(_client_ConnectionTerminated);
                _client.LostConnection += new EventHandler<ExceptionEventArgs>(_client_LostConnection);
            }
        }

        void _client_LostConnection(object sender, ExceptionEventArgs e)
        {
            EnableToolbar(false);                        
        }

        void _client_ConnectionTerminated(object sender, EventArgs e)
        {
            EnableToolbar(false);
        }

        void _client_FullFileListReceived(object sender, FileListEventArgs e)
        {
            EnableToolbar(true);
        }

        private void EnableToolbar(bool enable)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    EnableToolbar(enable);
                });
            }
            else
            {
                _tools.Enabled = true;
            }
        }

        void _client_FileListUpdateReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileListModificationEventArgs e)
        {
            foreach (FileModificationDescriptor file in e.Modifications)
            {
                if (file.OriginId != _client.ConnectionDetails.InstanceId) continue;

                if (file.Mode == FileModificationMode.New)
                {
                    if (file.OriginId != _client.ConnectionDetails.InstanceId) continue;

                    SetFileAsAvailable(file.Hash);
                } 
                else if (file.Mode == FileModificationMode.Renamed)
                {
                    RenameFile(file.ToFileDescriptor());
                }
            }
        }

        private void RenameFile(FileDescriptor file)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)
                    delegate{RenameFile(file);}
                    );
            }
            else
            {
                ListViewItem item = _helper.GetListViewItem(file.Hash);

                item.Text = file.FileName;

                item.Tag = CreateSharedFile(file);
            }
        }

        
        private void SetFileAsAvailable(string hash)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    SetFileAsAvailable(hash);
                });
            }
            else
            {                 
                ListViewItem item = _helper.GetListViewItem(hash);

                if (item == null)
                {
                    MessageBox.Show(string.Format("Warning, locally available file (hash: {0}) not in list", hash));
                }

                (item.Tag as SharedFile).SetFileAvailable();
            }
        }

        private void AddFileToList(FileDescriptor file)
        {
            Invoke((MethodInvoker)delegate
            {
                if (_helper.GetListViewItem(file.FileFullName, ListViewHelper.ComparisonKey.FileFullName) != null) return;

                _helper.AddFileToList(CreateSharedFile(file),
                    file.FileName,
                    file.FileFullName,
                    string.Format("{0} kb", Math.Round((double)(file.Size / 1024), 2)),
                    ""                    
                    );
            });            
        }        

        private SharedFile CreateSharedFile(FileDescriptor fileDescriptor)
        {
            SharedFile shared = new SharedFile(fileDescriptor);

            shared.PropertyChanged += new PropertyChangedEventHandler(shared_PropertyChanged);

            return shared;
        }

        void shared_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "State")
            {
                SetViewState(sender as SharedFile);                                
            }
        }

        private void SetViewState(SharedFile sharedFile)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { SetViewState(sharedFile); });
            }
            else
            {
                ListViewItem item = _helper.GetListViewItem(sharedFile.File.Hash);

                if (item == null) return;

                item.SubItems[3] = new ListViewItem.ListViewSubItem(item, sharedFile.State.ToString());            
            }
        }        


        #endregion

        private void _addFilesToolStripButton_Click(object sender, EventArgs e)
        {
            if (_fileDialog.ShowDialog() == DialogResult.OK)
            {
                List<FileDescriptor> files = new List<FileDescriptor>();

                foreach (string file in _fileDialog.FileNames)
                {
                    if (_helper.GetListViewItem(file, ListViewHelper.ComparisonKey.FileFullName) != null) continue;

                    ShareFile(file);
                }
            }
        }

        private void ShareFile(string file)
        {
            FileDescriptor descriptor = FileDescriptor.Create(file, false);
            AddFileToList(descriptor);

            descriptor.HashGenerationCompleted += new EventHandler(descriptor_HashGenerationCompleted);

            descriptor.GenerateHash();
        }

        void descriptor_HashGenerationCompleted(object sender, EventArgs e)
        {
            FileDescriptor descriptor = sender as FileDescriptor;

            _client.AddFiles(new FileDescriptor[] { descriptor });
        }

        private void _sharedFilesView_SelectedIndexChanged(object sender, EventArgs e)
        {
            _removeSelectedFiles.Enabled = _sharedFilesView.SelectedItems.Count > 0;
            _removeSelectedFiles.Text = string.Format("Remove File{0}", _sharedFilesView.SelectedItems.Count > 1 ? "s" : "");
        }

        private void  _removeSelectedFiles_Click(object sender, EventArgs e)
        {
            foreach(ListViewItem item in _sharedFilesView.SelectedItems)
            {
                RemoveFile(item.Tag as SharedFile);
            }
        }

        private void RemoveFile(SharedFile sharedFile)
        {
            try
            {
                sharedFile.SetFileAsRemoving();

                _client.RemoveFiles(new FileDescriptor[] { sharedFile.File });

                Invoke((MethodInvoker)delegate { _helper.RemoveFileFromListView(sharedFile.File.Hash); });

            }
            catch(NotImplementedException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void _addFolderToolstripButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                foreach (FileInfo file in new DirectoryInfo(fbd.SelectedPath).GetFiles())
                {
                    if (((int)file.Attributes & (int)FileAttributes.System) != 0) continue;

                    ShareFile(file.FullName);
                }

                _watchers.Add(GetFileSystemWatcher(fbd.SelectedPath));                
            }
        }

        private FileSystemWatcher GetFileSystemWatcher(string path)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(path);
            
            watcher.Created += new FileSystemEventHandler(watcher_Created);
            watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);

            return watcher;
        }

        void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            
        }

        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            
        }   
    }
}
