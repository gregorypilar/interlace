using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class FileRequestResponseEventArgs : EventArgs
    {        
        public FileRequestResponseEventArgs(FileDescriptor file, FileRequestMode response)
        {
            File = file;
            Response = response;
        }

        public FileDescriptor File { get; private set; }
        public FileRequestMode Response { get; private set; } 
    }
}
