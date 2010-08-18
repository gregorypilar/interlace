using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ObviousCode.Interlace.BitTunnelLibrary;
using ObviousCode.Interlace.BitTunnel.Connectivity;
using System.Net;
using BitTunnelServerExample.Controls;

namespace BitTunnelServerExample
{
    public partial class ServerForm : Form
    {        
        AppSettings _settings;
        ServerInstance _instance;        

        List<IServerControl> _serverControls;

        public ServerForm()
        {
            InitializeComponent();

            _settings = new AppSettings();

            //Default values
            _settings.Port = 1234;
            _settings.ServerAddress = IPAddress.Parse("127.0.0.1");
            _settings.ServerIsRemote = false;            
            
            _instance = new ServerInstance(_settings);

            _instance.ConnectionMade += new EventHandler(_instance_ConnectionMade);
            _instance.ConnectionTerminated += new EventHandler(_instance_ConnectionTerminated);

            LoadControls();
            LoadMenu();

            SetupForConnection();
        }
        
        private void LoadControls()
        {
            _settingsControl.Server = _instance;
            _loggingControl.Server = _instance;
            _availableFilesControl.Server = _instance;

            _settingsControl.Settings = _settings;
            _loggingControl.Settings = _settings;                       

            _serverControls = new List<IServerControl>();

            _serverControls.Add(_settingsControl);
            _serverControls.Add(_loggingControl);
            _serverControls.Add(_availableFilesControl);
        }

        private void LoadMenu()
        {
            _menu.AfterSelect += new TreeViewEventHandler(_menu_AfterSelect);

            _menu.ImageList = new ImageList();

            _menu.ImageList.Images.Add(Images.Default);

            foreach (IServerControl control in _serverControls)
            {
                AddControlToMenu(control);
            }
        }        

        private void AddControlToMenu(IServerControl control)
        {
            int imageKey = 0;

            if (control.MenuIcon != null)
            {
                _menu.ImageList.Images.Add(control.MenuIcon);
                imageKey = _menu.ImageList.Images.Count - 1;
            }

            TreeNode node = new TreeNode(control.MenuText, imageKey, imageKey);

            node.Tag = control;

            _menu.Nodes.Add(node);
        }

        private void ViewControl(UserControl control)
        {
            control.BringToFront();
        }

        void _menu_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is UserControl)
            {
                ViewControl(e.Node.Tag as UserControl);
            }
        }
        
        private void _actionButton_Click(object sender, EventArgs e)
        {
            if (_instance.IsConnected)
            {
                _instance.Disconnect();
            }
            else
            {
                _instance.Connect();
            }
        }        

        void _instance_ConnectionTerminated(object sender, EventArgs e)
        {
            SetupForConnection();
        }

        void _instance_ConnectionMade(object sender, EventArgs e)
        {
            SetupForDisconnection();
        }        


        private void SetupForConnection()
        {
            _actionButton.Text = "Connect";
        }

        private void SetupForDisconnection()
        {
            _actionButton.Text = "Disconnect";
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            _instance.Dispose();
        }

        private void _loggingControl_Load(object sender, EventArgs e)
        {

        }
    }
}
