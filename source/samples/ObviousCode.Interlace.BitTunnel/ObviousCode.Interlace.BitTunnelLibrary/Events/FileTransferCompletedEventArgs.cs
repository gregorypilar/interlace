using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using System.IO;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class FileTransferCompletedEventArgs : EventArgs
    {
        public FileTransferCompletedEventArgs(string hash, string location)
        {
            Hash = hash;
            Location = location;
        }

        public string Hash { get; set; }
        public string Location { get; private set; } 
    }
}
