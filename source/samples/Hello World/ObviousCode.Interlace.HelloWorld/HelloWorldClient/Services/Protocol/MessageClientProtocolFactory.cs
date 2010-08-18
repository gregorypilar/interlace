using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorCore;
using Interlace.Erlang;

namespace HelloWorldClient.Services.Protocol
{
    public class MessageClientProtocolFactory : IProtocolFactory
    {
        MessageClientProtocol _protocol;

        public MessageClientProtocolFactory()
        {

        }

        #region IProtocolFactory Members

        public Interlace.ReactorCore.Protocol BuildProtocol()
        {
            _protocol = new MessageClientProtocol(this);

            return _protocol;
        }        

        public void StartedConnecting()
        {
            //Log event
        }

        public void ConnectionFailed(Exception ex) { }

        #endregion

        internal void SendMessage(string message)
        {
            _protocol.SendTerm(Atom.From(message));
        }


    }
}
