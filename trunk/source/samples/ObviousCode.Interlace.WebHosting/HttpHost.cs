using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Net;
using System.Threading;
using ObviousCode.Interlace.BitTunnel.Connectivity;

namespace WebHost
{
    public class HttpHost : MarshalByRefObject, IRegisteredObject
    {        
        Thread _listenThread;
        HttpListener _listener;

        bool _listening = false;

        public HttpHost()
        {
            HostingEnvironment.RegisterObject(this);

            _listener = null;
            _listening = false;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {            
            _listening = true;

            _listenThread = new Thread((ThreadStart)StartListening);
            _listenThread.Name = "Http Listener";
        }

        private void StartListening()
        {

        }

        #region IRegisteredObject Members

        public void Stop(bool immediate)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
