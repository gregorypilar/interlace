#region Using Directives and Copyright Notice

// Copyright (c) 2006-2010, Bit Plantation (ABN 80 332 904 638)
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

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

#endregion

namespace Interlace.DatabaseManagement
{
	public class WindowsDatabaseUpgrader : System.Windows.Forms.Form
	{
		private System.Windows.Forms.PictureBox NormalIcon;
		private System.Windows.Forms.Label Prompt;
		private System.Windows.Forms.PictureBox WaitIcon;
		private System.Windows.Forms.PictureBox ErrorIcon;
		private System.Windows.Forms.ProgressBar ProgressBar;
		private System.Windows.Forms.Label ProgressLabel;
		private System.Windows.Forms.Button NextButton;
		private System.Windows.Forms.Button LeaveButton;

		private System.ComponentModel.Container components = null;

		public WindowsDatabaseUpgrader()
		{
			InitializeComponent();
		}

		DatabaseManager _manager = null;
		DatabaseManager.UpgradeDelegate _upgrade;
		string _fromVersion;
		string _toVersion;

		public static bool CheckDatabase(IDbConnection connection, IDatabaseImplementation implementation, IDatabaseSchemaFactory databaseSchemaFactory, string requiredProduct, string requiredVersion)
		{
			DatabaseManager manager = new DatabaseManager(connection, implementation, databaseSchemaFactory, requiredProduct);

			string fromVersion;	

			// Get the current version of the database:
			if (manager.VersionTableExists())
			{
				try
				{
					DatabaseVersion version = manager.GetVersion();

					fromVersion = version.Version;
				}
				catch (DatabaseVersionTableInvalidException ex)
				{
					using (WindowsDatabaseUpgrader upgrader = new WindowsDatabaseUpgrader())
					{
						upgrader.ShowError("Database Version Problem", ex.Message);
						upgrader.ShowDialog();

						return false;
					}
				}
			}
			else
			{
				fromVersion = DatabaseManager.UninitialisedVersion;
			}

			// Exit unless an upgrade is needed:
			if (fromVersion == requiredVersion) return true;

			// Try to upgrade:
			if (!manager.CanUpgrade(fromVersion, requiredVersion))
			{
				using (WindowsDatabaseUpgrader upgrader = new WindowsDatabaseUpgrader())
				{
					upgrader.ShowError("No Upgrade Available", String.Format(
						"There is no upgrade available from version {0} to " +
						"version {1}. This database can not be used.",
						fromVersion, requiredVersion));
					upgrader.ShowDialog();

					return false;
				}
			}

			using (WindowsDatabaseUpgrader upgrader = new WindowsDatabaseUpgrader())
			{
				upgrader._manager = manager;
				upgrader._fromVersion = fromVersion;
				upgrader._toVersion = requiredVersion;

				DialogResult result = upgrader.ShowDialog();

				return result == DialogResult.OK;
			}
		}

		private void DatabaseUpgrader_Load(object sender, System.EventArgs e)
		{
			if (_manager != null)
			{
				if (_fromVersion == DatabaseManager.UninitialisedVersion)
				{
					ShowInitialisation();
				}
				else
				{
					ShowUpgrade();
				}
			}
		}

		private void NextButton_Click(object sender, System.EventArgs e)
		{
			_upgrade = new DatabaseManager.UpgradeDelegate(_manager.Upgrade);

			ShowUpgrading();

			_upgrade.BeginInvoke(_fromVersion, _toVersion, 
				new DatabaseManager.UpgradeProgressDelegate(UpgradeProgressInvoke),
				new AsyncCallback(UpdateCompletedInvoke), null);
		}

		private void UpdateCompleted(IAsyncResult result)
		{
			try
			{
				_upgrade.EndInvoke(result);
			}
			catch (Exception)
			{
				ShowError("Upgrading Error", 
					"An error occurred while upgrading the database.");

				return;
			}

			DialogResult = DialogResult.OK;
		}

		private void UpdateCompletedInvoke(IAsyncResult result)
		{
			System.Threading.Thread.Sleep(new TimeSpan(0, 0, 3));

			Invoke(new AsyncCallback(UpdateCompleted), new object[] {result});
		}

		protected void UpgradeProgress(int commandsRun, int totalCommands)
		{
			ProgressBar.Value = commandsRun;
			ProgressBar.Minimum = 0;
			ProgressBar.Maximum = totalCommands;

			if (commandsRun < totalCommands) 
			{
				ProgressLabel.Text = String.Format("{0} of {1} commands executed.", 
					commandsRun, totalCommands);
			}
			else
			{
				ProgressLabel.Text = "All commands executed.";
			}
		}

		protected void UpgradeProgressInvoke(int commandsRun, int totalCommands)
		{
			Invoke(new DatabaseManager.UpgradeProgressDelegate(
				UpgradeProgress), new object[] {commandsRun, totalCommands});
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		public void ShowInitialisation()
		{
			Text = "Database Initialisation Required";
			NormalIcon.Visible = true;
			ErrorIcon.Visible = false;
			WaitIcon.Visible = false;
			Prompt.Text = "The database you have connected to has not been prepared for use " +
				"by this application. Click continue to prepare the database and start using it.";

			ProgressBar.Visible = false;
			ProgressLabel.Visible = false;

			NextButton.Visible = true;
			NextButton.Text = "&Continue";

			LeaveButton.Visible = true;
			LeaveButton.Text = "&Cancel";
		}

		public void ShowUpgrade()
		{
			Text = "Database Upgrade Required";
			NormalIcon.Visible = true;
			ErrorIcon.Visible = false;
			WaitIcon.Visible = false;
			Prompt.Text = "The database you have connected to must be upgraded to work with " +
				"this application. Click continue to upgrade the database and start using it.";

			ProgressBar.Visible = false;
			ProgressLabel.Visible = false;

			NextButton.Visible = true;
			NextButton.Text = "&Continue";

			LeaveButton.Visible = true;
			LeaveButton.Text = "&Cancel";
		}

		public void ShowWaiting()
		{
			Text = "Upgrading Database";
			NormalIcon.Visible = false;
			ErrorIcon.Visible = false;
			WaitIcon.Visible = true;
			Prompt.Text = "The database is in use by one or more other computers. Close this " +
			  "application on all computers to continue.";

			ProgressBar.Visible = false;
			ProgressLabel.Visible = false;

			NextButton.Visible = false;

			LeaveButton.Visible = true;
			LeaveButton.Text = "&Cancel";
		}

		public void ShowUpgrading()
		{
			Text = "Upgrading Database";
			NormalIcon.Visible = true;
			ErrorIcon.Visible = false;
			WaitIcon.Visible = false;
			Prompt.Text = "The database is currently being updated.";

			ProgressBar.Visible = true;
			ProgressLabel.Visible = true;

			NextButton.Visible = false;
			LeaveButton.Visible = false;
		}

		public void ShowError(string title, string message)
		{
			Text = title;
			NormalIcon.Visible = false;
			ErrorIcon.Visible = true;
			WaitIcon.Visible = false;
			Prompt.Text = message;

			ProgressBar.Visible = false;
			ProgressLabel.Visible = false;

			NextButton.Visible = false;

			LeaveButton.Visible = true;
			LeaveButton.Text = "&Close";
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WindowsDatabaseUpgrader));
            this.NormalIcon = new System.Windows.Forms.PictureBox();
            this.Prompt = new System.Windows.Forms.Label();
            this.WaitIcon = new System.Windows.Forms.PictureBox();
            this.ErrorIcon = new System.Windows.Forms.PictureBox();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.NextButton = new System.Windows.Forms.Button();
            this.LeaveButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.NormalIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.WaitIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // NormalIcon
            // 
            this.NormalIcon.Image = ((System.Drawing.Image)(resources.GetObject("NormalIcon.Image")));
            this.NormalIcon.Location = new System.Drawing.Point(8, 8);
            this.NormalIcon.Name = "NormalIcon";
            this.NormalIcon.Size = new System.Drawing.Size(32, 32);
            this.NormalIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.NormalIcon.TabIndex = 0;
            this.NormalIcon.TabStop = false;
            this.NormalIcon.Visible = false;
            // 
            // Prompt
            // 
            this.Prompt.Location = new System.Drawing.Point(48, 8);
            this.Prompt.Name = "Prompt";
            this.Prompt.Size = new System.Drawing.Size(384, 48);
            this.Prompt.TabIndex = 1;
            this.Prompt.Text = "(Prompt)";
            // 
            // WaitIcon
            // 
            this.WaitIcon.Image = ((System.Drawing.Image)(resources.GetObject("WaitIcon.Image")));
            this.WaitIcon.Location = new System.Drawing.Point(8, 8);
            this.WaitIcon.Name = "WaitIcon";
            this.WaitIcon.Size = new System.Drawing.Size(32, 32);
            this.WaitIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.WaitIcon.TabIndex = 2;
            this.WaitIcon.TabStop = false;
            this.WaitIcon.Visible = false;
            // 
            // ErrorIcon
            // 
            this.ErrorIcon.Image = ((System.Drawing.Image)(resources.GetObject("ErrorIcon.Image")));
            this.ErrorIcon.Location = new System.Drawing.Point(8, 8);
            this.ErrorIcon.Name = "ErrorIcon";
            this.ErrorIcon.Size = new System.Drawing.Size(32, 32);
            this.ErrorIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.ErrorIcon.TabIndex = 3;
            this.ErrorIcon.TabStop = false;
            this.ErrorIcon.Visible = false;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Location = new System.Drawing.Point(48, 56);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(320, 16);
            this.ProgressBar.TabIndex = 4;
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.Location = new System.Drawing.Point(48, 80);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(320, 16);
            this.ProgressLabel.TabIndex = 5;
            this.ProgressLabel.Text = "(ProgressLabel)";
            // 
            // NextButton
            // 
            this.NextButton.Location = new System.Drawing.Point(256, 104);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(80, 23);
            this.NextButton.TabIndex = 6;
            this.NextButton.Text = "(Next)";
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // LeaveButton
            // 
            this.LeaveButton.Location = new System.Drawing.Point(344, 104);
            this.LeaveButton.Name = "LeaveButton";
            this.LeaveButton.Size = new System.Drawing.Size(80, 23);
            this.LeaveButton.TabIndex = 7;
            this.LeaveButton.Text = "(Leave)";
            this.LeaveButton.Click += new System.EventHandler(this.LeaveButton_Click);
            // 
            // DatabaseUpgrader
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(432, 134);
            this.ControlBox = false;
            this.Controls.Add(this.LeaveButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.ProgressLabel);
            this.Controls.Add(this.ProgressBar);
            this.Controls.Add(this.ErrorIcon);
            this.Controls.Add(this.WaitIcon);
            this.Controls.Add(this.Prompt);
            this.Controls.Add(this.NormalIcon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DatabaseUpgrader";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Upgrade Required";
            this.Load += new System.EventHandler(this.DatabaseUpgrader_Load);
            ((System.ComponentModel.ISupportInitialize)(this.NormalIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.WaitIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void LeaveButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
