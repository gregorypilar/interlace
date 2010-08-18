using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Talcasoft.ReactorService;

namespace ObviousCode.Interlace.TestNestedFrames
{
    public class NestedFrameServerService : IService
    {
        NestedFrameServerProtocolFactory _factory;
        #region IService Members

        public void Close(IServiceHost host)
        {
            Console.WriteLine("Nested Frame Server Service stopping ...");
        }

        public void Open(IServiceHost host)
        {
            _factory = new NestedFrameServerProtocolFactory();

            host.Reactor.ListenStream(
                _factory, 1234
                );
        }

        #endregion
    }
}
