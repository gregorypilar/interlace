using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.Chatroom.Library;
using Interlace.PropertyLists;
using ObviousCode.Interlace.ChatroomServer.Protocols;

namespace ObviousCode.Interlace.ChatroomServer.Plugins
{
    public class ChatroomServerBroadcastPlugin : IChatroomServerPlugin
    {
        #region IChatroomServerPlugin Members
        object _lockObject;

        public ChatroomServerBroadcastPlugin()
        {
            _lockObject = new object();
        }

        public string Key
        {
            get { return ChatroomKeys.Broadcast; }
        }

        public void HandleRequest(ChatroomServerProtocol protocol, PropertyDictionary request)
        {
            if (!request.HasValueFor(ChatroomKeys.MessageType) || !(request.StringFor(ChatroomKeys.MessageType) == Key))
            {
                protocol.SendMalformedRequestResponse();
                return;
            }

            lock (_lockObject)
            {
                string sender = protocol.Parent.ResolveSenderName(request);

                if (sender != null)
                {
                    request[ChatroomKeys.SenderName] = sender;

                    protocol.Parent.Broadcast(request);
                }
                else
                {
                    protocol.SendMalformedRequestResponse();
                }
            }
        }

        #endregion
    }
}
