using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitTunnelClientExample
{
    internal interface IClientTabControl : IClientControl
    {
        string TabText { get; }

    }
}
