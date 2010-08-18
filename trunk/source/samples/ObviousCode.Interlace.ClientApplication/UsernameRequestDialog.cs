using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ObviousCode.Interlace.ClientApplication
{
    public partial class UsernameRequestDialog : Form
    {
        public UsernameRequestDialog()
        {
            InitializeComponent();
        }

        public string RequestedUsername
        {
            get
            {
                return _username.Text;
            }
        }

        private void _username_KeyUp(object sender, KeyEventArgs e)
        {
            _ok.Enabled = _username.Text.Trim().Length > 0;

            if (e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        
    }
}
