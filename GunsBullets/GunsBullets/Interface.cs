using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GunsBullets {
    class Interface {
        private KeyboardState OldKeyboardState;

        public bool Hosting { get; private set; }
        public bool ToggleFullScreen { get; private set; }
        public bool Guesting { get; private set; }
        public bool StopHosting { get; set; }
        public bool InitializeHost { get; set; }
        public bool StopGuesting { get; set; }
        public bool InitializeGuest { get; set; }

        public Interface() {
            Hosting = false;
            ToggleFullScreen = false;
            Guesting = false;
        }

        public bool Update() {
            bool ret = true;
            KeyboardState newKeyboardState = Keyboard.GetState();
            //interface features
            if (newKeyboardState.IsKeyDown(Keys.F) && OldKeyboardState.IsKeyUp(Keys.F)) {
                ToggleFullScreen = !ToggleFullScreen;
            } else if (newKeyboardState.IsKeyDown(Keys.H) && OldKeyboardState.IsKeyUp(Keys.H) && !Guesting) {
                //multiplayer options
                if (Hosting) StopHosting = true;
                else InitializeHost = true;
                Hosting = !Hosting;
            } else if (newKeyboardState.IsKeyDown(Keys.G) && OldKeyboardState.IsKeyUp(Keys.G) && !Hosting) {
                if (Guesting) StopGuesting = true;
                else InitializeGuest = true;
                Guesting = !Guesting;
            } else ret = false;

            OldKeyboardState = newKeyboardState;
            return ret;
        }

        public static string GetLocalIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }

            throw new Exception("Local IP Address Not Found!");
        }
    }
}
