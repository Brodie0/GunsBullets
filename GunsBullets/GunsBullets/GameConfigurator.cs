using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GunsBullets {
    public partial class GameConfigurator : Form {
        public bool Launch { get; private set; }

        public GameConfigurator() {
            InitializeComponent();
            Launch = false;
        }

        private void LaunchGame(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(hostnameTextBox.Text) || string.IsNullOrWhiteSpace(portTextBox.Text)) {
                MessageBox.Show("Please enter the server's IP/hostname and its port.");
                return;
            } else Config.IPHostname = hostnameTextBox.Text;

            short port;
            if (!Int16.TryParse(portTextBox.Text, out port) || (port < 1024 || port > 10000)) {
                MessageBox.Show("The server's port must be a valid number between 1024 and 10000.");
                return;
            } else Config.Port = port;

            Launch = true;
            Config.HostGame = hostCheckBox.Checked;
            Config.DebugMode = debugCheckBox.Checked;
            Config.FullScreen = fullscreenCheckBox.Checked;

            Close();
        }

        private void hostCheckBox_CheckedChanged(object sender, EventArgs e) {
            hostnameTextBox.Enabled = !hostCheckBox.Checked;
            portTextBox.Enabled = !hostCheckBox.Checked;
        }
    }
}
