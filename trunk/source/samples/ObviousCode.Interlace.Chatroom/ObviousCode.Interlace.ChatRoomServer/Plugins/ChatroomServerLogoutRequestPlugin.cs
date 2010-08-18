using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.Chatroom.Library;
using Interlace.PropertyLists;

namespace ObviousCode.Interlace.ChatroomServer.Plugins
{
    public class ChatroomServerLogoutRequestPlugin : IChatroomServerPlugin
    {

        #region IChatroomServerPlugin Members

        public string Key
        {
            get { return ChatroomKeys.LogoutNotification; }
        }

        public void HandleRequest(ObviousCode.Interlace.ChatroomServer.Protocols.ChatroomServerProtocol protocol, PropertyDictionary request)
        {
            protocol.Parent.NotifyOnLogout(protocol);
        }

        #endregion
    }
}
