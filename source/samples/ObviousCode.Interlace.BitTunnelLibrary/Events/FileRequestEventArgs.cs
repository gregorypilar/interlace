using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    /// <summary>
    /// This will allow file requests by default, with the onus on the receiving event to disallow the request.
    /// 
    /// If this is a security risk then ENSURE that Allow property is switched to false before event is fired,
    /// or change in constructor and recompile    
    /// 
    /// </summary>
    public class FileRequestEventArgs : FileDescriptorEventArgs
    {
        string _hash;
        public FileRequestEventArgs(FileDescriptor descriptor)
            : base(descriptor)
        {
            //By default, allow. If this is a security risk, then switch here
            Allow = true;
        }

        public long ChunkIndex { get; set; }
        public bool Allow { get; set; }

        /// <summary>
        /// Next Chunk requests don't come with File Descriptor, just hash
        /// </summary>
        public string Hash
        {
            get
            {
                if (File == null)
                {
                    return _hash;
                }
                return File.Hash;
            }
            set
            {
                _hash = value;
            }
        }       
    }
}
