using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BitTunnelClientExample.Docking
{
    public partial class DockingPanelHeaderStrip : UserControl
    {
        public DockingPanelHeaderStrip()
        {
            InitializeComponent();
            _dockUndock.Image = Images.Pinned;
        }
    }
}
