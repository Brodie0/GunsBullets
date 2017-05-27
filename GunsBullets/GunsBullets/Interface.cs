using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GunsBullets {
    class Interface {
        bool _hosting;
        bool _toggleFullScreen;
        bool _guesting;
        bool _stopHosting;
        bool _initializeHost;
        bool _stopGuesting;
        bool _initializeGuest;
        KeyboardState _oldKeyboardState;

        public bool? Hosting { get => _hosting; }
        public bool ToggleFullScreen { get => _toggleFullScreen; }
        public bool Guesting { get => _guesting; }
        public bool StopHosting { get => _stopHosting; set => _stopHosting = value; }
        public bool InitializeHost { get => _initializeHost; set => _initializeHost = value; }
        public bool StopGuesting { get => _stopGuesting; set => _stopGuesting = value; }
        public bool InitializeGuest { get => _initializeGuest; set => _initializeGuest = value; }

        public Interface() {
            _hosting = false;
            _toggleFullScreen = false;
            _guesting = false;
        }

        public bool Update() {
            bool ret = true;
            KeyboardState newKeyboardState = Keyboard.GetState();
            //interface features
            if (newKeyboardState.IsKeyDown(Keys.F))
                _toggleFullScreen = !_toggleFullScreen;

            //multiplayer options
            else if (newKeyboardState.IsKeyDown(Keys.H) && _oldKeyboardState.IsKeyUp(Keys.H) && !_guesting) {
                if (_hosting)
                    _stopHosting = true;
                else
                    _initializeHost = true;
                _hosting = !_hosting;

            }
            else if (newKeyboardState.IsKeyDown(Keys.G) && _oldKeyboardState.IsKeyUp(Keys.G) && !_hosting) {
                if (_guesting)
                    _stopGuesting = true;
                else
                    _initializeGuest = true;
                _guesting = !_guesting;

            }
            else
                ret = false;
            _oldKeyboardState = newKeyboardState;

            return ret;
        }
    }
}
