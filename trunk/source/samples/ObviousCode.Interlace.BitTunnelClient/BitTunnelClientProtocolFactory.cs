using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Identification;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.MessageInstances;
using ObviousCode.Interlace.BitTunnelLibrary.Protocols;

namespace ObviousCode.Interlace.BitTunnelClient
{
    internal class BitTunnelClientProtocolFactory : BitTunnelProtocolFactory
    {
        internal event EventHandler ProtocolMadeConnection;
        internal event EventHandler<FileListEventArgs> FullFileListReceived;
        internal event EventHandler<FileListModificationEventArgs> FileListModificationsReceived;        
        internal event EventHandler<FileTransferEventArgs> FileTransferInitiated;
        internal event EventHandler<FileTransferEventArgs> FileTransferProgressed;
        internal event EventHandler<FileDescriptorEventArgs> FileTransferFailed;
        internal event EventHandler<FileTransferCompletedEventArgs> FileTransferCompleted;
        internal event EventHandler<FileRequestEventArgs> FileRequestReceived;
        internal event EventHandler<FileRequestEventArgs> FileNextChunkRequestReceived;
        internal event EventHandler<FileRequestResponseEventArgs> FileRequestResponseReceived;

        ConnectedClient _identification;
        
        string _clientName;

        private Dictionary<MessageKeys, Action<IMessage>> _messageHandlers;
        private Dictionary<string, FileDescriptor> _requestedFiles;

        public BitTunnelClientProtocolFactory(AppSettings settings, string clientName) : base(settings)
        {
            _clientName = clientName;

            _requestedFiles = new Dictionary<string, FileDescriptor>();

            LoadMessageHandlers();
        }

        private void LoadMessageHandlers()
        {
            _messageHandlers = new Dictionary<MessageKeys, Action<IMessage>>();

            _messageHandlers[MessageKeys.Identification] = HandleIdentificationRequest;
            _messageHandlers[MessageKeys.FileList] = HandleFullFileListMessageReceived;
            _messageHandlers[MessageKeys.FileListModifications] = HandleFileListModificationsMessageReceived;
            _messageHandlers[MessageKeys.FileRequest] = HandleFileRequestMessage;
            _messageHandlers[MessageKeys.FileChunk] = HandleFileChunkMessage;            
        }

        public ConnectedClient ConnectionDetails
        {
            get
            {
                return _identification;
            }
        }

        #region IProtocolFactory Members                

        public override void StartedConnecting()
        {
            Debug.WriteLine("Connecting ... - Should handle with NLog");
        }

        #endregion

        internal void SendFileListModifications(IList<FileModificationDescriptor> modifications)
        {
            using (FileModificationMessage message = new FileModificationMessage())
            {
                foreach (FileModificationDescriptor descriptor in modifications)
                {
                    ClientProtocol.AddValueToMessage(message, descriptor);
                }

                ClientProtocol.SendMessage(message);
            }
        }

        internal void HandleIdentificationRequest(IMessage receivedMessage)
        {
            _identification = (receivedMessage as IdentificationMessage).Client;

            using (IdentificationMessage message = new IdentificationMessage())
            {
                _identification.PublicName = _clientName;

                ClientProtocol.AddValueToMessage(message, _identification);

                ClientProtocol.SendMessage(message);
                
                if (ProtocolMadeConnection != null)
                {
                    ProtocolMadeConnection(this, EventArgs.Empty);
                }
            
            }            
        }        

        internal void HandleFullFileListMessageReceived(IMessage fileListMessage)
        {            
            if (FullFileListReceived != null)
            {
                FullFileListReceived(this, new FileListEventArgs((fileListMessage as FileListMessage).FileList));
            }
        }

        internal void HandleFileListModificationsMessageReceived(IMessage fileListModificationMessage)
        {
            if (FileListModificationsReceived != null)
            {
                FileListModificationsReceived(this, new FileListModificationEventArgs((fileListModificationMessage as FileModificationMessage).Modifications));
            }
        }

        internal void HandleFileChunkMessage(IMessage message)
        {
            FileChunkMessage chunk = message as FileChunkMessage;

            if (chunk.IsStartChunk && FileTransferInitiated != null)
            {
                FileTransferInitiated(this, new FileTransferEventArgs(chunk));
            }

            if (FileTransferProgressed != null)
            {
                FileTransferProgressed(this, new FileTransferEventArgs(chunk));
            }            

            if (chunk.IsEndChunk)
            {
                _requestedFiles.Remove(chunk.Header.Hash);
            }
        }

        internal void HandleFileRequestMessage(IMessage message)
        {
            FileRequestMessage requestMessage = message as FileRequestMessage;

            switch (requestMessage.Header.Response)
            {
                case FileRequestMode.Available:
                case FileRequestMode.NotAvailable:                    
                    HandleFileRequestResponse(requestMessage);
                    return;
                case FileRequestMode.Request:
                    HandleRequestedFileMessage(requestMessage);
                    return;                               
                case FileRequestMode.TransferFailed:
                    break;
                case FileRequestMode.ReadyToReceive:
                    HandleFileSend(requestMessage);
                    return;
                default:
                    break;
            }
            throw new NotImplementedException();
        }

        private void HandleFileSend(FileRequestMessage requestMessage)
        {
            if (!requestMessage.RequestedFile.Exists)
            {
                ClientProtocol.SendFileTransferFailure(requestMessage.RequestedFile);
                return;
            }

            long chunkIndex = requestMessage.Header.ChunkIndex;

            using (FileStream file = requestMessage.RequestedFile.OpenForRead())
            {                
                FileChunkHeader header = new FileChunkHeader();

                header.ChunkCount = (int)Math.Ceiling((double)file.Length / Settings.FileChunkSize);
                header.ChunkIndex = chunkIndex;
                header.Id = requestMessage.Header.Id;
                header.Hash = requestMessage.RequestedFile.Hash;                

                using (FileChunkMessage message = new FileChunkMessage(header))
                {
                    long offset = chunkIndex * Settings.FileChunkSize;
                    int length = (int) (offset + Settings.FileChunkSize > file.Length ? file.Length - offset : Settings.FileChunkSize);

                    byte[] chunk = new byte[length];                   

                    file.Seek(offset, SeekOrigin.Begin);

                    file.Read(chunk, 0, chunk.Length);
         
                    try
                    {
                        ClientProtocol.AddDataToMessage(message, chunk);

                        ClientProtocol.SendMessage(message);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
        }
        
        private void HandleRequestedFileMessage(FileRequestMessage requestMessage)
        {            
            FileRequestEventArgs e = new FileRequestEventArgs(requestMessage.RequestedFile);            

            if (FileRequestReceived != null)
            {
                FileRequestReceived(this, e);
            }

            ClientProtocol.SendFileRequestResponse(requestMessage.RequestedFile, e.Allow);
        }


        private void HandleFileRequestResponse(FileRequestMessage requestMessage)
        {
            if (FileRequestResponseReceived != null)
            {
                FileRequestResponseReceived(this, new FileRequestResponseEventArgs(requestMessage.RequestedFile, requestMessage.Header.Response));
            }
         
            if (requestMessage.Header.Response == FileRequestMode.Available)
            {
                PrepareForPendingFile(requestMessage.RequestedFile);
            }
            else
            {
                _requestedFiles.Remove(requestMessage.RequestedFile.Hash);
            }
        }

        private void PrepareForPendingFile(FileDescriptor fileDescriptor)
        {
            
        }

        private BitTunnelClientProtocol ClientProtocol
        {
            get
            {
                return ProtocolList.Count == 0 ? null : ProtocolList[0] as BitTunnelClientProtocol;
            }
        }

        protected override BitTunnelProtocol CreateProtocol()
        {
            if (ClientProtocol != null)
            {
                ClientProtocol.ProtocolMadeConnection -= new EventHandler(protocol_ProtocolMadeConnection);
            }
            BitTunnelClientProtocol protocol = new BitTunnelClientProtocol();

            protocol.ProtocolMadeConnection +=new EventHandler(protocol_ProtocolMadeConnection);

            return protocol;
        }

        void protocol_ProtocolMadeConnection(object sender, EventArgs e)
        {
            if (ProtocolMadeConnection != null)
            {
                ProtocolMadeConnection(this, EventArgs.Empty);
            }          
        }

        public override void OnConnectionFailed(Exception e)
        {
            
        }

        public override void OnMessageReceived(IMessage message)
        {
            if (_messageHandlers.ContainsKey(message.Key))
            {
                _messageHandlers[message.Key](message);
            }
            else
            {
                Debug.WriteLine("Dropped message type " + message.Key);
            }
        }
        
        internal void SendInitialFileRequest(FileDescriptor file)
        {
            if (_requestedFiles.ContainsKey(file.Hash)) return;//Request already in progress - action should also be blocked by UI

            _requestedFiles[file.Hash] = file;

            SendFileRequest(file, 0);

        }
        
        internal void SendFileRequest(FileDescriptor file, long requestedChunkIndex)
        {
            using (FileRequestMessage message = new FileRequestMessage())
            {
                message.Header.ChunkIndex = requestedChunkIndex;

                ClientProtocol.AddValueToMessage(message, file);

                ClientProtocol.SendMessage(message);
            }
        }

        internal void RequestFullFileList()
        {
            using (SimpleMessage message= new SimpleMessage())
            {
                message.MessageType = MessageType.FullFileListRequest;
               
                ClientProtocol.SendMessage(message);
            }
        }

        internal void OnNextFileChunkRequested(FileRequestEventArgs e)
        {
            if (!_requestedFiles.ContainsKey(e.Hash))
            {
                throw new InvalidOperationException(string.Format("Next chunk requested for unknown file {0)", e.Hash));
            }

            e.File = _requestedFiles[e.Hash];

            if (FileNextChunkRequestReceived != null)
            {
                FileNextChunkRequestReceived(this, new FileRequestEventArgs(e.File));
            }

            SendFileRequest(e.File, e.ChunkIndex);
        }        
    }
}
