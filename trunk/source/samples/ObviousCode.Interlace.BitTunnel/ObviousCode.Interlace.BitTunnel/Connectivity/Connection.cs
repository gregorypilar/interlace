using System;
using System.Collections.Generic;
using System.Threading;
using Interlace.ReactorService;
using ObviousCode.Interlace.BitTunnelClient;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelServer;

namespace ObviousCode.Interlace.BitTunnel.Connectivity
{
    public class Connection : IDisposable
    {
        public event EventHandler ConnectionMade;
        public event EventHandler ConnectionTermainated;
        public event EventHandler<ExceptionEventArgs> LostConnection;
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<MessageEventArgs> MessageSending;

        private ServiceHost _host;
        private IBitTunnelService _service;

        List<IService> _services;

        private bool _connected;

        public Connection(ConnectionType type, AppSettings settings) : this(type, settings, "") { }

        public Connection(ConnectionType type, AppSettings settings, string publicName)
        {
            _services = new List<IService>();
            LoadNetworkService(type, settings, publicName);
        }

        private void LoadNetworkService(ConnectionType type, AppSettings settings, string publicName)
        {
            if (type == ConnectionType.Unknown) throw new InvalidOperationException("Unknown Connection Type passed to Connection");

            if (type == ConnectionType.Client)
            {
                _service = new BitTunnelClientService(settings, publicName);
            }
            else
            {
                _service = new BitTunnelServerService(settings, publicName);                
            }

            _service.LostConnection += new EventHandler<ExceptionEventArgs>(_service_LostConnection);
            _service.MessageReceived += new EventHandler<MessageEventArgs>(_service_MessageReceived);
            _service.MessageSending += new EventHandler<MessageEventArgs>(_service_MessageSending);

            _services.Add(_service);

        }

        public void AddCustomService(IService service)
        {
            if (_connected) throw new InvalidOperationException("Services cannot be added once the connection is open");

            _services.Add(service);
        }

        public bool IsConnected
        {
            get
            {
                return _connected;
            }            
        }

        public bool Connect()
        {
            _service.ConnectionCompleted += new EventHandler(_service_ConnectionCompleted);

            if (_connected)
            {
                Disconnect();
            }

            _host = new ServiceHost();//Rebuild in case of show stopping exception killing the previous implementation            

            foreach (IService service in _services)
            {
                _host.AddService(_service);    
            }
            
            _host.StartServiceHost();
            _host.OpenServices();

            DateTime now = DateTime.Now;

            DateTime toolate = now.AddMilliseconds(_service.Settings.ClientConnectionTimeout);

            while (_connected == false && DateTime.Now < toolate)
            {
                Thread.Sleep(10);
            }

            _service.ConnectionCompleted -= new EventHandler(_service_ConnectionCompleted);

            if (_connected && ConnectionMade != null)
            {
                ConnectionMade(this, EventArgs.Empty);
            }

            return _connected;
        }

        void _service_ConnectionCompleted(object sender, EventArgs e)
        {
            _connected = true;
        }
        
        public void Disconnect()
        {
            if (_connected)
            {                
                try
                {                         
                    _host.CloseServices();                   
                    _host.StopServiceHost();
                    
                    _host.Dispose();
                    
                }
                finally
                {
                    _connected = false;

                    if (ConnectionTermainated != null)
                    {
                        ConnectionTermainated(this, EventArgs.Empty);
                    }                    
                }
            }
        }

        void _service_LostConnection(object sender, ExceptionEventArgs e)
        {            
            _connected = false;

            if (LostConnection != null)
            {                
                LostConnection(sender, e);
            }            
        }

        void _service_MessageReceived(object sender, MessageEventArgs e)
        {
            if (MessageReceived != null)
            {
                MessageReceived(sender, e);
            }
        }

        void _service_MessageSending(object sender, MessageEventArgs e)
        {
            if (MessageSending != null)
            {
                MessageSending(sender, e);
            }
        }        

        public IBitTunnelService Service
        {
            get { return _service; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_connected)
            {
                Disconnect();
            }
        }

        #endregion
    }
}
