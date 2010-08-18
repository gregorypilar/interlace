using System;
using System.Collections.Generic;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.Exceptions;
using ObviousCode.Interlace.BitTunnelClient;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary.Identification;
using ObviousCode.Interlace.BitTunnelLibrary.Services;
using System.IO;

namespace ObviousCode.Interlace.BitTunnel.Connectivity
{
    public class ClientInstance : Instance, IClientInstance
    {
        public event EventHandler<FileRequestEventArgs> FileRequestReceived;
        
        public event EventHandler<FileListModificationEventArgs> FileListUpdateReceived;
        public event EventHandler<FileListEventArgs> FullFileListReceived;

        public event EventHandler<FileRequestResponseEventArgs> FileRequestResponseReceived;

        public event EventHandler<FileTransferEventArgs> FileTransferInitiated;
        public event EventHandler<FileTransferEventArgs> FileTransferProgressed;
        public event EventHandler<FileDescriptorEventArgs> FileTransferFailed;
        public event EventHandler<FileTransferCompletedEventArgs> FileTransferCompleted;

        FileDescriptorLookup _networkFiles;
        FileDescriptorLookup _localFiles;

        public ClientInstance(AppSettings settings)
            : this(settings, "")
        {
            
        }       

        public ClientInstance(AppSettings settings, string instanceName)
            : base(settings, instanceName, ConnectionType.Client)
        {
            _networkFiles = new FileDescriptorLookup(true);
            _localFiles = new FileDescriptorLookup(false);                     

            ClientService.FullFileListReceived += new EventHandler<FileListEventArgs>(Service_FullFileListReceived);
            ClientService.FileListModificationsReceived += new EventHandler<FileListModificationEventArgs>(Service_FileListModificationsReceived);
            ClientService.FileRequestReceived += new EventHandler<FileRequestEventArgs>(Service_FileRequestReceived);
            ClientService.FileRequestResponseReceived += new EventHandler<FileRequestResponseEventArgs>(Service_FileRequestResponseReceived);
            
            ClientService.FileTransferInitiated += new EventHandler<FileTransferEventArgs>(ClientService_FileTransferInitiated);
            ClientService.FileTransferCompleted += new EventHandler<FileTransferCompletedEventArgs>(ClientService_FileTransferCompleted);
            ClientService.FileTransferProgressed += new EventHandler<FileTransferEventArgs>(ClientService_FileTransferProgressed);
        }

        void ClientService_FileTransferProgressed(object sender, FileTransferEventArgs e)
        {
            if(FileTransferProgressed != null)
            {
                FileTransferProgressed(sender, e);
            }
        }

        void ClientService_FileTransferCompleted(object sender, FileTransferCompletedEventArgs e)
        {
            if (FileTransferCompleted != null)
            {
                FileTransferCompleted(sender, e);
            }
        }

        void ClientService_FileTransferInitiated(object sender, FileTransferEventArgs e)
        {
            if (FileTransferInitiated != null)
            {
                FileTransferInitiated(sender, e);
            }
        }

        /// <summary>
        /// Will assume file is allowed unless FileRequestReceived event is not handled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Service_FileRequestReceived(object sender, FileRequestEventArgs e)
        {            
             //Check first whether file actually is available
            if (!_localFiles.Contains(e.File)) 
            {
                e.Allow = false;
                return;
            }

            if (FileRequestReceived != null)
            {
                FileRequestReceived(sender, e);
            }
        }

        void Service_FileRequestResponseReceived(object sender, FileRequestResponseEventArgs e)
        {
            if (FileRequestResponseReceived != null)
            {
                FileRequestResponseReceived(sender, e);
            }
        }        

        void Service_FullFileListReceived(object sender, FileListEventArgs e)
        {
            _networkFiles = new FileDescriptorLookup(e.FileList, true);            

            if (FullFileListReceived != null)
            {
                FullFileListReceived(sender, e);
            }
        }

        void Service_FileListModificationsReceived(object sender, FileListModificationEventArgs e)
        {
            _networkFiles.UpdateFileList(e.Modifications);

            if (FileListUpdateReceived != null)
            {
                FileListUpdateReceived(sender, e);
            }
        }

        public void RequestFile(FileDescriptor file)
        {
            if (_localFiles.Contains(file))
            {
                if (FileTransferCompleted != null)
                {
                    FileTransferCompleted(this, new FileTransferCompletedEventArgs(file.Hash, _localFiles[file.Hash].FileFullName));
                }
            }
            else
            {
                (Connection.Service as BitTunnelClientService).RequestFile(file);
            }
        }        
 
        public void RequestFullFileList()
        {
            (Connection.Service as BitTunnelClientService).RequestFullFileList();
        }

        public ConnectedClient ConnectionDetails
        {
            get
            {
                if (!Connection.IsConnected)
                {
                    return null;
                }

                return ClientService.ConnectionDetails;
            }
        }
        
        public void AddFiles(IEnumerable<FileDescriptor> files)
        {            
            SendModifiedFiles(files, FileModificationMode.New);
        }

        public void RemoveFiles(IEnumerable<FileDescriptor> files)
        {
            SendModifiedFiles(files, FileModificationMode.Remove);
        }

        public void RenameFiles(IEnumerable<FileDescriptor> files)
        {
            SendModifiedFiles(files, FileModificationMode.Renamed);
        }

        private void SendModifiedFiles(IEnumerable<FileDescriptor> files, FileModificationMode fileModificationMode)
        {
            List<FileModificationDescriptor> modifiedFiles = new List<FileModificationDescriptor>();

            foreach (FileDescriptor descriptor in files)
            {
                descriptor.OriginId = ConnectionDetails.InstanceId;

                modifiedFiles.Add(new FileModificationDescriptor(descriptor, fileModificationMode));
            }

            ClientService.SendFileListModifications(modifiedFiles);

            _localFiles.UpdateFileList(modifiedFiles);
        }

        protected override void OnMessageReceived(IMessage message)
        {
            
        }

        protected override void OnConnectionLost(Exception e)
        {
            
        }

        protected override void OnDispose()
        {
            
        }

        private BitTunnelClientService ClientService
        {
            get
            {
                return Connection.Service as BitTunnelClientService;
            }
        }
        
        public FileDescriptorLookup AvailableFiles
        {
            get { return _networkFiles; }
        }

        public FileDescriptorLookup LocalFiles
        {
            get { return _localFiles; }
        }        
    }
}
