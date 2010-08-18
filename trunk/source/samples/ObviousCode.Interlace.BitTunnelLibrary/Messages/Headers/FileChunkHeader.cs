using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers
{
    public class FileChunkHeader : BaseHeader
    {
        public FileChunkHeader() : base(MessageKeys.FileChunk)
        {

        }

        [Tunnel]
        public string Id { get; set; }
        [Tunnel]
        public string Hash { get; set; } 
        [Tunnel]
        public long ChunkCount { get; set; }
        [Tunnel]
        public long ChunkIndex { get; set; }                        
    }
}
