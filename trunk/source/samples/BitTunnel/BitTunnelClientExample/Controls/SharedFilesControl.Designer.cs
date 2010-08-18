namespace BitTunnelClientExample.Controls
{
    partial class SharedFilesControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SharedFilesControl));
            this._tools = new System.Windows.Forms.ToolStrip();
            this._addFilesToolStripButton = new System.Windows.Forms.ToolStripButton();
            this._addFolderToolstripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._removeSelectedFiles = new System.Windows.Forms.ToolStripButton();
            this._sharedFilesView = new System.Windows.Forms.ListView();
            this._fileName = new System.Windows.Forms.ColumnHeader();
            this._location = new System.Windows.Forms.ColumnHeader();
            this._size = new System.Windows.Forms.ColumnHeader();
            this._state = new System.Windows.Forms.ColumnHeader();
            this._enabled = new System.Windows.Forms.ColumnHeader();
            this._folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this._fileDialog = new System.Windows.Forms.OpenFileDialog();
            this._tools.SuspendLayout();
            this.SuspendLayout();
            // 
            // _tools
            // 
            this._tools.Enabled = false;
            this._tools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._addFilesToolStripButton,
            this._addFolderToolstripButton,
            this.toolStripSeparator1,
            this._removeSelectedFiles});
            this._tools.Location = new System.Drawing.Point(0, 0);
            this._tools.Name = "_tools";
            this._tools.Size = new System.Drawing.Size(661, 25);
            this._tools.TabIndex = 0;
            this._tools.Text = "toolStrip1";
            // 
            // _addFilesToolStripButton
            // 
            this._addFilesToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("_addFilesToolStripButton.Image")));
            this._addFilesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._addFilesToolStripButton.Name = "_addFilesToolStripButton";
            this._addFilesToolStripButton.Size = new System.Drawing.Size(84, 22);
            this._addFilesToolStripButton.Text = "Add Files...";
            this._addFilesToolStripButton.Click += new System.EventHandler(this._addFilesToolStripButton_Click);
            // 
            // _addFolderToolstripButton
            // 
            this._addFolderToolstripButton.Image = ((System.Drawing.Image)(resources.GetObject("_addFolderToolstripButton.Image")));
            this._addFolderToolstripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._addFolderToolstripButton.Name = "_addFolderToolstripButton";
            this._addFolderToolstripButton.Size = new System.Drawing.Size(94, 22);
            this._addFolderToolstripButton.Text = "Add Folder...";
            this._addFolderToolstripButton.Click += new System.EventHandler(this._addFolderToolstripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _removeSelectedFiles
            // 
            this._removeSelectedFiles.Enabled = false;
            this._removeSelectedFiles.Image = ((System.Drawing.Image)(resources.GetObject("_removeSelectedFiles.Image")));
            this._removeSelectedFiles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._removeSelectedFiles.Name = "_removeSelectedFiles";
            this._removeSelectedFiles.Size = new System.Drawing.Size(91, 22);
            this._removeSelectedFiles.Text = "Remove File";
            this._removeSelectedFiles.Click += new System.EventHandler(this._removeSelectedFiles_Click);
            // 
            // _sharedFilesView
            // 
            this._sharedFilesView.AllowColumnReorder = true;
            this._sharedFilesView.AutoArrange = false;
            this._sharedFilesView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._fileName,
            this._location,
            this._size,
            this._state,
            this._enabled});
            this._sharedFilesView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sharedFilesView.FullRowSelect = true;
            this._sharedFilesView.HideSelection = false;
            this._sharedFilesView.Location = new System.Drawing.Point(0, 25);
            this._sharedFilesView.Name = "_sharedFilesView";
            this._sharedFilesView.Size = new System.Drawing.Size(661, 377);
            this._sharedFilesView.TabIndex = 1;
            this._sharedFilesView.UseCompatibleStateImageBehavior = false;
            this._sharedFilesView.View = System.Windows.Forms.View.Details;
            this._sharedFilesView.SelectedIndexChanged += new System.EventHandler(this._sharedFilesView_SelectedIndexChanged);
            // 
            // _fileName
            // 
            this._fileName.Text = "File Name";
            this._fileName.Width = 207;
            // 
            // _location
            // 
            this._location.Text = "Location";
            this._location.Width = 195;
            // 
            // _size
            // 
            this._size.Text = "Size";
            this._size.Width = 64;
            // 
            // _state
            // 
            this._state.Text = "State";
            this._state.Width = 71;
            // 
            // _enabled
            // 
            this._enabled.Text = "";
            this._enabled.Width = 94;
            // 
            // _fileDialog
            // 
            this._fileDialog.Filter = "All Files|*.*";
            this._fileDialog.Multiselect = true;
            // 
            // SharedFilesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._sharedFilesView);
            this.Controls.Add(this._tools);
            this.Name = "SharedFilesControl";
            this.Size = new System.Drawing.Size(661, 402);
            this._tools.ResumeLayout(false);
            this._tools.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip _tools;
        private System.Windows.Forms.ToolStripButton _addFilesToolStripButton;
        private System.Windows.Forms.ToolStripButton _addFolderToolstripButton;
        private System.Windows.Forms.ListView _sharedFilesView;
        private System.Windows.Forms.ColumnHeader _fileName;
        private System.Windows.Forms.ColumnHeader _location;
        private System.Windows.Forms.ColumnHeader _enabled;
        private System.Windows.Forms.FolderBrowserDialog _folderDialog;
        private System.Windows.Forms.OpenFileDialog _fileDialog;
        private System.Windows.Forms.ColumnHeader _state;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton _removeSelectedFiles;
        private System.Windows.Forms.ColumnHeader _size;
    }
}
