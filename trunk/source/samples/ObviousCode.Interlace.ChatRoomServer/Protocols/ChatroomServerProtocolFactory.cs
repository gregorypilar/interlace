using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorCore;
using Interlace.PropertyLists;
using Interlace.Erlang;
using ObviousCode.Interlace.ChatroomServer.Plugins;
using ObviousCode.Interlace.Chatroom.Library;
using ObviousCode.Interlace.ChatroomServer;

namespace ObviousCode.Interlace.ChatroomServer.Protocols
{
    public class ChatroomServerProtocolFactory : IProtocolFactory
    {                
        ChatroomSettings _settings;        
        List<IChatroomServerPlugin> _plugins;        

        ClientCache _clients;

        public ChatroomServerProtocolFactory(ChatroomSettings settings, params IChatroomServerPlugin[] plugins)
        {            
            _settings = settings;

            LoadDefaultPlugins();

            _plugins.AddRange(plugins);

            _clients = new ClientCache();
        }

        private void LoadDefaultPlugins()
        {
            ChatroomServerBroadcastPlugin broadcaster = new ChatroomServerBroadcastPlugin();
            ChatroomServerLoginRequestPlugin login = new ChatroomServerLoginRequestPlugin();
            ChatroomServerLogoutRequestPlugin logout = new ChatroomServerLogoutRequestPlugin();

            _plugins = new List<IChatroomServerPlugin>();

            _plugins.Add(broadcaster);
            _plugins.Add(login);
            _plugins.Add(logout);
        }

        

        public Protocol BuildProtocol()
        {
            ChatroomServerProtocol newInstance = new ChatroomServerProtocol(this, _plugins);                                  

            return newInstance;
        }
       
        internal void Broadcast(PropertyDictionary request)
        {
            foreach (ChatroomServerProtocol instance in _clients)
            {
                instance.SendMessage(request);
            }
        }

        internal void BroadcastLoggedInUsers()
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor(ChatroomKeys.MessageType, ChatroomKeys.ChatterListUpdate);
            dictionary.SetValueFor(ChatroomKeys.Message, BuildUserList());

            Broadcast(dictionary);
        }

        internal void BroadcastServerMessage(string message, params string[] args)
        {
            PropertyDictionary dictionary = PropertyDictionary.EmptyDictionary();

            dictionary.SetValueFor(ChatroomKeys.MessageType, ChatroomKeys.Message);
            dictionary.SetValueFor(ChatroomKeys.Message, string.Format(message, args));

            Broadcast(dictionary);
        }

        private bool CanAddClient
        {
            get
            {                
                return _clients.Count < _settings.MaximumClients;
            }
        }

        internal string ResolveSenderName(PropertyDictionary request)
        {
            if (request.HasValueFor(ChatroomKeys.SenderId))
            {
                ChatroomServerProtocol protocol = _clients[request.StringFor(ChatroomKeys.SenderId)];

                return protocol == null ? null : protocol.Username;
            }

            return null;
        }

        private bool UsernameIsAvailable(string username)
        {
            foreach (ChatroomServerProtocol instance in _clients)
            {
                if (instance.Username.ToUpperInvariant() == username.Trim().ToUpperInvariant()) return false;
            }

            return true;
        }

        public void StartedConnecting()
        {
            Console.WriteLine("ChatRoom Server connection commenced ...");
        }

        internal object RequestLogin(ChatroomServerProtocol protocol, PropertyDictionary request)
        {
            if (!CanAddClient) return ChatroomKeys.LoginFail_TooManyClients;
            if (!UsernameIsAvailable(request.StringFor(ChatroomKeys.SenderName))) return ChatroomKeys.LoginFail_UserNameInUse;

            protocol.Username = request.StringFor(ChatroomKeys.SenderName);
            protocol.Key = request.StringFor(ChatroomKeys.SenderId);

            _clients.AddClient(protocol);

            BroadcastLoggedInUsers();

            return ChatroomKeys.LoginSuccess;
        }

        private PropertyArray BuildUserList()
        {
            PropertyArray users = PropertyArray.EmptyArray();

            foreach (ChatroomServerProtocol user in _clients)
            {
                users.AppendValue(user.Username);
            }

            return users;
        }        

        public void ConnectionFailed(Exception ex)
        {
            Console.WriteLine("Server Protocol Factory reports Connection Failed");
        }

        internal void ReportDisconnection(ChatroomServerProtocol disconnectingProtocol)
        {
            _clients.RemoveClient(disconnectingProtocol);

            BroadcastServerMessage("{0} has left.", disconnectingProtocol.Username);
            BroadcastLoggedInUsers();
        }

        internal string WelcomeMessage
        {
            get
            {
                return _settings.WelcomeMessage;
            }
        }

        internal void NotifyOnLogout(ChatroomServerProtocol protocol)
        {
            Console.WriteLine("{0} logged out gracefully.", protocol.Username);

            ReportDisconnection(protocol);
        }        
    }
}
