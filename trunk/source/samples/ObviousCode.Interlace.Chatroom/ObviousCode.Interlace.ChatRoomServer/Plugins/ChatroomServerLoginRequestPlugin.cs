using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.Chatroom.Library;
using Interlace.PropertyLists;
using ObviousCode.Interlace.ChatroomServer.Protocols;
using Interlace.Erlang;

namespace ObviousCode.Interlace.ChatroomServer.Plugins
{
    public class ChatroomServerLoginRequestPlugin : IChatroomServerPlugin
    {

        public object _lockObject = new object();

        #region IChatroomServerPlugin Members

        public string Key
        {
            get { return ChatroomKeys.LoginRequest; }
        }

        public void HandleRequest(ChatroomServerProtocol protocol, PropertyDictionary request)
        {
            if (!request.HasValueFor(ChatroomKeys.MessageType) || request.StringFor(ChatroomKeys.MessageType) != Key)
            {
                protocol.SendMalformedRequestResponse();
                return;
            }

            object connected = null;

            lock (_lockObject)
            {
                connected = protocol.Parent.RequestLogin(protocol, request);
                request.SetValueFor(ChatroomKeys.Message, connected);
            }

            if (connected.ToString() == ChatroomKeys.LoginSuccess)
            {                
                protocol.Parent.BroadcastServerMessage("{0} has joined.", protocol.Username);
                protocol.SendWelcomeMessage();
            }

            protocol.SendTerm(Atom.From(request.PersistToString()));            
        }        

        #endregion
    }
}
