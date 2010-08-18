using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Identification;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class IdentificationEventArgs : EventArgs
    {
        public IdentificationEventArgs(ConnectedClient client)
        {
            Client = client;
        }

        public ConnectedClient Client { get; set; }
    }
}
