using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;
using ObviousCode.Interlace.BitTunnelLibrary.Protocols;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using Interlace.ReactorService;
using Interlace.ReactorService;

namespace ObviousCode.Interlace.BitTunnelLibrary.Services
{
    public abstract class BitTunnelService : IBitTunnelService
    {
        BitTunnelProtocolFactory _factory;

        public event EventHandler ConnectionCompleted;//set this to internal then mark BitTunnel as an allowed assembly

        public event EventHandler<ExceptionEventArgs> LostConnection;
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<MessageEventArgs> MessageSending;
        public event EventHandler ConnectionTerminated;

        AppSettings _settings;

        public BitTunnelService(AppSettings settings)
        {
            _settings = settings;
        }

        public abstract void Close(IServiceHost host);

        public void Open(IServiceHost host)
        {
            OnServiceOpen(host);

            if (_factory != null)
            {
                _factory.LostConnection -= new EventHandler<ExceptionEventArgs>(_factory_LostConnection);
                _factory.MessageReceived -= new EventHandler<MessageEventArgs>(_factory_MessageReceived);
                _factory.MessageSending -= new EventHandler<MessageEventArgs>(_factory_MessageSending);
            }

            _factory = CreateFactory();

            _factory.LostConnection += new EventHandler<ExceptionEventArgs>(_factory_LostConnection);

            _factory.MessageReceived += new EventHandler<MessageEventArgs>(_factory_MessageReceived);

            _factory.MessageSending += new EventHandler<MessageEventArgs>(_factory_MessageSending);

            ConnectReactor(host);
        }


        protected virtual void OnServiceOpen(IServiceHost host)
        {

        }

        void _factory_MessageSending(object sender, MessageEventArgs e)
        {
            OnMessageSending(e.Message);
            if (MessageSending != null)
            {
                MessageSending(sender, e);
            }
        }

        void _factory_MessageReceived(object sender, MessageEventArgs e)
        {
            OnMessageReceived(e.Message);
            if (MessageReceived != null)
            {
                MessageReceived(sender, e);
            }
        }

        void _factory_LostConnection(object sender, ExceptionEventArgs e)
        {
            if (LostConnection != null)
            {
                //bubble bubble
                LostConnection(sender, e);
            }

            OnLostConnection(e.ThrownException);
        }

        protected abstract void OnLostConnection(Exception e);

        protected abstract void OnMessageReceived(IMessage message);

        protected abstract void OnMessageSending(IMessage message);

        protected abstract BitTunnelProtocolFactory CreateFactory();

        protected abstract void ConnectReactor(IServiceHost host);

        public abstract IInstance Instance { get; set; }

        protected void FireConnectionCompleted()
        {
            if (ConnectionCompleted != null)
            {
                ConnectionCompleted(this, EventArgs.Empty);
            }
        }

        public AppSettings Settings
        {
            get { return _settings; }
        }

        protected BitTunnelProtocolFactory Factory
        {
            get { return _factory; }
        }
    }      
}
