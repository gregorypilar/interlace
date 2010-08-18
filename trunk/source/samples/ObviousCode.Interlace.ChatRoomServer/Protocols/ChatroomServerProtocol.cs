using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.Erlang;
using Interlace.PropertyLists;
using ObviousCode.Interlace.Chatroom.Library;
using ObviousCode.Interlace.ChatroomServer;
using ObviousCode.Interlace.ChatroomServer.Plugins;
using Interlace.ReactorCore;

namespace ObviousCode.Interlace.ChatroomServer.Protocols
{
    public class ChatroomServerProtocol : TermProtocol
    {        
        ChatroomServerProtocolFactory _parent;        

        Dictionary<string, IChatroomServerPlugin> _plugins;

        string _username;
        string _key;

        public ChatroomServerProtocol(ChatroomServerProtocolFactory parent, List<IChatroomServerPlugin> plugins)
        {
            _parent = parent;

            LoadPlugins(plugins);

            _username = null;
            _key = null;
        }

        protected override void ConnectionLost(CloseReason reason)
        {
            Console.WriteLine("{0}Connection Lost {1}", string.IsNullOrEmpty(_username) ? "Lurker " : "", string.IsNullOrEmpty(_username) ? _username : "");
            //TODO: Log event
            //LOST CONNECTION CALL BACK

            if (!string.IsNullOrEmpty(_username))
            {
                Parent.ReportDisconnection(this);
            }
        }        

        protected override void TermReceived(object term)
        {
            Atom atom = term as Atom;

            if (atom == null || atom.Value == null) return;

            PropertyDictionary message = PropertyDictionary.FromString(atom.Value);
            
            if (!message.HasStringFor(ChatroomKeys.MessageType)) return;

            if (!_plugins.ContainsKey(message.StringFor(ChatroomKeys.MessageType))) return;

            _plugins[message.StringFor(ChatroomKeys.MessageType)].HandleRequest(this, message);
        }        

        public ChatroomServerProtocolFactory Parent
        {
            get { return _parent; }
        }

        private void LoadPlugins(List<IChatroomServerPlugin> pluginList)
        {
            _plugins = new Dictionary<string, IChatroomServerPlugin>();

            foreach (IChatroomServerPlugin plugin in pluginList)
            {
                _plugins[plugin.Key] = plugin;
            }
        }

        internal void SendMalformedRequestResponse()
        {
            PropertyDictionary response = PropertyDictionary.EmptyDictionary();

            response.SetValueFor(ChatroomKeys.Message, ChatroomKeys.MalformedRequest);

            SendMessage(response);
        }



        protected override void ConnectionMade()
        {
            Console.WriteLine("Connection made ...");    
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        internal void SendWelcomeMessage()
        {
            PropertyDictionary welcome = PropertyDictionary.EmptyDictionary();

            welcome.SetValueFor(ChatroomKeys.MessageType, ChatroomKeys.Message);
            welcome.SetValueFor(ChatroomKeys.Message, Parent.WelcomeMessage);

            SendMessage(welcome);
          
            welcome.SetValueFor(ChatroomKeys.Message, " ");

            SendMessage(welcome);
        }

        internal void SendMessage(PropertyDictionary message)
        {
            SendTerm(Atom.From(message.PersistToString()));
        }
    }    
}
