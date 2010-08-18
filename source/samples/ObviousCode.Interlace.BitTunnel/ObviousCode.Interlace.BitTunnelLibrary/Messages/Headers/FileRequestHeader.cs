using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers
{    
    public enum FileRequestMode : byte { Request = 0x0000, ReadyToReceive, Available, NotAvailable, TransferFailed }
 
    [Tunnel]
    public class FileRequestHeader : BaseHeader
    {
        public FileRequestHeader()
            : base(MessageKeys.FileRequest)
        {
            Response = FileRequestMode.Request;            
        }

        [Tunnel]
        public long ChunkIndex { get; set; }

        [Tunnel]
        public FileRequestMode Response { get; set; } 

        [Tunnel]
        public string Id { get; set; }
    }
}
