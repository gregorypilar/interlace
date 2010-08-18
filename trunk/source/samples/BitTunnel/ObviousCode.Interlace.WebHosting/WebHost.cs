using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.IO;

namespace WebHost
{
    public class WebHost : IDisposable
    {        
        ApplicationManager _manager;
        HttpHost _listenerHost;

        bool _started = false;
        string _id = Guid.NewGuid().ToString("N");

        public WebHost()
        {
            _manager = ApplicationManager.GetApplicationManager();
                
        }

        public void Start()
        {
            if (_started) throw new InvalidOperationException("Web Host already started");

            _listenerHost = _manager.CreateObject(_id, typeof(HttpHost), "/", Directory.GetCurrentDirectory(), false, true) as HttpHost;

            _listenerHost.Start();

            _started = true;
        }

        public void Stop()
        {
            _started = false;

            _listenerHost.Stop(false);

            _manager.ShutdownApplication(_id);
            _manager.Close();
        }

        #region IDisposable Members

        public void Dispose()   
        {
            if (_started)
            {
                Stop();
            }
        }

        #endregion
    }
}
