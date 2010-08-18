using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using System.Net;
using ObviousCode.Interlace.BitTunnelLibrary;
using System.Collections.Generic;
using ObviousCode.Interlace.BitTunnelLibrary.File;
using System.Threading;
using ObviousCode.Interlace.BitTunnelLibrary.Events;

namespace Knock.ViciMVC.Browser.Knock
{
    public class KnockClientManager
    {
        public enum Status { NotAvailable, Disconnected, Connected }
        static KnockClientManager _instance;

        AppSettings _settings;

        ClientInstance _client;

        public KnockClientManager()
        {
            _settings = new AppSettings();
        }

        public Status ClientStatus
        {
            get
            {
                return _client == null ? Status.NotAvailable :
                    (_client.IsConnected ? Status.Connected : Status.Disconnected);
            }
        }

        public IPAddress ServerAddress 
        {
            get { return _settings.ServerAddress; }
            set { _settings.ServerAddress = value; }
        }

        public int Port
        {
            get { return _settings.Port; }
            set { _settings.Port = value; }
        }
        
        public static KnockClientManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new KnockClientManager();
                }

                return _instance;
            }
        }
        
        internal void Connect()
        {
            _client = new ClientInstance(_settings);
            _client.Connect();
        }

        internal void Disconnect()
        {
            if (_client != null)
            {
                _client.Disconnect();
            }
        }

        internal List<DirectoryWrapper> GetFileList()
        {
            List<DirectoryWrapper> files = null;
            bool listed = false;
            EventHandler<FileListEventArgs> handler = delegate(object sender, FileListEventArgs e)
            {
                files = DirectoryWrapper.GetDirectories(e.FileList);

                listed = true;
            };

            _client.FullFileListReceived += new EventHandler<FileListEventArgs>(handler);

            _client.RequestFullFileList();

            DateTime then = DateTime.Now;

            while(!listed && ((TimeSpan)(DateTime.Now - then)).TotalMilliseconds < 1000)
            {
                Thread.Sleep(100);
            }

            _client.FullFileListReceived -= new EventHandler<FileListEventArgs>(handler);

            return files;
        }
    }
}
