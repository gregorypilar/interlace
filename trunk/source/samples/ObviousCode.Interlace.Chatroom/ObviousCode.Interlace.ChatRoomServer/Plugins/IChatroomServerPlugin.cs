using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.PropertyLists;
using ObviousCode.Interlace.ChatroomServer.Protocols;

namespace ObviousCode.Interlace.ChatroomServer.Plugins
{
    public interface IChatroomServerPlugin
    {
        string Key { get; }
        void HandleRequest(ChatroomServerProtocol protocol, PropertyDictionary request);
    }
}
