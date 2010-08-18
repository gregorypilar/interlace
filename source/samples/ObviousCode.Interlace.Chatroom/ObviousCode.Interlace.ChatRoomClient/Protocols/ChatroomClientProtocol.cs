using System;
using Interlace.Erlang;
using Interlace.PropertyLists;
using Interlace.ReactorCore;
using ObviousCode.Interlace.Chatroom.Library;

namespace ObviousCode.Interlace.ChatroomClient.Protocols
{
    public class ChatroomClientProtocol : TermProtocol
    {
        private string _id = Guid.NewGuid().ToString();

        private ChatroomClientProtocolFactory.TermReceivedDelegate _termReceived;     
        
        bool _isLoggedWithServer;
        ChatroomClientProtocolFactory _parent;

        internal ChatroomClientProtocol (ChatroomClientProtocolFactory parent)
        {
            _parent = parent;
        }

        protected override void  ConnectionMade()
        {
 	         base.ConnectionMade();
        }

        public ChatroomClientProtocolFactory.TermReceivedDelegate TermReceivedCallback
        {
            set
            {
                _termReceived = value;
            }
        }

        internal void SendChat(string message)
        {
            PropertyDictionary chat = BuildMessageDictionary(ChatroomKeys.Broadcast);

            chat[ChatroomKeys.Message] = message;

            SendMessage(chat);
        }        

        internal void RequestLogin(string username)
        {
            PropertyDictionary request = BuildMessageDictionary(ChatroomKeys.LoginRequest);

            request[ChatroomKeys.SenderName] = username;

            SendMessage(request);
        }

        internal void NotifyOnLogout()
        {
            PropertyDictionary logout = BuildMessageDictionary(ChatroomKeys.LogoutNotification);

            SendMessage(logout);
        }        
        
        internal void SendMessage(PropertyDictionary message)
        {
            try
            {
                SendTerm(Atom.From(message.PersistToString()));
            }
            catch(InvalidOperationException e)
            {
                //should we through a less generic exception to make this easier to detect?
                if (e.Message == "An attempt was made to send data through a closed socket.")
                {
                    _parent.GracefullyHandleLostServer();
                }
                else throw;
            }            
        }

        protected override void ConnectionLost(CloseReason reason)
        {
            base.ConnectionLost(reason);

            _parent.GracefullyHandleLostServer();
        }

        private PropertyDictionary BuildMessageDictionary(string messageType)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary[ChatroomKeys.MessageType] = messageType;
            dictionary[ChatroomKeys.SenderId] = _id;

            return dictionary;
        }

        protected override void TermReceived(object term)
        {            
            PropertyDictionary termDictionary = PropertyDictionary.FromString((term as Atom).Value);

            if (!termDictionary.HasStringFor(ChatroomKeys.MessageType)) return; //ignore

            Console.WriteLine("Message Received", termDictionary.StringFor(ChatroomKeys.Message));

            if (_termReceived != null)
            {
                _termReceived(term);
            }
        }        
    }
}
