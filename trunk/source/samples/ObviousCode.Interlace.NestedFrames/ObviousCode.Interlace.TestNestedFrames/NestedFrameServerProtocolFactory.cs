using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Talcasoft.ReactorCore;

namespace ObviousCode.Interlace.TestNestedFrames
{
    public class NestedFrameServerProtocolFactory : IProtocolFactory
    {
        NestedFrameServerProtocol _protocol;

        public NestedFrameServerProtocolFactory()
        {

        }

        #region IProtocolFactory Members

        public Protocol BuildProtocol()
        {
            _protocol = new NestedFrameServerProtocol();

            return _protocol;
        }

        public void ConnectionFailed(Exception ex)
        {
            Console.WriteLine("Connection Failed: {0}", ex.Message);
        }

        public void StartedConnecting()
        {
            Console.WriteLine("Connecting ...");
        }

        #endregion
    }
}
