#region Using Directives and Copyright Notice

// Copyright (c) 2007-2010, Computer Consultancy Pty Ltd
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the Computer Consultancy Pty Ltd nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
// OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.

#endregion

namespace Interlace.ReactorUtilities
{
    partial class DeferredProgressBar
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeferredProgressBar));
            this._marqueeProgressBar = new DevExpress.XtraEditors.MarqueeProgressBarControl();
            this._statusLabel = new DevExpress.XtraEditors.LabelControl();
            this._progressBar = new DevExpress.XtraEditors.ProgressBarControl();
            ((System.ComponentModel.ISupportInitialize)(this._marqueeProgressBar.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._progressBar.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // _marqueeProgressBar
            // 
            this._marqueeProgressBar.EditValue = 0;
            this._marqueeProgressBar.Location = new System.Drawing.Point(21, 48);
            this._marqueeProgressBar.Name = "_marqueeProgressBar";
            this._marqueeProgressBar.Size = new System.Drawing.Size(421, 18);
            this._marqueeProgressBar.TabIndex = 5;
            // 
            // _statusLabel
            // 
            this._statusLabel.Appearance.Options.UseTextOptions = true;
            this._statusLabel.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this._statusLabel.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this._statusLabel.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this._statusLabel.Location = new System.Drawing.Point(21, 85);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(421, 35);
            this._statusLabel.TabIndex = 4;
            this._statusLabel.Text = "[Status]";
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(21, 48);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(421, 18);
            this._progressBar.TabIndex = 3;
            // 
            // DeferredProgressBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(463, 133);
            this.Controls.Add(this._marqueeProgressBar);
            this.Controls.Add(this._statusLabel);
            this.Controls.Add(this._progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DeferredProgressBar";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Operation In Progress";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DeferredProgressBar_FormClosing);
            this.Load += new System.EventHandler(this.DeferredProgressBar_Load);
            ((System.ComponentModel.ISupportInitialize)(this._marqueeProgressBar.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._progressBar.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.MarqueeProgressBarControl _marqueeProgressBar;
        private DevExpress.XtraEditors.LabelControl _statusLabel;
        private DevExpress.XtraEditors.ProgressBarControl _progressBar;

    }
}
