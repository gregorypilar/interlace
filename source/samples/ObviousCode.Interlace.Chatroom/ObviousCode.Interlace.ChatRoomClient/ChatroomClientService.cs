using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorService;
using ObviousCode.Interlace.ChatroomClient.Protocols;
using System.Net;

namespace ObviousCode.Interlace.ChatroomClient
{
    public class ChatroomClientService : IService
    {
        public event EventHandler ServerConnectionFailed;

        ChatroomClientProtocolFactory _factory;

        public ChatroomClientService(ChatroomClientProtocolFactory.TermReceivedDelegate callback)
        {
            _factory = new ChatroomClientProtocolFactory(callback);
            _factory.ServerConnectionFailed += new EventHandler(_factory_ServerConnectionFailed);
        }
        
        public void SendMessage(string message)
        {
            if (_factory != null)
            {
                _factory.SendMessage(message);
            }
        }

        #region IService Members

        public string RequestLogin(string username)
        {
            return _factory.RequestLogin(username);
        }

        public void NotifyOnLogout()
        {
            _factory.NotifyOnLogout();
        }

        public void Close(IServiceHost host)
        {

        }

        public void Open(IServiceHost host)
        {
            host.Reactor.ConnectStream(_factory, IPAddress.Parse("127.0.0.1"), 1809);
        }

        void _factory_ServerConnectionFailed(object sender, EventArgs e)
        {
            if (ServerConnectionFailed != null)
            {
                ServerConnectionFailed(sender, e);
            }
        }             


        #endregion
    }
}
