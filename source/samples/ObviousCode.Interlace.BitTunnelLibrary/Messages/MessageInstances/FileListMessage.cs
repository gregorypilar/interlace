using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.NestedFrames;
using ObviousCode.Interlace.TunnelSerialiser;
using System.IO;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages.MessageInstances
{
    public class FileListMessage : BaseMessage<FileListHeader, FileDescriptor>
    {
        public FileListMessage() : base(new FileListHeader())
        {
            
        }        

        public List<FileDescriptor> FileList
        {
            get
            {
                return RestoredItems;
            }
        }        
    }    
}
