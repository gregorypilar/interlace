using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class FileListEventArgs : EventArgs
    {        
        public FileListEventArgs()
            : this(new FileDescriptor[] { })
        {
            
        }

        public FileListEventArgs(IEnumerable<FileDescriptor> modifications)
        {
            FileList = new List<FileDescriptor>(modifications);
        }

        public IList<FileDescriptor> FileList { get; set; }
    }
}
