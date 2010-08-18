using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.NestedFrames;
using System.IO;
using ObviousCode.Interlace.TunnelSerialiser;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages
{
    public class FileModificationMessage : BaseMessage<FileModificationHeader, FileModificationDescriptor>
    {
        public FileModificationMessage()
            : base(new FileModificationHeader()) { }

        public List<FileModificationDescriptor> Modifications
        {
            get { return RestoredItems == null ? new List<FileModificationDescriptor>() : RestoredItems; }
        }
    }
}
