using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{
    public class DataReceivedEventArgs : EventArgs
    {
        public DataReceivedEventArgs()
            : this(new byte[] { })
        {

        }

        public DataReceivedEventArgs(byte[] fileData)
        {
            FileData = fileData;
        }

        public byte[] FileData { get; set; }
    }
}
