using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.File;

namespace ObviousCode.Interlace.BitTunnelLibrary.Interfaces
{
    public interface IClientInstance : IInstance
    {        
        void AddFiles(IEnumerable<FileDescriptor> files);
        void RemoveFiles(IEnumerable<FileDescriptor> files);
    }
}
