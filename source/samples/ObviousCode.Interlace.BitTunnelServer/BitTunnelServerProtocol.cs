using System;
using System.Collections.Generic;
using Interlace.ReactorCore;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Identification;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.MessageInstances;
using ObviousCode.Interlace.BitTunnelLibrary.Protocols;

namespace ObviousCode.Interlace.BitTunnelServer
{
    public class BitTunnelServerProtocol : BitTunnelProtocol
    {        
        internal event EventHandler<FileRequestResponseEventArgs> FileRequestResponseReceived;
        internal event EventHandler<IdentificationEventArgs> ClientConnected;
        internal event EventHandler<IdentificationEventArgs> ClientIdentified;
        internal event EventHandler<FileListModificationEventArgs> AvailableFilesUpdated;

        FileDescriptorLookup _files;

        BitTunnelServerProtocolFactory _parent;

        ConnectedClient _client;

        Dictionary<MessageKeys, Action<IMessage>> _handlers;        

        public BitTunnelServerProtocol(BitTunnelServerProtocolFactory parent) : base()
        {
            _parent = parent;            

            _files = new FileDescriptorLookup();

            SetupHandlers();
        }

        public ConnectedClient ClientDetails
        {
            get { return _client; }
        }

        private void SetupHandlers()
        {
            _handlers = new Dictionary<MessageKeys, Action<IMessage>>();

            _handlers[MessageKeys.Identification] = HandleIdentificationMessage;
            _handlers[MessageKeys.FileListModifications] = HandleFileModificationMessage;
            _handlers[MessageKeys.FileRequest] = HandleFileRequestMessage;
            _handlers[MessageKeys.FileChunk] = HandleFileChunkMessage;
            _handlers[MessageKeys.SimpleMessage] = HandleSimpleMessage;
        }

        protected override void ConnectionMade()
        {
            _client = new ConnectedClient();

            _client.ServerId = _parent.ServerId;
            _client.ServerPublicName = _parent.ServerName;

            if (Connection is StreamSocketConnection)
            {
                _client.IPAddress = (Connection as StreamSocketConnection).RemoteEndPoint.Address.ToString();
                _client.Port = (Connection as StreamSocketConnection).RemoteEndPoint.Port;
            }

            using (IdentificationMessage message = new IdentificationMessage())
            {
                AddValueToMessage(message, _client);
                SendMessage(message);
            }

            base.ConnectionMade();

            if (ClientConnected != null)
            {
                ClientConnected(this, new IdentificationEventArgs(_client));
            }
        }       

        protected override void OnMessageReceived(IMessage message)
        {
            if (_handlers.ContainsKey(message.Key))
            {
                _handlers[message.Key](message);
            }
            else
                throw new InvalidOperationException(
                    string.Format("Server Protocol cannot handle message type {0}", message.Key));
        }

        public FileDescriptorLookup Files
        {
            get { return _files; }
        }

        private void HandleSimpleMessage(IMessage message)
        {
            if (message is SimpleMessage)
            {
                switch ((message as SimpleMessage).MessageType)
                {
                    case MessageType.FullFileListRequest:
                        SendFullFileList();
                        break;
                    default:
                        throw new InvalidOperationException(
                            string.Format("Simple Message type {0} not handled")
                            );
                }
            }
            else throw new InvalidOperationException();
        }

        private void HandleIdentificationMessage(IMessage message)
        {
            if (message is IdentificationMessage)
            {
                _client = (message as IdentificationMessage).Client;
                
                if (ClientIdentified != null)
                {
                    ClientIdentified(this, new IdentificationEventArgs(_client));
                }

                SendFullFileList();
            }
            else throw new InvalidOperationException();
        }        

        private void HandleFileModificationMessage(IMessage message)
        {
            if (message is FileModificationMessage)
            {
                _files.UpdateFileList((message as FileModificationMessage).Modifications);

                _parent.BroadcastFileListModifications((message as FileModificationMessage).Modifications);

                if (AvailableFilesUpdated != null)
                {
                    FileListModificationEventArgs e = new FileListModificationEventArgs((message as FileModificationMessage).Modifications);

                    AvailableFilesUpdated(this, e);
                }
            }
            else throw new InvalidOperationException();
        }

        private void HandleFileRequestMessage(IMessage message)
        {
            if (!(message is FileRequestMessage)) throw new InvalidOperationException();

            switch ((message as FileRequestMessage).Header.Response)
            {
                case FileRequestMode.Request:
                    _parent.RequestFile((message as FileRequestMessage).RequestedFile, (message as FileRequestMessage).Header.ChunkIndex, 
                                            SendFileRequestResponse, SendFileChunk);
                    break;
                case FileRequestMode.Available:
                case FileRequestMode.NotAvailable:
                     if (FileRequestResponseReceived != null)
                    {
                        FileRequestResponseReceived(this, new FileRequestResponseEventArgs((message as FileRequestMessage).RequestedFile, (message as FileRequestMessage).Header.Response));
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }            
        }

        private void HandleFileChunkMessage(IMessage message)
        {
            if (message is FileChunkMessage)
            {
                _parent.HandleFileChunk(message as FileChunkMessage);
            }
            else throw new InvalidOperationException();
        }

        private void SendFileChunk(FileDescriptor description, FileChunkMessage chunk)
        {
            AddDataToMessage(chunk, chunk.Chunk);

            SendMessage(chunk);
        }

        private void SendFullFileList()
        {
            using (FileListMessage fileListMessage = new FileListMessage())
            {
                foreach (FileDescriptor descriptor in _parent.CurrentlyAvailableFiles)
                {
                    AddValueToMessage(fileListMessage, descriptor);
                }

                SendMessage(fileListMessage);
            }
        }
    }
}
