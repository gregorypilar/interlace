using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.Erlang;
using ObviousCode.InterlaceApps.HelloWorld.Services.Protocol;
using Interlace.PropertyLists;
using Interlace.ReactorCore;

namespace HelloWorldClient.Services.Protocol
{
    public class MessageClientProtocol : TermProtocol
    {
        MessageClientProtocolFactory _parent;

        public MessageClientProtocol(MessageClientProtocolFactory parent)
        {
            _parent = parent;
        }        

        protected override void ConnectionMade()
        {
            //Log event
            base.ConnectionMade();
        }

        protected override void ConnectionLost(CloseReason reason)
        {            
            Console.WriteLine("Connection Lost: {0}", reason);
            
            base.ConnectionLost(reason);
        }

        protected override void TermReceived(object term)
        {
            PropertyDictionary dictionary = PropertyDictionary.FromString((term as Atom).Value);

            Console.WriteLine("{0}", dictionary["Message"]);
        }
    }
}
