using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorService;
using HelloWorldClient.Services.Protocol;
using System.Net;

namespace HelloWorldClient.Services
{
    public class MessageClientService : IService
    {        
        MessageClientProtocolFactory _factory;

        public MessageClientService()
        {
           
        }

        #region IService Members

        public void Close(IServiceHost host)
        {
            
        }

        public void Open(IServiceHost host)
        {            
            _factory = new MessageClientProtocolFactory();
            
            host.Reactor.ConnectStream(_factory, IPAddress.Parse("127.0.0.1"), 1969);
        }        

        public void SendMessage(string message)
        {
            _factory.SendMessage(message);
        }

        #endregion
    }
}
