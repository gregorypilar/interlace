namespace BitTunnelServerExample
{
    partial class ServerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerForm));
            this._menu = new System.Windows.Forms.TreeView();
            this._split = new System.Windows.Forms.SplitContainer();
            this._availableFilesControl = new BitTunnelServerExample.Controls.AvailableFilesControl();
            this._loggingControl = new BitTunnelServerExample.LoggingControl();
            this._settingsControl = new BitTunnelServerExample.SettingsControl();
            this._tools = new System.Windows.Forms.ToolStrip();
            this._actionButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._exitButton = new System.Windows.Forms.ToolStripButton();
            this._split.Panel1.SuspendLayout();
            this._split.Panel2.SuspendLayout();
            this._split.SuspendLayout();
            this._tools.SuspendLayout();
            this.SuspendLayout();
            // 
            // _menu
            // 
            this._menu.Dock = System.Windows.Forms.DockStyle.Fill;
            this._menu.Location = new System.Drawing.Point(0, 0);
            this._menu.Name = "_menu";
            this._menu.Size = new System.Drawing.Size(233, 365);
            this._menu.TabIndex = 0;
            // 
            // _split
            // 
            this._split.Dock = System.Windows.Forms.DockStyle.Fill;
            this._split.Location = new System.Drawing.Point(0, 25);
            this._split.Name = "_split";
            // 
            // _split.Panel1
            // 
            this._split.Panel1.Controls.Add(this._menu);
            // 
            // _split.Panel2
            // 
            this._split.Panel2.Controls.Add(this._availableFilesControl);
            this._split.Panel2.Controls.Add(this._loggingControl);
            this._split.Panel2.Controls.Add(this._settingsControl);
            this._split.Size = new System.Drawing.Size(700, 365);
            this._split.SplitterDistance = 233;
            this._split.TabIndex = 1;
            // 
            // _availableFilesControl
            // 
            this._availableFilesControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._availableFilesControl.Location = new System.Drawing.Point(0, 0);
            this._availableFilesControl.Name = "_availableFilesControl";
            this._availableFilesControl.Server = null;
            this._availableFilesControl.Size = new System.Drawing.Size(463, 365);
            this._availableFilesControl.TabIndex = 2;
            // 
            // _loggingControl
            // 
            this._loggingControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._loggingControl.Location = new System.Drawing.Point(0, 0);
            this._loggingControl.Name = "_loggingControl";
            this._loggingControl.Server = null;
            this._loggingControl.Settings = null;
            this._loggingControl.Size = new System.Drawing.Size(463, 365);
            this._loggingControl.TabIndex = 0;
            this._loggingControl.Load += new System.EventHandler(this._loggingControl_Load);
            // 
            // _settingsControl
            // 
            this._settingsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._settingsControl.Location = new System.Drawing.Point(0, 0);
            this._settingsControl.Name = "_settingsControl";
            this._settingsControl.Server = null;
            this._settingsControl.Settings = null;
            this._settingsControl.Size = new System.Drawing.Size(463, 365);
            this._settingsControl.TabIndex = 1;
            // 
            // _tools
            // 
            this._tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._actionButton,
            this.toolStripSeparator1,
            this._exitButton});
            this._tools.Location = new System.Drawing.Point(0, 0);
            this._tools.Name = "_tools";
            this._tools.Size = new System.Drawing.Size(700, 25);
            this._tools.TabIndex = 2;
            this._tools.Text = "toolStrip1";
            // 
            // _actionButton
            // 
            this._actionButton.Image = ((System.Drawing.Image)(resources.GetObject("_actionButton.Image")));
            this._actionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._actionButton.Name = "_actionButton";
            this._actionButton.Size = new System.Drawing.Size(62, 22);
            this._actionButton.Text = "Action";
            this._actionButton.Click += new System.EventHandler(this._actionButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _exitButton
            // 
            this._exitButton.Image = ((System.Drawing.Image)(resources.GetObject("_exitButton.Image")));
            this._exitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._exitButton.Name = "_exitButton";
            this._exitButton.Size = new System.Drawing.Size(45, 22);
            this._exitButton.Text = "Exit";
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 390);
            this.Controls.Add(this._split);
            this.Controls.Add(this._tools);
            this.Name = "ServerForm";
            this.Text = "Bit Tunnel Server";
            this.Load += new System.EventHandler(this.ServerForm_Load);
            this._split.Panel1.ResumeLayout(false);
            this._split.Panel2.ResumeLayout(false);
            this._split.ResumeLayout(false);
            this._tools.ResumeLayout(false);
            this._tools.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView _menu;
        private System.Windows.Forms.SplitContainer _split;
        private System.Windows.Forms.ToolStrip _tools;
        private System.Windows.Forms.ToolStripButton _actionButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton _exitButton;
        private SettingsControl _settingsControl;
        private LoggingControl _loggingControl;
        private BitTunnelServerExample.Controls.AvailableFilesControl _availableFilesControl;


    }
}

