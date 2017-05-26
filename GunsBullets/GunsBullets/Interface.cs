using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GunsBullets {
    class Interface {
        bool _imAHost;
        bool _toggleFullScreen;
        bool _imAGuest;
        KeyboardState _oldKeyboardState;

        public bool ImAHost { get => _imAHost; set => _imAHost = value; }
        public bool ToggleFullScreen { get => _toggleFullScreen; set => _toggleFullScreen = value; }
        public bool ImAGuest { get => _imAGuest; set => _imAGuest = value; }

        public Interface() {
            ImAHost = false;
            ToggleFullScreen = false;
            ImAGuest = false;
        }

        public void Update() {
            KeyboardState newKeyboardState = Keyboard.GetState();
            //interface features
            if (newKeyboardState.IsKeyDown(Keys.F))
                ToggleFullScreen = !ToggleFullScreen;
            //multiplayer options
            if (newKeyboardState.IsKeyDown(Keys.H) && !ImAGuest) {
                ImAHost = !ImAHost;
            }
            if (newKeyboardState.IsKeyDown(Keys.G) && !ImAHost) {
                ImAGuest = !ImAGuest;
            }

            _oldKeyboardState = newKeyboardState;
        }
    }
}
