namespace ObviousCode.Interlace.ClientApplication
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._chatterList = new System.Windows.Forms.TreeView();
            this._chat = new System.Windows.Forms.ListBox();
            this._messageInput = new System.Windows.Forms.TextBox();
            this._statusStrip = new System.Windows.Forms.StatusStrip();
            this._status = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._changeConnectionStatus = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this._statusStrip.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._chatterList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this._chat);
            this.splitContainer1.Panel2.Controls.Add(this._messageInput);
            this.splitContainer1.Size = new System.Drawing.Size(757, 319);
            this.splitContainer1.SplitterDistance = 135;
            this.splitContainer1.TabIndex = 1;
            // 
            // _chatterList
            // 
            this._chatterList.Dock = System.Windows.Forms.DockStyle.Fill;
            this._chatterList.Location = new System.Drawing.Point(0, 0);
            this._chatterList.Name = "_chatterList";
            this._chatterList.Size = new System.Drawing.Size(135, 319);
            this._chatterList.TabIndex = 0;
            // 
            // _chat
            // 
            this._chat.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._chat.Dock = System.Windows.Forms.DockStyle.Fill;
            this._chat.Enabled = false;
            this._chat.FormattingEnabled = true;
            this._chat.Location = new System.Drawing.Point(0, 0);
            this._chat.Name = "_chat";
            this._chat.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this._chat.Size = new System.Drawing.Size(618, 299);
            this._chat.TabIndex = 5;
            // 
            // _messageInput
            // 
            this._messageInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._messageInput.Enabled = false;
            this._messageInput.Location = new System.Drawing.Point(0, 299);
            this._messageInput.Name = "_messageInput";
            this._messageInput.Size = new System.Drawing.Size(618, 20);
            this._messageInput.TabIndex = 4;
            this._messageInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this._messageInput_KeyDown);
            // 
            // _statusStrip
            // 
            this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._status});
            this._statusStrip.Location = new System.Drawing.Point(0, 344);
            this._statusStrip.Name = "_statusStrip";
            this._statusStrip.Size = new System.Drawing.Size(757, 22);
            this._statusStrip.TabIndex = 2;
            this._statusStrip.Text = "statusStrip1";
            // 
            // _status
            // 
            this._status.Name = "_status";
            this._status.Size = new System.Drawing.Size(88, 17);
            this._status.Text = "Not Connected";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._changeConnectionStatus});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(757, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _changeConnectionStatus
            // 
            this._changeConnectionStatus.Image = global::ObviousCode.Interlace.ClientApplication.Properties.Resources.PlayHS;
            this._changeConnectionStatus.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._changeConnectionStatus.Name = "_changeConnectionStatus";
            this._changeConnectionStatus.Size = new System.Drawing.Size(57, 22);
            this._changeConnectionStatus.Text = "Login";
            this._changeConnectionStatus.Click += new System.EventHandler(this._changeConnectionStatus_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 366);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this._statusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Interlace Chat";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this._mainForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this._statusStrip.ResumeLayout(false);
            this._statusStrip.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _status;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton _changeConnectionStatus;
        private System.Windows.Forms.TextBox _messageInput;
        private System.Windows.Forms.ListBox _chat;
        private System.Windows.Forms.TreeView _chatterList;
    }
}

