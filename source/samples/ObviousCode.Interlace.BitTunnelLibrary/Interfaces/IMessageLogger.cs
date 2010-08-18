using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;

namespace ObviousCode.Interlace.BitTunnelLibrary.Interfaces
{
    public enum MessageAction { Sending, Receiving };

    public interface IMessageLogger
    {
        void ViewMessage(string source, MessageAction action, BaseMessage message, DateTime timeStamp);
        void Flush();
    }
}
