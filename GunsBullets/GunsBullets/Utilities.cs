using System;
using System.Net;
using System.Net.Sockets;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace GunsBullets {
    class GameInput {
        private GraphicsDeviceManager gdm = null;

        public bool ToggleFullScreen { get; set; } = false;
        public bool Reload { get; set; } = false;

        public bool ShootPreviously { get; set; } = false;
        public bool Shoot { get; set; } = false;
        
        public Vector2 MovementDirection = Vector2.Zero;
        public Vector2 AimingDirection = Vector2.Zero;

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
            MovementDirection = Vector2.Zero;

            if (Config.GamePadEnabled) UpdateGamePadInput();
            else UpdateKeyboardInput();

            // Normalization converts this vector to a **UNIT** vector which
            // points in the same direction. (Unit Vec => Vec2.Length() == 1.)
            if (AimingDirection != Vector2.Zero) AimingDirection.Normalize();
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
            
            if (IsKeyPressed(Keys.A, Keys.Left) && IsKeyPressed(Keys.D, Keys.Right)) MovementDirection.X = 0.0f;
            else if (IsKeyPressed(Keys.A, Keys.Left)) MovementDirection.X = -1.0f;
            else if (IsKeyPressed(Keys.D, Keys.Right)) MovementDirection.X = 1.0f;

            if (IsKeyPressed(Keys.W, Keys.Up) && IsKeyPressed(Keys.S, Keys.Down)) MovementDirection.Y = 0.0f;
            else if (IsKeyPressed(Keys.W, Keys.Up)) MovementDirection.Y = -1.0f;
            else if (IsKeyPressed(Keys.S, Keys.Down)) MovementDirection.Y = 1.0f;
            
            if (Utilities.IsMouseInsideWindow(gdm, CurrentMouseState)) {
                AimingDirection.X = CurrentMouseState.X - MouseOffsetX;
                AimingDirection.Y = CurrentMouseState.Y - MouseOffsetY;
            }
            
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
            
            if (IsGamePadButtonPressed(Buttons.DPadLeft, Buttons.LeftThumbstickLeft)) {
                if (IsGamePadButtonPressed(Buttons.DPadLeft)) MovementDirection.X = -1.0f;
                else MovementDirection.X = CurrentGamePadState.ThumbSticks.Left.X;
            }

            if (IsGamePadButtonPressed(Buttons.DPadRight, Buttons.LeftThumbstickRight)) {
                if (IsGamePadButtonPressed(Buttons.DPadRight)) MovementDirection.X = 1.0f;
                else MovementDirection.X = CurrentGamePadState.ThumbSticks.Left.X;
            }

            if (IsGamePadButtonPressed(Buttons.DPadUp, Buttons.LeftThumbstickUp)) {
                if (IsGamePadButtonPressed(Buttons.DPadUp)) MovementDirection.Y = -1.0f;
                else MovementDirection.Y = -CurrentGamePadState.ThumbSticks.Left.Y;
            }

            if (IsGamePadButtonPressed(Buttons.DPadDown, Buttons.LeftThumbstickDown)) {
                if (IsGamePadButtonPressed(Buttons.DPadDown)) MovementDirection.Y = 1.0f;
                else MovementDirection.Y = -CurrentGamePadState.ThumbSticks.Left.Y;
            }

            if (CurrentGamePadState.ThumbSticks.Right != Vector2.Zero) {
                AimingDirection = CurrentGamePadState.ThumbSticks.Right;
                AimingDirection.Y = -AimingDirection.Y;
            }

            PreviousGamePadState = CurrentGamePadState;
        }
    }

    public static class Utilities {
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

        public static bool IsWithin(this float number, Vector2 range) {
            return (number >= range.X && number <= range.Y);
        }

        public static bool IsWithin(this float number, float center, float range) {
            return (number >= center - range && number <= center + range);
        }

        public static Vector2 GetDimensions(this Texture2D texture) {
            return new Vector2(texture.Width, texture.Height);
        }
    }
}
