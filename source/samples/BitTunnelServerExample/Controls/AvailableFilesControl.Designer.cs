namespace BitTunnelServerExample.Controls
{
    partial class AvailableFilesControl
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
            this._availableFilesView = new System.Windows.Forms.ListView();
            this._fileName = new System.Windows.Forms.ColumnHeader();
            this._client = new System.Windows.Forms.ColumnHeader();
            this._hash = new System.Windows.Forms.ColumnHeader();
            this._size = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // _availableFilesView
            // 
            this._availableFilesView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._fileName,
            this._client,
            this._hash,
            this._size});
            this._availableFilesView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._availableFilesView.FullRowSelect = true;
            this._availableFilesView.Location = new System.Drawing.Point(0, 0);
            this._availableFilesView.Name = "_availableFilesView";
            this._availableFilesView.Size = new System.Drawing.Size(774, 476);
            this._availableFilesView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this._availableFilesView.TabIndex = 4;
            this._availableFilesView.UseCompatibleStateImageBehavior = false;
            this._availableFilesView.View = System.Windows.Forms.View.Details;
            // 
            // _fileName
            // 
            this._fileName.Text = "File";
            this._fileName.Width = 243;
            // 
            // _client
            // 
            this._client.Text = "Available From";
            this._client.Width = 209;
            // 
            // _hash
            // 
            this._hash.DisplayIndex = 3;
            this._hash.Text = "Hash";
            this._hash.Width = 349;
            // 
            // _size
            // 
            this._size.Text = "Size";
            this._size.Width = 71;
            // 
            // AvailableFilesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._availableFilesView);
            this.Name = "AvailableFilesControl";
            this.Size = new System.Drawing.Size(774, 476);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView _availableFilesView;
        private System.Windows.Forms.ColumnHeader _fileName;
        private System.Windows.Forms.ColumnHeader _client;
        private System.Windows.Forms.ColumnHeader _hash;
        private System.Windows.Forms.ColumnHeader _size;
    }
}
