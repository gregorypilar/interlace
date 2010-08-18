using System;
using System.Collections.Generic;
using Interlace.ReactorCore;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using ObviousCode.Interlace.BitTunnelLibrary.Interfaces;

namespace ObviousCode.Interlace.BitTunnelLibrary.Protocols
{
    public abstract class BitTunnelProtocolFactory : IProtocolFactory
    {
        List<BitTunnelProtocol> _protocols;        
        public event EventHandler<ExceptionEventArgs> LostConnection;
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<MessageEventArgs> MessageSending;

        AppSettings _settings;

        public BitTunnelProtocolFactory(AppSettings settings)
        {
            _protocols = new List<BitTunnelProtocol>();
            _settings = settings;
        }

        #region IProtocolFactory Members

        public Protocol BuildProtocol()
        {
            BitTunnelProtocol protocol = CreateProtocol();

            _protocols.Add(protocol);

            protocol.MessageReceived += new EventHandler<MessageEventArgs>(_protocol_MessageReceived);
            protocol.MessageSending += new EventHandler<MessageEventArgs>(protocol_MessageSending);
            
            return protocol;
        }
        

        void protocol_MessageSending(object sender, MessageEventArgs e)
        {
            if (MessageSending != null)
            {
                MessageSending(sender, e);
            }
        }

        public void ConnectionFailed(Exception ex)
        {           
            OnConnectionFailed(ex);
            
            if (LostConnection != null)
            {
                LostConnection(this, new ExceptionEventArgs(ex));
            }
        }

        protected abstract BitTunnelProtocol CreateProtocol();
        
        public virtual void StartedConnecting() 
        {

        }
        
        public abstract void OnConnectionFailed(Exception e);
        public abstract void OnMessageReceived(IMessage message);

        void _protocol_MessageReceived(object sender, MessageEventArgs e)
        {            
            OnMessageReceived(e.Message);

            if (MessageReceived != null)
            {
                MessageReceived(sender, e);
            }

        }

        #endregion

        protected List<BitTunnelProtocol> ProtocolList
        {
            get { return _protocols; }
        }

        protected AppSettings Settings
        {
            get { return _settings; }
        }

        public void LoseConnection()//set this to internal then mark BitTunnel as an allowed assembly
        {
            foreach (BitTunnelProtocol protocol in ProtocolList)
            {
                protocol.LoseConnection();
            }
        }        
    }
}
