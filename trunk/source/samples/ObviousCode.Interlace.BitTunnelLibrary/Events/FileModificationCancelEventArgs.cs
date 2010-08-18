using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ObviousCode.Interlace.BitTunnelLibrary.File;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class FileModificationCancelEventArgs : CancelEventArgs
    {        
        public FileModificationCancelEventArgs(FileModificationDescriptor file)
        {
            File = file;
        }

        public FileModificationDescriptor File { get; set; }
    }
}
