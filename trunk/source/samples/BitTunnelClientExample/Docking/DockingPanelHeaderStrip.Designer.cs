namespace BitTunnelClientExample.Docking
{
    partial class DockingPanelHeaderStrip
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
            this._title = new System.Windows.Forms.Label();
            this._dockUndock = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _title
            // 
            this._title.AutoSize = true;
            this._title.Location = new System.Drawing.Point(3, 0);
            this._title.Name = "_title";
            this._title.Size = new System.Drawing.Size(96, 13);
            this._title.TabIndex = 0;
            this._title.Text = "An unnamed panel";
            // 
            // _dockUndock
            // 
            this._dockUndock.Location = new System.Drawing.Point(129, -1);
            this._dockUndock.Name = "_dockUndock";
            this._dockUndock.Size = new System.Drawing.Size(18, 18);
            this._dockUndock.TabIndex = 1;
            this._dockUndock.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this._dockUndock.UseVisualStyleBackColor = true;
            // 
            // DockingPanelHeaderStrip
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._dockUndock);
            this.Controls.Add(this._title);
            this.Name = "DockingPanelHeaderStrip";
            this.Size = new System.Drawing.Size(150, 23);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _title;
        private System.Windows.Forms.Button _dockUndock;
    }
}
