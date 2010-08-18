using System;
using System.ComponentModel;
using ObviousCode.Interlace.Chatroom.Library;
using ObviousCode.Interlace.ChatroomClient;
using Interlace.Erlang;
using Interlace.PropertyLists;
using Interlace.ReactorService;

namespace ObviousCode.Interlace.ClientApplication
{
    public class Client : INotifyPropertyChanged
    {        
        ServiceHost _host;
        ChatroomClientService _service;

        internal event EventHandler ServerConnectionFailed;
        internal event EventHandler<MessageReceivedEventArgs> MessageReceived;

        static Client _adapter;

        bool _hostStarted = false;
        bool _loggedIn = false;        

        private Client()
        {

        }

        internal void StartHost()
        {
            if (_service != null) return;

            _host = new ServiceHost();

            _service = new ChatroomClientService(TermReceived);

            _service.ServerConnectionFailed += new EventHandler(_service_ServerConnectionFailed);

            _host.AddService(_service);

            _host.StartServiceHost();
            
            _host.OpenServices();

            _hostStarted = true;          
        }        

        internal void StopHost()
        {
            if (_service == null) return;

            _host.CloseServices();
            _host.StopServiceHost();

            HostStarted = false;

            LoggedIn = false;

            _service = null;
        }

        public string RequestLogin(string username)
        {
            if (_service == null) StartHost();

            return _service.RequestLogin(username);
        }

        public void Logout()
        {
            if (_service == null) return;//oh well

            //If we just drop out, server will realise fairly quickly, but lets be nice here, and let it know we are departing
            _service.NotifyOnLogout();

            StopHost();
        }

        internal void SendMessage(string message)
        {
            _service.SendMessage(message);
        }

        public void TermReceived(object term)
        {
            PropertyDictionary message = PropertyDictionary.FromString((term as Atom).Value);

            if (message.StringFor(ChatroomKeys.MessageType) == ChatroomKeys.LoginRequest)
            {
                LoggedIn = message.StringFor(ChatroomKeys.Message) == ChatroomKeys.LoginSuccess;
            }

            if (MessageReceived != null)
            {
                MessageReceived(this, new MessageReceivedEventArgs(message));
            }
        }

        public static Client Adapter
        {
            get
            {
                EnsureInstanceInstantiated();

                return _adapter;
            }
        }

        private static void EnsureInstanceInstantiated()
        {
            if (_adapter == null)
            {
                _adapter = new Client();
            }
        }

        public bool LoggedIn
        {
            get { return _loggedIn; }
            private set 
            { 
                _loggedIn = value;
                NotifyPropertyChanged("LoggedIn");
            }
        }
        
        public bool HostStarted
        {
            get { return _hostStarted; }
            private set
            {
                _hostStarted = value;
                NotifyPropertyChanged("HostStarted");
            }            
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        void _service_ServerConnectionFailed(object sender, EventArgs e)
        {
            if (ServerConnectionFailed != null)
            {
                _service = null;
                _host = null;
                ServerConnectionFailed(sender, e);
            }

            LoggedIn = false;
        }

        #endregion        
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        PropertyDictionary _message;

        public MessageReceivedEventArgs(PropertyDictionary message)
        {
            _message = message;
        }

        public PropertyDictionary Message { get { return _message; } set { _message = value; } }
    }
}
