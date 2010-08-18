using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;

namespace ObviousCode.Interlace.BitTunnelLibrary.Events
{    
    public class MessageEventArgs : EventArgs  
    {
        public MessageEventArgs(IMessage message, byte[] data)
        {
            Message = message;
            Data = data;
            TimeStamp = DateTime.Now;
        }

        public DateTime TimeStamp { get; private set; } 
        public IMessage Message { get; private set; }
        public byte[] Data { get; private set; }
    }

}
