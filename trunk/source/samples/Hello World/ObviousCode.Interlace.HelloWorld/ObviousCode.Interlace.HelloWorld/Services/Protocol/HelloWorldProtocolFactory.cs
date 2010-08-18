using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorCore;

namespace ObviousCode.InterlaceApps.HelloWorld.Services.Protocol
{
    public class HelloWorldProtocolFactory : IProtocolFactory
    {
        #region IProtocolFactory Members

        public Interlace.ReactorCore.Protocol BuildProtocol()
        {
            return new HelloWorldProtocol();
        }

        public void ConnectionFailed(Exception ex)
        {
            //Clean up if required
        }

        public void StartedConnecting()
        {
            Console.WriteLine("Connecting ...");  
        }

        #endregion
    }
}
