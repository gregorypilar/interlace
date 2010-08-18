namespace ObviousCode.Interlace.ClientApplication
{
    partial class UsernameRequestDialog
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
            this._username = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._ok = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _username
            // 
            this._username.Location = new System.Drawing.Point(12, 36);
            this._username.Name = "_username";
            this._username.Size = new System.Drawing.Size(284, 20);
            this._username.TabIndex = 0;
            this._username.KeyUp += new System.Windows.Forms.KeyEventHandler(this._username_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "What is your name?";
            // 
            // _ok
            // 
            this._ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._ok.Enabled = false;
            this._ok.Location = new System.Drawing.Point(221, 62);
            this._ok.Name = "_ok";
            this._ok.Size = new System.Drawing.Size(75, 23);
            this._ok.TabIndex = 2;
            this._ok.Text = "OK";
            this._ok.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(140, 62);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // UsernameRequestDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(308, 97);
            this.ControlBox = false;
            this.Controls.Add(this.button1);
            this.Controls.Add(this._ok);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._username);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UsernameRequestDialog";
            this.Text = "Please enter your name";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _username;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button _ok;
        private System.Windows.Forms.Button button1;
    }
}