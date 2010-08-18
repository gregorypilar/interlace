using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class FileListModificationEventArgs : EventArgs
    {
        public FileListModificationEventArgs()
            : this(new FileModificationDescriptor[] { })
        {

        }
        
        public FileListModificationEventArgs(IEnumerable<FileModificationDescriptor> modifications)
        {
            Modifications = new List<FileModificationDescriptor>(modifications);
        }

        public IList<FileModificationDescriptor> Modifications { get; set; }
    }
}
