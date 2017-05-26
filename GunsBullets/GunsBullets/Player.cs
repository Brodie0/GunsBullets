using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GunsBullets {
    [Serializable]
    class Player {
        [NonSerialized] private readonly Texture2D _playerTexture;
        private readonly Vector2 _origin;
        [NonSerialized] private KeyboardState _oldKeyboardState;
        [NonSerialized] private MouseState _oldMouseState;
        private float _rotation;
        private Vector2 _spritePosition;
        private Vector2 _spriteSpeed;
        [NonSerialized] private bool _continuousFire;
        [NonSerialized] private bool _singleShot;
        private readonly float _radiusOfBody;
        [NonSerialized] private readonly SoundEffect _deathScream;
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
            _deathScream = content.Load<SoundEffect>(Config.Sound_DeathScream);
            _spritePosition = Vector2.Zero;
            _spriteSpeed = Vector2.Zero;
            _continuousFire = false;
            _singleShot = false;
            _destroyMe = false;
            _ammoAmount = Config.MaxAmmoAmount;
            _deathsAmount = 0;
        }

        public void UpdatePlayer(ref GraphicsDeviceManager graphics, ref Map map, ref List<Bullet> bullets, IEnumerable<Vector2> wallPositions, Texture2D wallTexture) {
            UpdateKeyboard(ref map, wallPositions, wallTexture);
            UpdateMouse(ref graphics);
            UpdateCollision(bullets);
        }


        public void DrawPlayer(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(_playerTexture, _spritePosition + _origin, null, Color.White, _rotation, _origin, 1.0f, SpriteEffects.None, 0.0f);
        }


        private void UpdateKeyboard(ref Map map, IEnumerable<Vector2> wallPositions, Texture2D wallTexture) {
            KeyboardState newKeyboardState = Keyboard.GetState();

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

            var maxX = map._mapTexture.Width - _playerTexture.Width;
            const int minX = 0;
            var maxY = map._mapTexture.Height - _playerTexture.Height;
            const int minY = 0;

            // borders collision
            if (_spritePosition.X + _spriteSpeed.X > maxX)
                _spriteSpeed.X = 0;
            else if (_spritePosition.X + _spriteSpeed.X < minX)
                _spriteSpeed.X = 0;

            if (_spritePosition.Y + _spriteSpeed.Y > maxY)
                _spriteSpeed.Y = 0;
            else if (_spritePosition.Y + _spriteSpeed.Y < minY)
                _spriteSpeed.Y = 0;

            //wall collision
            foreach (var wallPosition in wallPositions) {
                var p = new BoundingSphere(new Vector3(_spritePosition + _origin, 0), _playerTexture.Height / 2);
                var r = new Rectangle((int)wallPosition.X, (int)wallPosition.Y, wallTexture.Width, wallTexture.Height);

                if (Collisions.Intersects(p, r)) {
                    double wy = (p.Radius + r.Height / 2) * (p.Center.Y - r.Center.Y);
                    double hx = (p.Radius + r.Width / 2) * (p.Center.X - r.Center.X);
                    if (wy > hx) {
                        if (wy > -hx && _spriteSpeed.Y < 0)
                            _spriteSpeed.Y = 0;
                        else if( wy <=- hx && _spriteSpeed.X > 0)
                            _spriteSpeed.X = 0;
                    }
                    else {
                        if (wy > -hx && _spriteSpeed.X < 0)
                            _spriteSpeed.X = 0;
                        else if(wy <= -hx && _spriteSpeed.Y > 0)  {
                            _spriteSpeed.Y = 0;
                        }
                    }
                }
            }
            _spritePosition += _spriteSpeed;
            _oldKeyboardState = newKeyboardState;
        }

        private void UpdateMouse(ref GraphicsDeviceManager graphics) {
            var newMouseState = Mouse.GetState();
            var newX = newMouseState.X + _spritePosition.X - graphics.GraphicsDevice.Viewport.Width / 2;
            var newY = newMouseState.Y + _spritePosition.Y - graphics.GraphicsDevice.Viewport.Height / 2;

            //rotation
            var rotationTemp = Convert.ToSingle(Math.Asin(Math.Abs(_spritePosition.X - newX) /
                (Math.Sqrt(Math.Pow(_spritePosition.X - newX, 2.0) + Math.Pow(_spritePosition.Y - newY, 2.0)))));
            if (newX > _spritePosition.X && newY < _spritePosition.Y)
                _rotation = rotationTemp;
            else if (newX > _spritePosition.X && newY > _spritePosition.Y)
                _rotation = Convert.ToSingle(Math.PI) - rotationTemp;
            else if (newX < _spritePosition.X && newY > _spritePosition.Y)
                _rotation = Convert.ToSingle(2 * Math.PI) - (Convert.ToSingle(Math.PI) - rotationTemp);
            else if (newX < _spritePosition.X && newY < _spritePosition.Y)
                _rotation = Convert.ToSingle(2 * Math.PI) - rotationTemp;

            //fire
            if (newMouseState.LeftButton == ButtonState.Pressed && IsMouseInsideWindow(graphics) && _ammoAmount > 0) {
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

        bool IsMouseInsideWindow(GraphicsDeviceManager graphics) {
            MouseState ms = Mouse.GetState();
            Point pos = new Point(ms.X, ms.Y);
            return graphics.GraphicsDevice.Viewport.Bounds.Contains(pos);
        }

        private void UpdateCollision(IEnumerable<Bullet> bullets) {
            // distance between player's and bullet's centres
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

        public void AmmoReload(ContentManager content) {
            _ammoAmount = Config.MaxAmmoAmount;
            var sound = content.Load<SoundEffect>(Config.Sound_Reload);
            sound.Play();
        }

        public bool UpdateReloadPosition(IEnumerable<Vector2> ammoPositions, Texture2D ammoTexture) {
            foreach(var ammoPosition in ammoPositions) {
                if (_spritePosition.X + _origin.X >= ammoPosition.X && _spritePosition.X + _origin.X <= ammoPosition.X + ammoTexture.Width &&
                    _spritePosition.Y + _origin.Y >= ammoPosition.Y && _spritePosition.Y + _origin.Y <= ammoPosition.Y + ammoTexture.Height)
                    return true;
            }
            return false;
        }

        private void OnHitReact() {
            _deathScream.Play();
            _deathsAmount++;
            Thread.Sleep(200);
            _spritePosition = Vector2.Zero;
            _ammoAmount = Config.MaxAmmoAmount;
            //_destroyMe = true;
        }

        public override string ToString() {
            string s1 = _origin.ToString();
            string s2 = _spritePosition.ToString();
            string s3 = _spriteSpeed.ToString();

            return "\nOrigin: " + s1 + "\nSpritePosition: " + s2 + "\nSpriteSpeed: " + s3 + 
                "\nRotation: " + _rotation + "\nAmmoAmount: " + _ammoAmount + "\nDeathsAmount: " + _deathsAmount + "\nDestroyMe: " + _destroyMe+ "\n";
        }
    }
}
