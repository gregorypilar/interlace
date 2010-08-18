using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BitTunnelServerExample.Properties;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary;

namespace BitTunnelServerExample
{
    public partial class SettingsControl : UserControl, IServerControl
    {        
        public SettingsControl()
        {
            InitializeComponent();
        }
        public SettingsControl(ServerInstance server, AppSettings settings) : this()
        {
            Settings = settings;
            _settings.SelectedObject = Settings;
        }

        public ServerInstance Server { get; set; }
        public AppSettings Settings 
        { 
            get
            {
                return _settings.SelectedObject as AppSettings;
            }
            set
            {
                _settings.SelectedObject = value;
            }
        }



        #region IServerControl Members

        public Image MenuIcon
        {
            get { return null; }
        }

        public string MenuText
        {
            get { return "Settings"; }
        }

        #endregion
    }
}
