using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages
{
    public class FileRequestMessage : BaseMessage<FileRequestHeader, FileDescriptor>
    {
        public FileRequestMessage()
            : this(new FileRequestHeader())
        {

        }

        public FileRequestMessage(FileRequestHeader header)
            : base(header)
        {

        }
        
        public FileDescriptor RequestedFile
        {
            get
            {
                if (RestoredItems.Count == 0) throw new InvalidOperationException();

                return RestoredItems[0];
            }
        }
    }
}
