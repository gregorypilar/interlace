using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class FileDescriptorEventArgs : EventArgs
    {               
        public FileDescriptorEventArgs(FileDescriptor descriptor)
        {
            File = descriptor;
        }

        public FileDescriptor File { get; set; }
    }
}
