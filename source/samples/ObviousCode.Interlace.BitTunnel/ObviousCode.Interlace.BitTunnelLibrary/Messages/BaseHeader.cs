using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages
{
    [Tunnel]
    public class BaseHeader : IHeader
    {
        public BaseHeader(MessageKeys key)
        {
            Key = key;
        }
        
        public MessageKeys Key { private set;  get; }

        [Tunnel]
        public int Count { get; set; }  
    }
}
