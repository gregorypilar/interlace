using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelClient;
using ObviousCode.Interlace.BitTunnelLibrary.Messages;
using System.IO;
using System.Security.AccessControl;

namespace ObviousCode.Interlace.BitTunnel.Connectivity
{
    public abstract class Instance : IInstance
    {
        ConnectionType _connectionType;

        AppSettings _settings;

        public event EventHandler ConnectionMade;
        public event EventHandler ConnectionTerminated;
        public event EventHandler<ExceptionEventArgs> LostConnection;
        public event EventHandler<MessageEventArgs> MessageReceived;

        public Instance(AppSettings settings, ConnectionType type) : this(settings, "", type)
        { }

        public Instance(AppSettings settings, string instanceName, ConnectionType type)
        {
            //is this necessary to store?
            _connectionType = type;

            _settings = settings;

            if (!_settings.WorkingPath.Exists)
            {
                _settings.WorkingPath.Create();
            }

            if (!_settings.TransferPath.Exists)
            {
                _settings.TransferPath.Create();
            }

            Connection = new Connection(type, settings, instanceName);

            Connection.Service.Instance = this;

            Connection.LostConnection += new EventHandler<ExceptionEventArgs>(Connection_LostConnection);
            Connection.MessageReceived += new EventHandler<MessageEventArgs>(Connection_MessageReceived);
            Connection.MessageSending += new EventHandler<MessageEventArgs>(Connection_MessageSending);
            Connection.ConnectionMade += new EventHandler(Connection_ConnectionMade);
            Connection.ConnectionTermainated += new EventHandler(Connection_ConnectionTermainated);
        }

        public AppSettings Settings
        {
            get
            {
                return _settings;
            }
        }

        public IBitTunnelService Service
        {
            get
            {
                return Connection.Service;
            }
        }

        public bool Connect()
        {
            return Connection.Connect();
        }

        public void Disconnect()
        {           
            Connection.Disconnect();
        }

        public Connection Connection { get; set; }

        void Connection_ConnectionTermainated(object sender, EventArgs e)
        {
            if (ConnectionTerminated != null)
            {
                ConnectionTerminated(sender, e);
            }

        }

        void Connection_ConnectionMade(object sender, EventArgs e)
        {
            if (ConnectionMade != null)
            {
                ConnectionMade(sender, e);
            }
        }   

        void Connection_LostConnection(object sender, ExceptionEventArgs e)
        {
            if (LostConnection != null)
            {
                LostConnection(sender, e);
            }
            else
            {
                throw e.ThrownException;
            }

            OnConnectionLost(e.ThrownException);
        }

        public bool IsConnected
        {
            get
            {
                return Connection.IsConnected;
            }
        }


        void Connection_MessageReceived(object sender, MessageEventArgs e)
        {
            if (_settings.Logger != null)
            {
                string name = Service is BitTunnelClientService ? (Service as BitTunnelClientService).ConnectionDetails.PublicName : "Server";
                _settings.Logger.ViewMessage(name, MessageAction.Receiving, e.Message as BaseMessage, e.TimeStamp);
            }

            if (MessageReceived != null)
            {
                MessageReceived(sender, e);
            }            

            OnMessageReceived(e.Message);
        }

        void Connection_MessageSending(object sender, MessageEventArgs e)
        {
            try
            {
                if (_settings.Logger != null)
                {
                    string name = (Service is BitTunnelClientService && (Service as BitTunnelClientService).ConnectionDetails != null)
                        ? (Service as BitTunnelClientService).ConnectionDetails.PublicName
                        : "Server";

                    _settings.Logger.ViewMessage(name, MessageAction.Sending, e.Message as BaseMessage, e.TimeStamp);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected abstract void OnMessageReceived(IMessage message);
        protected abstract void OnConnectionLost(Exception e);
        protected abstract void OnDispose();

        public void Dispose()
        {
            OnDispose();

            Connection.Dispose();           
        }       
    }
}
