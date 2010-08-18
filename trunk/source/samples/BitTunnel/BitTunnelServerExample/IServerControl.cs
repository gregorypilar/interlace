using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace BitTunnelServerExample
{
    public interface IServerControl
    {
        Image MenuIcon { get; }
        string MenuText { get; }
    }
}
