using System;
using Interlace.ReactorCore;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;
using ObviousCode.Interlace.BitTunnelLibrary.Protocols;

namespace ObviousCode.Interlace.BitTunnelClient
{
    public class BitTunnelClientProtocol : BitTunnelProtocol
    {
        public event EventHandler ProtocolMadeConnection;//set this to internal then mark BitTunnel as an allowed assembly
             
        protected override void ConnectionLost(CloseReason reason)
        {
            base.ConnectionLost(reason);
        }

        internal void SendFileRequestResponse(FileDescriptor fileDescriptor, bool allowRequest)
        {
            SendFileRequestResponse(fileDescriptor, allowRequest ? FileRequestMode.Available : FileRequestMode.NotAvailable);
        }

        internal void SendFileTransferFailure(FileDescriptor fileDescriptor)
        {
            SendFileRequestResponse(fileDescriptor, FileRequestMode.NotAvailable);
        }
    }
}
