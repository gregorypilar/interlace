using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception e)
        {
            ThrownException = e;
        }

        public Exception ThrownException { get; private set; }
    }
}
