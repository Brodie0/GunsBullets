using System;
using System.Net;
using System.Net.Sockets;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GunsBullets {
    class GameInput {
        private GraphicsDeviceManager gdm = null;

        public bool ToggleFullScreen { get; set; } = false;
        public bool Reload { get; set; } = false;

        public bool ShootPreviously { get; set; } = false;
        public bool Shoot { get; set; } = false;
        
        public Vector2 MovementDirection { get; set; } = Vector2.Zero;
        public Vector2 AimingDirection { get; set; } = Vector2.Zero;

        public GameInput(GraphicsDeviceManager gdm) {
            this.gdm = gdm;

            if (Config.GamePadEnabled) {
                PreviousGamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            }

            PreviousKeyboardState = Keyboard.GetState();
            MouseOffsetX = gdm.GraphicsDevice.Viewport.Width / 2;
            MouseOffsetY = gdm.GraphicsDevice.Viewport.Height / 2;
        }
        
        public void Update() {
            ShootPreviously = Shoot;

            if (Config.GamePadEnabled) UpdateGamePadInput();
            else UpdateKeyboardInput();
        }

        // --- PC Master Race Input Support ---

        private KeyboardState PreviousKeyboardState, CurrentKeyboardState;
        private MouseState CurrentMouseState;

        // Mouse reports (0, 0) in the upper left corner. We want (0, 0) at the center
        // of our screen - fix eeeeet here.
        int MouseOffsetX, MouseOffsetY;

        private bool IsKeyTapped(Keys key) {
            return CurrentKeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyUp(key);
        }

        private bool IsKeyPressed(Keys key) {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        private bool IsKeyPressed(Keys key1, Keys key2) {
            return CurrentKeyboardState.IsKeyDown(key1) || CurrentKeyboardState.IsKeyDown(key2);
        }

        private void UpdateKeyboardInput() {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();

            ToggleFullScreen = IsKeyTapped(Keys.F);
            Reload = IsKeyTapped(Keys.R);
            Shoot = CurrentMouseState.LeftButton == ButtonState.Pressed;

            Vector2 Movement = Vector2.Zero;
            Vector2 Aiming = Vector2.Zero;

            if (IsKeyPressed(Keys.A, Keys.Left) && IsKeyPressed(Keys.D, Keys.Right)) Movement.X = 0.0f;
            else if (IsKeyPressed(Keys.A, Keys.Left)) Movement.X = -1.0f;
            else if (IsKeyPressed(Keys.D, Keys.Right)) Movement.X = 1.0f;

            if (IsKeyPressed(Keys.W, Keys.Up) && IsKeyPressed(Keys.S, Keys.Down)) Movement.Y = 0.0f;
            else if (IsKeyPressed(Keys.W, Keys.Up)) Movement.Y = -1.0f;
            else if (IsKeyPressed(Keys.S, Keys.Down)) Movement.Y = 1.0f;
            
            if (Utilities.IsMouseInsideWindow(gdm, CurrentMouseState)) {
                Aiming.X = CurrentMouseState.X - MouseOffsetX;
                Aiming.Y = CurrentMouseState.Y - MouseOffsetY;
            }

            MovementDirection = Movement;
            AimingDirection = Aiming;
            PreviousKeyboardState = CurrentKeyboardState;
        }

        // --- Game Pad Input Support ---

        private GamePadState PreviousGamePadState, CurrentGamePadState;

        private bool IsGamePadButtonTapped(Buttons button) {
            return PreviousGamePadState.IsButtonDown(button) && CurrentGamePadState.IsButtonUp(button);
        }

        private bool IsGamePadButtonPressed(Buttons button) {
            return CurrentGamePadState.IsButtonDown(button);
        }

        private bool IsGamePadButtonPressed(Buttons button1, Buttons button2) {
            return CurrentGamePadState.IsButtonDown(button1) || CurrentGamePadState.IsButtonDown(button2);
        }

        private void UpdateGamePadInput() {
            CurrentGamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);

            ToggleFullScreen = IsGamePadButtonTapped(Buttons.Back);
            Reload = IsGamePadButtonTapped(Buttons.X);
            Shoot = IsGamePadButtonPressed(Buttons.RightShoulder, Buttons.LeftShoulder);

            Vector2 Movement = Vector2.Zero;
            Vector2 Aiming = Vector2.Zero;

            if (IsGamePadButtonPressed(Buttons.DPadLeft, Buttons.LeftThumbstickLeft)) {
                if (IsGamePadButtonPressed(Buttons.DPadLeft)) Movement.X = -1.0f;
                else Movement.X = CurrentGamePadState.ThumbSticks.Left.X;
            }

            if (IsGamePadButtonPressed(Buttons.DPadRight, Buttons.LeftThumbstickRight)) {
                if (IsGamePadButtonPressed(Buttons.DPadRight)) Movement.X = 1.0f;
                else Movement.X = CurrentGamePadState.ThumbSticks.Left.X;
            }

            if (IsGamePadButtonPressed(Buttons.DPadUp, Buttons.LeftThumbstickUp)) {
                if (IsGamePadButtonPressed(Buttons.DPadUp)) Movement.Y = -1.0f;
                else Movement.Y = -CurrentGamePadState.ThumbSticks.Left.Y;
            }

            if (IsGamePadButtonPressed(Buttons.DPadDown, Buttons.LeftThumbstickDown)) {
                if (IsGamePadButtonPressed(Buttons.DPadDown)) Movement.Y = 1.0f;
                else Movement.Y = -CurrentGamePadState.ThumbSticks.Left.Y;
            }
            
            Aiming.X = CurrentGamePadState.ThumbSticks.Right.X * 100.0f;
            Aiming.Y = -CurrentGamePadState.ThumbSticks.Right.Y * 100.0f;

            MovementDirection = Movement;
            AimingDirection = Aiming;
            PreviousGamePadState = CurrentGamePadState;
        }
    }

    class Utilities {
        public static string GetLocalIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }

            throw new Exception("Local IP Address Not Found!");
        }

        public static bool IsMouseInsideWindow(GraphicsDeviceManager graphics, MouseState ms) {
            Point pos = new Point(ms.X, ms.Y);
            return graphics.GraphicsDevice.Viewport.Bounds.Contains(pos);
        }

        /// <summary>Clamps a given value within a given range. For example,
        /// this may force the passed value to be within a <-1, 5> range for
        /// Clamp(value, -1.0, 5.0), and will return -1.0/5.0 if the passed
        /// value isn't in this range.</summary>
        public static double Clamp(double value, double clampFrom, double clampTo) {
            if (clampFrom > clampTo) {
                double temp = clampFrom;
                clampFrom = clampTo;
                clampTo = clampFrom;
            }

            if (value < clampFrom) return clampFrom;
            else if (value > clampTo) return clampTo;
            else return value;
        }
    }
}
