using System;
using Interlace.Erlang;
using Interlace.PropertyLists;
using Interlace.ReactorCore;
using ObviousCode.Interlace.Chatroom.Library;

namespace ObviousCode.Interlace.ChatroomClient.Protocols
{
    public class ChatroomClientProtocolFactory : IProtocolFactory
    {
        public event EventHandler ServerConnectionFailed;
        public delegate void TermReceivedDelegate(object term);

        private TermReceivedDelegate _termReceived;

        ChatroomClientProtocol _protocol = null;

        public ChatroomClientProtocolFactory(TermReceivedDelegate callback)
        {
            _termReceived = callback;
        }        

        internal string RequestLogin(string username)
        {
            if (_protocol == null) return ChatroomKeys.LoginFail_ServerNotConnected;

            _protocol.RequestLogin(username);

            return null;
        }

        internal void NotifyOnLogout()
        {
            if (_protocol != null) _protocol.NotifyOnLogout();
        }

        internal void SendMessage(string message)
        {
            _protocol.SendChat(message);
        }

        #region IProtocolFactory Members

        public Protocol BuildProtocol()
        {
            _protocol = new ChatroomClientProtocol(this);

            _protocol.TermReceivedCallback = _termReceived;

            return _protocol;
        }

        public void ConnectionFailed(Exception ex)
        {
            Console.WriteLine("Client Reports Connection Failed");
        }

        public void StartedConnecting()
        {
            //Log event
        }

        #endregion        
    
        internal void GracefullyHandleLostServer()
        {
            if (ServerConnectionFailed != null)
            {
                ServerConnectionFailed(this, EventArgs.Empty);
            }
        }
    }
}
