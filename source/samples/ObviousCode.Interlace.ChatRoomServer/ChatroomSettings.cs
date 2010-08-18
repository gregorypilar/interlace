using System;
using System.Collections.Generic;
using System.Text;

namespace ObviousCode.Interlace.ChatroomServer
{
    public class ChatroomSettings
    {
        int _port;
        int _maximumClients;

        string _welcomeMessage;
        string _version;

        public ChatroomSettings()
        {
            _port = 1809;            
            _maximumClients = 3;

            _welcomeMessage = "Welcome To Interlace Chat";
            _version = "0.1";
        }

        public string WelcomeMessage
        {
            get { return _welcomeMessage; }
            set { _welcomeMessage = value; }
        }

        public int MaximumClients
        {
            get { return _maximumClients; }
            set { _maximumClients = value; }
        }

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }
    }
}
