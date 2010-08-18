using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ExampleLibrary.File;
using System.IO;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using System.Windows.Forms;

namespace BitTunnelExampleLibrary.File
{
    public class AvailableFile : FileWrapper
    {
        public enum FileState { Available, Downloading, Downloaded, Requesting, DownloadConfirmed, Failed }

        FileState _state;
        ClientInstance _client;
        FileInfo _downloadedFile;
        double _percent;

        public AvailableFile(ClientInstance client, FileDescriptor descriptor)
            : base(descriptor)
        {
            _state = FileState.Available;
            _client = client;
            _percent = 0d;
            _client.FileRequestResponseReceived += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileRequestResponseEventArgs>(_client_FileRequestResponseReceived);
            _client.FileTransferProgressed += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferEventArgs>(_client_FileTransferProgressed);
            _client.FileTransferCompleted += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferCompletedEventArgs>(_client_FileTransferCompleted);
            _client.FileTransferFailed += new EventHandler<ObviousCode.Interlace.BitTunnelLibrary.Events.FileDescriptorEventArgs>(_client_FileTransferFailed);
        }

        void _client_FileTransferFailed(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileDescriptorEventArgs e)
        {
            if (e.File.Hash != File.Hash) return;

            MessageBox.Show(string.Format("File transfer of {0} failed", e.File.FileName));
            StateEnum = FileState.Failed;
        }

        void _client_FileTransferCompleted(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferCompletedEventArgs e)
        {
            if (e.Hash != File.Hash) return;

            StateEnum = FileState.Downloaded;
           
            string destination = Path.Combine(_client.Settings.TransferPath.FullName, File.FileName);

            int token = 1;

            while (System.IO.File.Exists(destination))
            {                
                destination = string.Format("{0} ({1})", Path.Combine(_client.Settings.TransferPath.FullName, File.FileName), ++token);
            }

            try
            {
                if (System.IO.File.Exists(e.Location))
                {
                    System.IO.File.Move(e.Location, destination);
                }
            }
            catch(Exception ex)
            {

            }
        }

        void _client_FileTransferProgressed(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileTransferEventArgs e)
        {
            if (e.FileChunk.Header.Hash != File.Hash) return;

            StateEnum = FileState.Downloading;

            _percent = Math.Round((double) ((e.FileChunk.ChunkIndex + 1) * 100) / e.FileChunk.Header.ChunkCount, 2);
        }

        void _client_FileRequestResponseReceived(object sender, ObviousCode.Interlace.BitTunnelLibrary.Events.FileRequestResponseEventArgs e)
        {
            if (e.File.Hash != File.Hash) return;

            if (e.File == File)
            {
                if (e.Response == FileRequestMode.Available)
                {
                    if (_state == FileState.Requesting)
                    {
                        StateEnum = FileState.DownloadConfirmed;
                    }
                }
                else
                {
                    MessageBox.Show("Unable to retrieve file. It may have been removed, or is locked by its owner or the server.");
                }
            }
        }

        public string State
        {
            get
            {
                switch (_state)
                {
                    case FileState.Available:
                        return "Available";
                    case FileState.Downloaded:

                        if (_downloadedFile != null)
                        {
                            if (_downloadedFile.Exists)
                            {
                                return "Downloaded";
                            }
                        }

                        return "Available";

                    case FileState.Downloading:
                        return string.Format("Downloading ({0})%", ProgressPercent);
                    case FileState.Requesting:
                        return "Requesting File";
                    case FileState.DownloadConfirmed:
                        return "Waiting for File";
                    case FileState.Failed:
                        return "Failed"
;
                    default:
                        return "Unknown State";
                }
            }
        }

        private FileState StateEnum
        {
            set
            {
                _state = value;

                NotifyPropertyChanged("State");
            }
        }

        private double ProgressPercent
        {
            get
            {
                return _percent;
            }
        }

        public void SetStateToRequesting()
        {
            StateEnum = FileState.Requesting;
        }
    }
}