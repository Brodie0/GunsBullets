using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GunsBullets {
    class Bullet {
        private readonly Texture2D _bulletTexture;
        private readonly Vector2 _origin;
        private Vector2 _spritePosition;
        private Vector2 _spritePositionPrev;
        private Vector2 _spriteSpeed;
        //private bool _destroyMe;
        private readonly SoundEffect[] _ricochetSounds;
        private readonly float _radius;
        private readonly Random _randGenerator;
        //public bool DestroyMe { get { return _destroyMe; } set { _destroyMe = value; } }
        public bool DestroyMe { get; set; }
        public Vector2 SpritePosition => _spritePosition;
        public float Radius => _radius;

        public Bullet(ContentManager content, Vector2 playerPosition, float playerRotation, MouseState mouseState, Vector2 playerOrigin) {
            //pocisk musi sie pojawic nie na krancu tekstury gracza ale mniej wiecej na srodku (na koncu karabinu)
            playerRotation -= Convert.ToSingle(Math.PI / 2.5);
            _bulletTexture = content.Load<Texture2D>(Config.BulletTexture);
            _origin = new Vector2(_bulletTexture.Width / 2.0f, _bulletTexture.Height / 2.0f);
            _spritePosition = new Vector2(Convert.ToSingle(Config.BulletAppearDistanceFromPlayer * Math.Cos(playerRotation) + playerPosition.X + playerOrigin.X),
                Convert.ToSingle(Config.BulletAppearDistanceFromPlayer * Math.Sin(playerRotation) + playerPosition.Y + playerOrigin.Y));
            _spritePositionPrev = _spritePosition;
            float distance = Convert.ToSingle(Math.Sqrt(Math.Pow(mouseState.X - _spritePosition.X, 2.0) + Math.Pow(mouseState.Y - _spritePosition.Y, 2.0)));
            _spriteSpeed = new Vector2((mouseState.X - _spritePosition.X) * Config.BulletSpeed / distance, (mouseState.Y - _spritePosition.Y) * Config.BulletSpeed / distance);
            DestroyMe = false;
            var sound = content.Load<SoundEffect>(Config.BulletSoundEffect);
            sound.Play();

            _ricochetSounds = new SoundEffect[Config.RicochetesSoundsAmount];
            _ricochetSounds[0] = content.Load<SoundEffect>(Config.Ricochet1);
            _ricochetSounds[1] = content.Load<SoundEffect>(Config.Ricochet2);

            _radius = (_bulletTexture.Width / 2.0f + _bulletTexture.Height / 2.0f) / 2.0f;

            _randGenerator = new Random();
        }


        public void UpdateBullet(ref GraphicsDeviceManager graphics, List<Vector2> wallPositions, Texture2D wallTexture) {

            _spritePosition += _spriteSpeed;
            var maxX = graphics.GraphicsDevice.Viewport.Width - _bulletTexture.Width;
            const int minX = 0;
            var maxY = graphics.GraphicsDevice.Viewport.Height - _bulletTexture.Height;
            const int minY = 0;

            //implementacja rykoszetu od murow
            foreach (var wallPosition in wallPositions) {

                if (!(wallPosition.X > _spritePosition.X + _bulletTexture.Width || _spritePosition.X > wallPosition.X + wallTexture.Width ||
                    wallPosition.Y > _spritePosition.Y + _bulletTexture.Height || _spritePosition.Y > wallPosition.Y + wallTexture.Height)) {
                    if (((_spritePosition.X > wallPosition.X && _spritePosition.X < wallPosition.X + wallTexture.Width) &&
                        (_spritePosition.Y > wallPosition.Y && _spritePosition.Y < wallPosition.Y + wallTexture.Height)) ||
                        ((_spritePosition.X + _bulletTexture.Width > wallPosition.X && _spritePosition.X + _bulletTexture.Width < wallPosition.X + wallTexture.Width) &&
                        (_spritePosition.Y + _bulletTexture.Height > wallPosition.Y && _spritePosition.Y + _bulletTexture.Height < wallPosition.Y + wallTexture.Height))) {
                            if(_spritePositionPrev.X <= wallPosition.X || _spritePositionPrev.X >= wallPosition.X + wallTexture.Width)
                                RicochetOrDestruction(true, (int)_spritePositionPrev.X);
                        if (_spritePositionPrev.Y <= wallPosition.Y || _spritePositionPrev.Y >= wallPosition.Y + wallTexture.Height)
                            RicochetOrDestruction(false, (int)_spritePositionPrev.Y);
                    }                 
                }
            }

            //implementacja rykoszetu od scian mapy
            if (_spritePosition.Y < minY)
                RicochetOrDestruction(false, minY);
            else if (_spritePosition.Y > maxY)
                RicochetOrDestruction(false, maxY);
            if (_spritePosition.X < minX)
                RicochetOrDestruction(true, minX);
            else if (_spritePosition.X > maxX)
                RicochetOrDestruction(true, maxX);

            _spritePositionPrev = _spritePosition;
        }

        public void DrawBullet(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(_bulletTexture, _spritePosition - _origin, Color.White);
        }


        private void RicochetOrDestruction(bool verticalWall, int border = -1) {
            var rand = _randGenerator.Next(Config.RicochetProbability);
            if (rand == 0) {

                _spriteSpeed.X *= verticalWall ? -1 : 1;
                _spriteSpeed.Y *= verticalWall ? 1 : -1;
                if (border != -1) {
                    if (!verticalWall)
                        _spritePosition.Y = border;
                    else _spritePosition.X = border;

                }
                _ricochetSounds[_randGenerator.Next(Config.RicochetesSoundsAmount)].Play();
            } else {
                DestroyMe = true;
            }
        }
    }
}
