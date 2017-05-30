using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GunsBullets {
    [Serializable]
    class Bullet {
        private readonly Vector2 _origin;
        private Vector2 _spritePosition;
        private Vector2 _spritePositionPrev;
        private Vector2 _spriteSpeed;
        private readonly float _radius;
        private readonly Random _randGenerator;

        public bool DestroyMe { get; set; }
        public Vector2 SpritePosition => _spritePosition;
        public float Radius => _radius;

        public int ShootersServerIdentificationNumber { get; set; }
        public Texture2D BulletTexture { get; set; }
        public SoundEffect[] RicochetSounds { get; set; }

        public Bullet(ref GraphicsDeviceManager graphics, ContentManager content, Vector2 playerPosition, float playerRotation, MouseState mouseState, Vector2 playerOrigin) {
            var newX = Mouse.GetState().X + playerPosition.X - graphics.GraphicsDevice.Viewport.Width / 2;
            var newY = Mouse.GetState().Y + playerPosition.Y - graphics.GraphicsDevice.Viewport.Height / 2;
            playerRotation -= Convert.ToSingle(Math.PI / 2.5);
            BulletTexture = content.Load<Texture2D>(Config.BulletTexture);
            _origin = new Vector2(BulletTexture.Width / 2.0f, BulletTexture.Height / 2.0f);
            _spritePosition = new Vector2(Convert.ToSingle(Config.BulletAppearDistanceFromPlayer * Math.Cos(playerRotation) + playerPosition.X + playerOrigin.X),
                Convert.ToSingle(Config.BulletAppearDistanceFromPlayer * Math.Sin(playerRotation) + playerPosition.Y + playerOrigin.Y));
            _spritePositionPrev = _spritePosition;
            float distance = Convert.ToSingle(Math.Sqrt(Math.Pow(newX - _spritePosition.X, 2.0) + Math.Pow(newY - _spritePosition.Y, 2.0)));
            _spriteSpeed = new Vector2((newX - _spritePosition.X) * Config.BulletSpeed / distance, (newY - _spritePosition.Y) * Config.BulletSpeed / distance);
            DestroyMe = false;
            var sound = content.Load<SoundEffect>(Config.Sound_Shot);
            sound.Play();

            RicochetSounds = new SoundEffect[Config.RicochetesSoundsAmount];
            RicochetSounds[0] = content.Load<SoundEffect>(Config.Sound_Ricochet1);
            RicochetSounds[1] = content.Load<SoundEffect>(Config.Sound_Ricochet2);

            _radius = (BulletTexture.Width / 2.0f + BulletTexture.Height / 2.0f) / 2.0f;

            _randGenerator = new Random();
        }

        public void UpdateBullet(ref GraphicsDeviceManager graphics, ref Map map, List<Vector2> wallPositions, Texture2D wallTexture) {

            _spritePosition += _spriteSpeed;
            var maxX = map._mapTexture.Width - BulletTexture.Width;
            const int minX = 0;
            var maxY = map._mapTexture.Height - BulletTexture.Height;
            const int minY = 0;

            //ricochete off walls
            foreach (var wallPosition in wallPositions) {

                var b = new BoundingSphere(new Vector3(_spritePosition + _origin, 0), BulletTexture.Height / 2);
                var r = new Rectangle((int)wallPosition.X, (int)wallPosition.Y, wallTexture.Width, wallTexture.Height);

                if (Collisions.Intersects(b, r)) {
                    double wy = (b.Radius + r.Height / 2) * (b.Center.Y - r.Center.Y);
                    double hx = (b.Radius + r.Width / 2) * (b.Center.X - r.Center.X);
                    if (wy > hx) {
                        if (wy > -hx && _spriteSpeed.Y < 0)
                            RicochetOrDestruction(false, (int)wallPosition.Y + wallTexture.Height + (int)BulletTexture.Height/2);
                        else if (wy <= -hx && _spriteSpeed.X > 0)
                            RicochetOrDestruction(true, (int)wallPosition.X - (int)BulletTexture.Width/2);
                    }
                    else {
                        if (wy > -hx && _spriteSpeed.X < 0)
                            RicochetOrDestruction(true, (int)wallPosition.X + wallTexture.Width + (int)BulletTexture.Width/2);
                        else if (wy <= -hx && _spriteSpeed.Y > 0) {
                            RicochetOrDestruction(false, (int)wallPosition.Y - (int)BulletTexture.Height/2);
                        }
                    }
                }
            }

            //ricochete off borders
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
            spriteBatch.Draw(BulletTexture, _spritePosition - _origin, Color.White);
        }

        private void RicochetOrDestruction(bool verticalWall, int border) {
            var rand = _randGenerator.Next(Config.RicochetProbability);
            if (rand == 0) {

                _spriteSpeed.X *= verticalWall ? -1 : 1;
                _spriteSpeed.Y *= verticalWall ? 1 : -1;
                RicochetSounds[_randGenerator.Next(Config.RicochetesSoundsAmount)].Play();
            } else {
                DestroyMe = true;
            }
            if (!verticalWall)
                _spritePosition.Y = border;
            else
                _spritePosition.X = border;
        }

        public override string ToString() {
            string s1 = _origin.ToString();
            string s2 = _spritePosition.ToString();
            string s3 = _spriteSpeed.ToString();
            return "\nOrigin: " + s1 + "\nSpritePosition: " + s2 + "\nSpriteSpeed: " + s3 + "\nDestroyMe: " + DestroyMe + "\n";
        }
    }
}
