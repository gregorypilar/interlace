using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;

namespace ObviousCode.Interlace.BitTunnelLibrary.Interfaces
{
    public interface IHeader
    {
        MessageKeys Key { get; }
        int Count { get; set; }
    }
}
