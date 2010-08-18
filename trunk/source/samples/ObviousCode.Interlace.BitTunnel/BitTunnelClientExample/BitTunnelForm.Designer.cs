namespace BitTunnelClientExample
{
    partial class BitTunnelForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BitTunnelForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._mainTabs = new System.Windows.Forms.TabControl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._actionButton = new System.Windows.Forms.ToolStripButton();
            this._infoTabs = new System.Windows.Forms.TabControl();
            this._actionButtonImages = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._mainTabs);
            this.splitContainer1.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this._infoTabs);
            this.splitContainer1.Size = new System.Drawing.Size(893, 454);
            this.splitContainer1.SplitterDistance = 297;
            this.splitContainer1.TabIndex = 0;
            // 
            // _mainTabs
            // 
            this._mainTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._mainTabs.Location = new System.Drawing.Point(0, 25);
            this._mainTabs.Name = "_mainTabs";
            this._mainTabs.SelectedIndex = 0;
            this._mainTabs.Size = new System.Drawing.Size(893, 272);
            this._mainTabs.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._actionButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(893, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _actionButton
            // 
            this._actionButton.Image = ((System.Drawing.Image)(resources.GetObject("_actionButton.Image")));
            this._actionButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._actionButton.Name = "_actionButton";
            this._actionButton.Size = new System.Drawing.Size(62, 22);
            this._actionButton.Text = "Action";
            this._actionButton.Click += new System.EventHandler(this._connectToolstripButton_Click);
            // 
            // _infoTabs
            // 
            this._infoTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this._infoTabs.Location = new System.Drawing.Point(0, 0);
            this._infoTabs.Name = "_infoTabs";
            this._infoTabs.SelectedIndex = 0;
            this._infoTabs.Size = new System.Drawing.Size(893, 153);
            this._infoTabs.TabIndex = 0;
            // 
            // _actionButtonImages
            // 
            this._actionButtonImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_actionButtonImages.ImageStream")));
            this._actionButtonImages.TransparentColor = System.Drawing.Color.Transparent;
            this._actionButtonImages.Images.SetKeyName(0, "Connect");
            this._actionButtonImages.Images.SetKeyName(1, "Disconnect");
            // 
            // BitTunnelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(893, 454);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BitTunnelForm";
            this.Text = "Bit Tunnel";
            this.Load += new System.EventHandler(this.BitTunnelForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BitTunnelForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl _infoTabs;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton _actionButton;
        private System.Windows.Forms.TabControl _mainTabs;
        private System.Windows.Forms.ImageList _actionButtonImages;
    }
}

