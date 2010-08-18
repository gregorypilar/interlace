using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BitTunnelClientExample.Controls;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnelLibrary.Events;
using System.Net;

namespace BitTunnelClientExample
{
    public partial class BitTunnelForm : Form
    {
        AppSettings _settings;
        ClientInstance _client;
        List<IClientTabControl> _infoTabControls;
        List<IClientTabControl> _mainTabControls;
        public BitTunnelForm()
        {
            InitializeComponent();

            _settings = new AppSettings();

            //Default values
            _settings.Port = 1234;

            _settings.ServerAddress = IPAddress.Parse("127.0.0.1");
            _settings.ServerIsRemote = false;            

            _client = new ClientInstance(_settings);

            _client.ConnectionMade += new EventHandler(_client_ConnectionMade);
            _client.ConnectionTerminated += new EventHandler(_client_ConnectionTerminated);
            _client.LostConnection += new EventHandler<ExceptionEventArgs>(_client_LostConnection);

            AddInfoTabControls();
            AddMainTabControls();
            LoadInfoTabs();
            LoadMainTabs();
            SetupForConnection();
        }        
        
        void _client_LostConnection(object sender, ExceptionEventArgs e)
        {
            SetupForConnection();
        }

        void _client_ConnectionTerminated(object sender, EventArgs e)
        {
            SetupForConnection();
        }
        
        void _client_ConnectionMade(object sender, EventArgs e)
        {
            SetupForDisconnection();
        }

        private void SetupForConnection()
        {
            _actionButton.Text = "Connect";
            _actionButton.Image = _actionButtonImages.Images["Connect"];
        }

        private void SetupForDisconnection()
        {
            _actionButton.Text = "Disconnect";
            _actionButton.Image = _actionButtonImages.Images["Disconnect"];
        }        

        private void AddInfoTabControls()
        {
            _infoTabControls = new List<IClientTabControl>();

            _infoTabControls.Add(new LoggingControl(_client));
        }

        private void AddMainTabControls()
        {
            _mainTabControls = new List<IClientTabControl>();

            _mainTabControls.Add(new SharedFilesControl(_client));
            _mainTabControls.Add(new AvailableFilesClientControl(_client));
        }


        private void LoadInfoTabs()
        {
            foreach (IClientTabControl control in _infoTabControls)
            {
                TabPage page = new TabPage(control.TabText);

                control.Dock = DockStyle.Fill;
                control.Visible = true;

                page.Controls.Add(control as UserControl);
                _infoTabs.TabPages.Add(page);
            }
        }

        private void LoadMainTabs()
        {
            foreach (IClientTabControl control in _mainTabControls)
            {
                TabPage page = new TabPage(control.TabText);

                control.Dock = DockStyle.Fill;
                control.Visible = true;

                page.Controls.Add(control as UserControl);
                _mainTabs.TabPages.Add(page);
            }
        }

        private void _connectToolstripButton_Click(object sender, EventArgs e)
        {
            if (_client.IsConnected)
            {
                _client.Disconnect();
            }
            else
            {
                ConnectionDetailsDialog details = new ConnectionDetailsDialog();

                if (details.ShowDialog() == DialogResult.OK)
                {
                    _client.Settings.ServerAddress = details.Address;
                    _client.Settings.Port = details.Port.Value;

                    _client.Connect();
                }                
            }
        }

        private void BitTunnelForm_Load(object sender, EventArgs e)
        {
            
        }

        private void BitTunnelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _client.Dispose();            
        }
    }
}
