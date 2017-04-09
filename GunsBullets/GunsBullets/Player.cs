using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace GunsBullets {
    class Player {
        private readonly Texture2D _playerTexture;
        private readonly Vector2 _origin;
        private KeyboardState _oldKeyboardState;
        private MouseState _oldMouseState;
        private float _rotation;
        private Vector2 _spritePosition;
        private Vector2 _spriteSpeed;
        private bool _continuousFire;
        private bool _singleShot;
        private readonly float _radiusOfBody;
        private readonly SoundEffect _deathScream;
        private readonly bool _destroyMe;
        private int _ammoAmount;
        private int _deathsAmount;

        public bool ContinuousFire => _continuousFire;
        public bool SingleShot => _singleShot;
        public Vector2 SpritePosition => _spritePosition;
        public float Rotation => _rotation;
        public MouseState OldMouseState => _oldMouseState;
        public Vector2 Origin => _origin;
        public bool DestroyMe => _destroyMe;
        public void DecreaseAmmo() { _ammoAmount--; }

        public Player(ContentManager content) {
            _playerTexture = content.Load<Texture2D>(Config.PlayerTexture);
            _origin = new Vector2(_playerTexture.Width / 2.0f, _playerTexture.Height / 2.0f);
            _radiusOfBody = (_playerTexture.Width / 2.0f + _playerTexture.Height / 2.0f) / 2.0f;
            _deathScream = content.Load<SoundEffect>(Config.DeathScream);
            _spritePosition = Vector2.Zero;
            _spriteSpeed = Vector2.Zero;
            _continuousFire = false;
            _singleShot = false;
            _destroyMe = false;
            _ammoAmount = Config.AmmoAmount;
            _deathsAmount = 0;
        }

        public void UpdatePlayer(ref GraphicsDeviceManager graphics, ref List<Bullet> bullets, IEnumerable<Vector2> wallPositions, Texture2D wallTexture) {
            UpdateKeyboard(ref graphics, wallPositions, wallTexture);
            UpdateMouse();
            UpdateCollision(bullets);
        }


        public void DrawPlayer(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(_playerTexture, _spritePosition + _origin, null, Color.White, _rotation, _origin, 1.0f, SpriteEffects.None, 0.0f);
        }


        private void UpdateKeyboard(ref GraphicsDeviceManager graphics, IEnumerable<Vector2> wallPositions, Texture2D wallTexture) {
            KeyboardState newKeyboardState = Keyboard.GetState();
            Vector2 oldPosition = _spritePosition;
            //interface features
            if (newKeyboardState.IsKeyDown(Keys.F))
                graphics.ToggleFullScreen();

            //moves
            if (newKeyboardState.IsKeyDown(Keys.W))
                _spriteSpeed.Y = -Config.PlayerMaxSpeed;
            else if (_oldKeyboardState.IsKeyDown(Keys.W))
                _spriteSpeed.Y = 0.0f;
            if (newKeyboardState.IsKeyDown(Keys.S))
                _spriteSpeed.Y = Config.PlayerMaxSpeed;
            else if (_oldKeyboardState.IsKeyDown(Keys.S))
                _spriteSpeed.Y = 0.0f;
            if (newKeyboardState.IsKeyDown(Keys.A))
                _spriteSpeed.X = -Config.PlayerMaxSpeed;
            else if (_oldKeyboardState.IsKeyDown(Keys.A))
                _spriteSpeed.X = 0.0f;
            if (newKeyboardState.IsKeyDown(Keys.D))
                _spriteSpeed.X = Config.PlayerMaxSpeed;
            else if (_oldKeyboardState.IsKeyDown(Keys.D))
                _spriteSpeed.X = 0.0f;
            _spritePosition += _spriteSpeed;
            var maxX = graphics.GraphicsDevice.Viewport.Width - _playerTexture.Width;
            const int minX = 0;
            var maxY = graphics.GraphicsDevice.Viewport.Height - _playerTexture.Height;
            const int minY = 0;

            // Check for collision.
            if (_spritePosition.X > maxX)
                _spritePosition.X = maxX;
            else if (_spritePosition.X < minX)
                _spritePosition.X = minX;

            if (_spritePosition.Y > maxY)
                _spritePosition.Y = maxY;
            else if (_spritePosition.Y < minY)
                _spritePosition.Y = minY;

            //kolizja  z murem, jako kolizja dwóch prostokątów
            foreach (var wallPosition in wallPositions) {
                var p = new Rectangle((int)_spritePosition.X, (int)_spritePosition.Y, _playerTexture.Width, _playerTexture.Height);
                var r = new Rectangle((int)wallPosition.X, (int)wallPosition.Y, wallTexture.Width, wallTexture.Height);

                if (p.Intersects(r))
                    _spritePosition = oldPosition;
            }

            _oldKeyboardState = newKeyboardState;
        }


        private void UpdateMouse() {
            var newMouseState = Mouse.GetState();

            //rotation
            var rotationTemp = Convert.ToSingle(Math.Asin(Math.Abs(_spritePosition.X - newMouseState.X) /
                (Math.Sqrt(Math.Pow(_spritePosition.X - newMouseState.X, 2.0) + Math.Pow(_spritePosition.Y - newMouseState.Y, 2.0)))));
            if (newMouseState.X > _spritePosition.X && newMouseState.Y < _spritePosition.Y)
                _rotation = rotationTemp;
            else if (newMouseState.X > _spritePosition.X && newMouseState.Y > _spritePosition.Y)
                _rotation = Convert.ToSingle(Math.PI) - rotationTemp;
            else if (newMouseState.X < _spritePosition.X && newMouseState.Y > _spritePosition.Y)
                _rotation = Convert.ToSingle(2 * Math.PI) - (Convert.ToSingle(Math.PI) - rotationTemp);
            else if (newMouseState.X < _spritePosition.X && newMouseState.Y < _spritePosition.Y)
                _rotation = Convert.ToSingle(2 * Math.PI) - rotationTemp;

            //fire
            if (newMouseState.LeftButton == ButtonState.Pressed && _ammoAmount > 0) {
                if (_oldMouseState.LeftButton == ButtonState.Released)
                    _singleShot = true;
                else
                    _continuousFire = true;
            } else {
                _singleShot = false;
                _continuousFire = false;
            }

            _oldMouseState = newMouseState;
        }


        private void UpdateCollision(IEnumerable<Bullet> bullets) {
            // odległość pomiędzy środkami okręgów (gracz jako okrąg i pocisk tez)
            foreach (var bullet in bullets) {
                var distance = Convert.ToSingle(
                    Math.Sqrt(Math.Pow(bullet.SpritePosition.X - _spritePosition.X - _origin.X, 2) +
                              Math.Pow(bullet.SpritePosition.Y - _spritePosition.Y - _origin.Y, 2)));
                if (!(distance > bullet.Radius + _radiusOfBody)) {
                    OnHitReact();
                    bullet.DestroyMe = true;
                }
            }
        }


        private void OnHitReact() {
            _deathScream.Play();
            _deathsAmount++;
            //_destroyMe = true;
            Console.WriteLine("Deaths: " + _deathsAmount);
        }
    }
}
