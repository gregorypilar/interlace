using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Messages.MessageInstances;
using ObviousCode.Interlace.TunnelSerialiser.Attributes;

namespace ObviousCode.Interlace.BitTunnelLibrary.Messages.Headers
{
    public class SimpleMessageHeader : BaseHeader
    {
        public SimpleMessageHeader()
            : base(MessageKeys.SimpleMessage)
        {

        }

        [Tunnel]
        public MessageType MessageType { get; set; }
    }
}
