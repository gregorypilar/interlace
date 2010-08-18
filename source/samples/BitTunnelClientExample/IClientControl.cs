using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using System.Windows.Forms;

namespace BitTunnelClientExample
{
    internal interface IClientControl
    {        
        ClientInstance Client { get; set; }
        DockStyle Dock { get; set; }
        bool Visible { get; set; }
    }
}
