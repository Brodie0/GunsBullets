namespace GunsBullets {
    partial class GameConfigurator {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.titleLabel = new System.Windows.Forms.Label();
            this.hostnameTextBox = new System.Windows.Forms.TextBox();
            this.serverLabel = new System.Windows.Forms.Label();
            this.portLabel = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.debugCheckBox = new System.Windows.Forms.CheckBox();
            this.fullscreenCheckBox = new System.Windows.Forms.CheckBox();
            this.launchButton = new System.Windows.Forms.Button();
            this.hostCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.titleLabel.Location = new System.Drawing.Point(12, 0);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(183, 36);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "GunsBullets";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // hostnameTextBox
            // 
            this.hostnameTextBox.Location = new System.Drawing.Point(12, 55);
            this.hostnameTextBox.Name = "hostnameTextBox";
            this.hostnameTextBox.Size = new System.Drawing.Size(130, 20);
            this.hostnameTextBox.TabIndex = 1;
            this.hostnameTextBox.Text = "127.0.0.1";
            // 
            // serverLabel
            // 
            this.serverLabel.AutoSize = true;
            this.serverLabel.Location = new System.Drawing.Point(9, 36);
            this.serverLabel.Name = "serverLabel";
            this.serverLabel.Size = new System.Drawing.Size(70, 13);
            this.serverLabel.TabIndex = 2;
            this.serverLabel.Text = "IP/Hostname";
            // 
            // portLabel
            // 
            this.portLabel.AutoSize = true;
            this.portLabel.Location = new System.Drawing.Point(145, 36);
            this.portLabel.Name = "portLabel";
            this.portLabel.Size = new System.Drawing.Size(26, 13);
            this.portLabel.TabIndex = 3;
            this.portLabel.Text = "Port";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(148, 55);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(50, 20);
            this.portTextBox.TabIndex = 4;
            this.portTextBox.Text = "8888";
            // 
            // debugCheckBox
            // 
            this.debugCheckBox.AutoSize = true;
            this.debugCheckBox.Checked = true;
            this.debugCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.debugCheckBox.Location = new System.Drawing.Point(110, 104);
            this.debugCheckBox.Name = "debugCheckBox";
            this.debugCheckBox.Size = new System.Drawing.Size(88, 17);
            this.debugCheckBox.TabIndex = 5;
            this.debugCheckBox.Text = "Debug Mode";
            this.debugCheckBox.UseVisualStyleBackColor = true;
            // 
            // fullscreenCheckBox
            // 
            this.fullscreenCheckBox.AutoSize = true;
            this.fullscreenCheckBox.Location = new System.Drawing.Point(12, 104);
            this.fullscreenCheckBox.Name = "fullscreenCheckBox";
            this.fullscreenCheckBox.Size = new System.Drawing.Size(74, 17);
            this.fullscreenCheckBox.TabIndex = 6;
            this.fullscreenCheckBox.Text = "Fullscreen";
            this.fullscreenCheckBox.UseVisualStyleBackColor = true;
            // 
            // launchButton
            // 
            this.launchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.launchButton.Location = new System.Drawing.Point(12, 127);
            this.launchButton.Name = "launchButton";
            this.launchButton.Size = new System.Drawing.Size(186, 23);
            this.launchButton.TabIndex = 7;
            this.launchButton.Text = "Launch!";
            this.launchButton.UseVisualStyleBackColor = true;
            this.launchButton.Click += new System.EventHandler(this.LaunchGame);
            // 
            // hostCheckBox
            // 
            this.hostCheckBox.AutoSize = true;
            this.hostCheckBox.Location = new System.Drawing.Point(12, 81);
            this.hostCheckBox.Name = "hostCheckBox";
            this.hostCheckBox.Size = new System.Drawing.Size(79, 17);
            this.hostCheckBox.TabIndex = 9;
            this.hostCheckBox.Text = "Host Game";
            this.hostCheckBox.UseVisualStyleBackColor = true;
            this.hostCheckBox.CheckedChanged += new System.EventHandler(this.hostCheckBox_CheckedChanged);
            // 
            // GameConfigurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(208, 156);
            this.Controls.Add(this.hostCheckBox);
            this.Controls.Add(this.launchButton);
            this.Controls.Add(this.fullscreenCheckBox);
            this.Controls.Add(this.debugCheckBox);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.portLabel);
            this.Controls.Add(this.serverLabel);
            this.Controls.Add(this.hostnameTextBox);
            this.Controls.Add(this.titleLabel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameConfigurator";
            this.Text = "GunsBullets";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.TextBox hostnameTextBox;
        private System.Windows.Forms.Label serverLabel;
        private System.Windows.Forms.Label portLabel;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.CheckBox debugCheckBox;
        private System.Windows.Forms.CheckBox fullscreenCheckBox;
        private System.Windows.Forms.Button launchButton;
        private System.Windows.Forms.CheckBox hostCheckBox;
    }
}