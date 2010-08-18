namespace BitTunnelClientExample.Controls
{
    partial class AvailableFilesClientControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AvailableFilesClientControl));
            this._availableFilesView = new System.Windows.Forms.ListView();
            this._fileName = new System.Windows.Forms.ColumnHeader();
            this._size = new System.Windows.Forms.ColumnHeader();
            this._state = new System.Windows.Forms.ColumnHeader();
            this._hash = new System.Windows.Forms.ColumnHeader();
            this._tools = new System.Windows.Forms.ToolStrip();
            this._downloadButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._refresh = new System.Windows.Forms.ToolStripButton();
            this._openTransferDirectory = new System.Windows.Forms.ToolStripButton();
            this._tools.SuspendLayout();
            this.SuspendLayout();
            // 
            // _availableFilesView
            // 
            this._availableFilesView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._fileName,
            this._size,
            this._state,
            this._hash});
            this._availableFilesView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._availableFilesView.FullRowSelect = true;
            this._availableFilesView.Location = new System.Drawing.Point(0, 25);
            this._availableFilesView.Name = "_availableFilesView";
            this._availableFilesView.Size = new System.Drawing.Size(654, 440);
            this._availableFilesView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this._availableFilesView.TabIndex = 0;
            this._availableFilesView.UseCompatibleStateImageBehavior = false;
            this._availableFilesView.View = System.Windows.Forms.View.Details;
            this._availableFilesView.SelectedIndexChanged += new System.EventHandler(this._availableFilesView_SelectedIndexChanged);
            // 
            // _fileName
            // 
            this._fileName.Text = "File";
            this._fileName.Width = 223;
            // 
            // _size
            // 
            this._size.Text = "Size";
            this._size.Width = 70;
            // 
            // _state
            // 
            this._state.Text = "State";
            this._state.Width = 102;
            // 
            // _hash
            // 
            this._hash.Text = "Hash";
            this._hash.Width = 237;
            // 
            // _tools
            // 
            this._tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._downloadButton,
            this._openTransferDirectory,
            this.toolStripSeparator1,
            this._refresh});
            this._tools.Location = new System.Drawing.Point(0, 0);
            this._tools.Name = "_tools";
            this._tools.Size = new System.Drawing.Size(654, 25);
            this._tools.TabIndex = 1;
            this._tools.Text = "toolStrip1";
            // 
            // _downloadButton
            // 
            this._downloadButton.Enabled = false;
            this._downloadButton.Image = ((System.Drawing.Image)(resources.GetObject("_downloadButton.Image")));
            this._downloadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._downloadButton.Name = "_downloadButton";
            this._downloadButton.Size = new System.Drawing.Size(113, 22);
            this._downloadButton.Text = "Get Selected File";
            this._downloadButton.Click += new System.EventHandler(this._downloadButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _refresh
            // 
            this._refresh.Enabled = false;
            this._refresh.Image = global::BitTunnelClientExample.Properties.Resources.RefreshDocViewHS;
            this._refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._refresh.Name = "_refresh";
            this._refresh.Size = new System.Drawing.Size(66, 22);
            this._refresh.Text = "Refresh";
            this._refresh.Click += new System.EventHandler(this._refresh_Click);
            // 
            // _openTransferDirectory
            // 
            this._openTransferDirectory.Image = ((System.Drawing.Image)(resources.GetObject("_openTransferDirectory.Image")));
            this._openTransferDirectory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._openTransferDirectory.Name = "_openTransferDirectory";
            this._openTransferDirectory.Size = new System.Drawing.Size(128, 22);
            this._openTransferDirectory.Text = "View Received Files";
            this._openTransferDirectory.Click += new System.EventHandler(this._openTransferDirectory_Click);
            // 
            // AvailableFilesClientControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._availableFilesView);
            this.Controls.Add(this._tools);
            this.Name = "AvailableFilesClientControl";
            this.Size = new System.Drawing.Size(654, 465);
            this._tools.ResumeLayout(false);
            this._tools.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView _availableFilesView;
        private System.Windows.Forms.ToolStrip _tools;
        private System.Windows.Forms.ToolStripButton _downloadButton;
        private System.Windows.Forms.ColumnHeader _fileName;
        private System.Windows.Forms.ColumnHeader _state;
        private System.Windows.Forms.ColumnHeader _hash;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton _refresh;
        private System.Windows.Forms.ColumnHeader _size;
        private System.Windows.Forms.ToolStripButton _openTransferDirectory;
    }
}
