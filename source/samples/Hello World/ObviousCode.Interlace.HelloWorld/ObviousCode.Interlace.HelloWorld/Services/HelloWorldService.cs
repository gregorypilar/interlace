using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interlace.ReactorService;
using ObviousCode.InterlaceApps.HelloWorld.Services.Protocol;

namespace ObviousCode.InterlaceApps.HelloWorld.Services
{
    class HelloWorldService : IService
    {       
        #region IService Members

        public void Close(IServiceHost host)
        {
            
        }

        public void Open(IServiceHost host)
        {
            host.Reactor.ListenStream(new HelloWorldProtocolFactory(), 1969);
        }

        #endregion
    }
}
