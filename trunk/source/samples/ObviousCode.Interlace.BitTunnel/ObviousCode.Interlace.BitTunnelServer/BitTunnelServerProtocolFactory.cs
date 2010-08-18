using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorCore;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.Protocols;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using System.Diagnostics;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using System.Collections;
using ObviousCode.Interlace.BitTunnelLibrary;

namespace ObviousCode.Interlace.BitTunnelServer
{
    public class BitTunnelServerProtocolFactory : BitTunnelProtocolFactory
    {
        public event EventHandler<IdentificationEventArgs> ClientConnected;
        public event EventHandler<IdentificationEventArgs> ClientIdentified;
        public event EventHandler<FileListModificationEventArgs> AvailableFilesUpdated;
        public event EventHandler ConnectionTerminated;
        
        BitTunnelServerService _service;

        public BitTunnelServerProtocolFactory(BitTunnelServerService service, string serverName) : base(service.Settings)
        {                        
            ServerId = Guid.NewGuid().ToString();
            
            ServerName = serverName;

            _service = service;
        }

        #region IProtocolFactory Members

        protected override BitTunnelProtocol CreateProtocol()
        {
            BitTunnelServerProtocol protocol = new BitTunnelServerProtocol(this);
            
            protocol.ClientConnected += new EventHandler<IdentificationEventArgs>(protocol_ClientConnected);                       
            protocol.LostConnection +=new EventHandler(protocol_LostConnection);
            protocol.ClientIdentified += new EventHandler<IdentificationEventArgs>(protocol_ClientIdentified);
            protocol.AvailableFilesUpdated += new EventHandler<FileListModificationEventArgs>(protocol_AvailableFilesUpdated);
            return protocol;
        }

        void protocol_AvailableFilesUpdated(object sender, FileListModificationEventArgs e)
        {
            if (AvailableFilesUpdated != null)
            {
                AvailableFilesUpdated(sender, e);
            }
        }

        void protocol_ClientIdentified(object sender, IdentificationEventArgs e)
        {
            if (ClientIdentified != null)
            {
                ClientIdentified(sender, e);
            }
        }

        void protocol_ClientConnected(object sender, IdentificationEventArgs e)
        {
            if (ClientConnected != null)
            {
                ClientConnected(sender, e);
            }
        }

        void protocol_LostConnection(object sender, EventArgs e)
        {
            ProtocolList.Remove(sender as BitTunnelProtocol); 
            if (ConnectionTerminated!= null)
            {
                ConnectionTerminated(sender, e);
            }
        }        

        public override void StartedConnecting()
        {
            Debug.WriteLine("Connecting ... - Should handle with NLog");
        }

        #endregion

        //LINQify
        internal IList<FileDescriptor> CurrentlyAvailableFiles
        {
            get
            {
                List<FileDescriptor> files = new List<FileDescriptor>();

                foreach (BitTunnelServerProtocol protocol in ProtocolList)
                {
                    files.AddRange(protocol.Files.GetCurrentUniqueFileList());
                }

                return files;
            }
        }

        public override void OnConnectionFailed(Exception e)
        {

        }

        public override void OnMessageReceived(IMessage message)
        {
            
        }

        public int ConnectionCount
        {
            get
            {
                return ProtocolList.Count;
            }
        }

        internal string ServerName { get; private set; }
        internal string ServerId { get; private set; }

        internal void Broadcast(IMessage message)
        {            
            BitTunnelServerProtocol[] asNow = new BitTunnelServerProtocol[ProtocolList.Count];

            ProtocolList.CopyTo(asNow);

            for (int i = 0; i < asNow.Length; i++)
            {
                asNow[i].SendMessage(message);
            }
        }

        //Send modifications where they appeared to have been valid
        //i.e Remove has fully removed all instances, or New where file is available on a client.        
        //Functionality is, therefore, subtlety different for 
        //
        //  * Remove (will be used in the message ONLY when all files are removed) and
        //  * New (will be added in the message whilst the uniquely hashed file is available each time an instance is added).
        //
        //As multiple of the same file (in different locations on the client) can be added at once,
        //it is not so easy to differentiate a 'first addition' of a file to only use those here as
        //
        //Currently up to clients as to whether to store the added files (with the limitToUniquelyHashedFilesOnly
        //flag on the FileDescriptorLookup).
        //
        //Sure I can come up with a nicer solution, but, until then, this is functionally accurate        
        internal void BroadcastFileListModifications(List<FileModificationDescriptor> list)
        {
            if (ProtocolList.Count == 0) return;

            List<FileDescriptor> availableFiles = CurrentlyAvailableFiles as List<FileDescriptor>;

            using (FileModificationMessage message = new FileModificationMessage())
            {
                foreach(FileModificationDescriptor modification in list)
                {
                    bool available = availableFiles.Exists(f => f.Hash == modification.Hash);

                    if (
                        (modification.Mode == FileModificationMode.New && available) ||
                        (modification.Mode == FileModificationMode.Remove && !available) ||
                        (modification.Mode == FileModificationMode.Renamed)
                        )
                    {
                        ProtocolList[0].AddValueToMessage(message, modification);
                    }
                }

                Broadcast(message);
            }
        }

        internal IList<BitTunnelServerProtocol> Protocols
        {
            get
            {
                return new List<BitTunnelServerProtocol>(ProtocolList.Cast<BitTunnelServerProtocol>());
            }
        }

        internal void RequestFile(FileDescriptor fileDescriptor, long chunkIndex, Action<FileDescriptor, FileRequestMode> sendResponseCallback, Action<FileDescriptor, FileChunkMessage> sendChunkCallback)
        {
            _service.RequestFile(fileDescriptor, chunkIndex, sendResponseCallback, sendChunkCallback);            
        }

        internal void HandleFileChunk(FileChunkMessage message)
        {
            _service.FileChunkReceived(message);
        }
    }
}
