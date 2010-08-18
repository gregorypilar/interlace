using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;

namespace BitTunnelClientExample
{
    public partial class ConnectionDetailsDialog : Form
    {
        IPAddress _address;
        int? _portNumber;

        public ConnectionDetailsDialog()
        {
            InitializeComponent();
        }

        private void _ipAddress_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(_ipAddress.Text))
            {
                _errorProvider.SetError(_ipAddress, "Please enter IP Address");
                Address = null;
            }

            IPAddress address;

            if (!IPAddress.TryParse(_ipAddress.Text.Trim(), out address))
            {
                _errorProvider.SetError(_ipAddress, "Bad IP Address");                
            }
            else
            {
                _errorProvider.Clear();
            }
            Address = address;
        }

        private void _port_Validating(object sender, CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(_port.Text))
            {
                _errorProvider.SetError(_port, "Please enter Port");
                Port = null;
            }

            int port;

            if (!Int32.TryParse(_port.Text.Trim(), out port))
            {
                _errorProvider.SetError(_port, "Bad Port");                
            }
            else
            {
                _errorProvider.Clear();
            }
            Port = port;
        }

        public IPAddress Address
        {
            get { return _address;  }
            private set
            {
                _address = value;
                CheckOKEnabled();
            }
        }

        public int? Port
        {
            get { return _portNumber; }
            private set
            {
                _portNumber = value;

                CheckOKEnabled();
            }
        }


        private void CheckOKEnabled()
        {
            _ok.Enabled = Port.HasValue && Address != null;
        }        
    }
}
