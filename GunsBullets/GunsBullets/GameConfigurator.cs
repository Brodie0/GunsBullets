using System;
using System.Windows.Forms;

using Microsoft.Xna.Framework.Input;

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

            if (string.IsNullOrWhiteSpace(nicknameTextBox.Text) || string.IsNullOrWhiteSpace(nicknameTextBox.Text)) {
                MessageBox.Show("Please enter your nickname.");
                return;
            } else Config.Nickname = nicknameTextBox.Text;

            short port;
            if (!Int16.TryParse(portTextBox.Text, out port) || (port < 1024 || port > 10000)) {
                MessageBox.Show("The server's port must be a valid number between 1024 and 10000.");
                return;
            } else Config.Port = port;

            Launch = true;

            Config.HostGame = hostCheckBox.Checked;
            Config.DebugMode = debugCheckBox.Checked;
            Config.FullScreen = fullscreenCheckBox.Checked;
            Config.GamePadEnabled = gamepadCheckBox.Checked;

            if (Config.GamePadEnabled) {
                try {
                    GamePadState state = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
                    if (!state.IsConnected) throw new Exception("Gamepad for Player 1 isn't connected!");
                } catch {
                    MessageBox.Show("Looks like your gamepad isn't connected. (Are you sure it's the first gamepad?");
                    return;
                }
            }

            Close();
        }

        private void hostCheckBox_CheckedChanged(object sender, EventArgs e) {
            hostnameTextBox.Enabled = !hostCheckBox.Checked;
            portTextBox.Enabled = !hostCheckBox.Checked;
        }
    }
}
