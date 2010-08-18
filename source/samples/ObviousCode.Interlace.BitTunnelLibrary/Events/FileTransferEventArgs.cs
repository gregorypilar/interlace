using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class FileTransferEventArgs : EventArgs
    {
        public FileTransferEventArgs(FileChunkMessage chunk)
        {
            FileChunk = chunk;
        }

        public FileChunkMessage FileChunk { get; private set; } 
    }
}
