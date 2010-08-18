namespace BitTunnelServerExample
{
    partial class SettingsControl
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
            this._settings = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // _settings
            // 
            this._settings.Dock = System.Windows.Forms.DockStyle.Fill;
            this._settings.Location = new System.Drawing.Point(0, 0);
            this._settings.Name = "_settings";
            this._settings.Size = new System.Drawing.Size(690, 400);
            this._settings.TabIndex = 0;
            // 
            // SettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._settings);
            this.Name = "SettingsControl";
            this.Size = new System.Drawing.Size(690, 400);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid _settings;
    }
}
