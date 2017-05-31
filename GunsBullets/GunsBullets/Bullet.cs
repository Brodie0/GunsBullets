using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GunsBullets {
    [Serializable]
    class Bullet {
        public int OwnerID { get; set; }

        private readonly Vector2 _origin;
        private Vector2 _spritePosition;
        private Vector2 _spritePositionPrev;
        private Vector2 _spriteSpeed;
        public float Radius { get; private set; }
        private Random rng;

        public bool DestroyMe { get; set; }
        public Vector2 SpritePosition => _spritePosition;
        
        public Bullet(ref GraphicsDeviceManager graphics, Vector2 playerPosition, float playerRotation, GameInput input, Vector2 playerOrigin) {
            if (Config.DebugMode) {
                Console.WriteLine("New bullet shot at: " + input.AimingDirection + ", from: " + playerPosition.ToString());
            }

            float newX = input.AimingDirection.X + playerPosition.X;
            float newY = input.AimingDirection.Y + playerPosition.Y;
            playerRotation -= Convert.ToSingle(Math.PI / 2.5);
            _origin = new Vector2(TextureAtlas.Bullet.Width / 2.0f, TextureAtlas.Bullet.Height / 2.0f);
            _spritePosition = new Vector2(Convert.ToSingle(Config.BulletAppearDistanceFromPlayer * Math.Cos(playerRotation) + playerPosition.X + playerOrigin.X),
                Convert.ToSingle(Config.BulletAppearDistanceFromPlayer * Math.Sin(playerRotation) + playerPosition.Y + playerOrigin.Y));
            _spritePositionPrev = _spritePosition;
            float distance = Convert.ToSingle(Math.Sqrt(Math.Pow(newX - _spritePosition.X, 2.0) + Math.Pow(newY - _spritePosition.Y, 2.0)));
            _spriteSpeed = new Vector2((newX - _spritePosition.X) * Config.BulletSpeed / distance, (newY - _spritePosition.Y) * Config.BulletSpeed / distance);
            DestroyMe = false;
            AudioAtlas.Shot.Play();

            Radius = (TextureAtlas.Bullet.Width / 2.0f + TextureAtlas.Bullet.Height / 2.0f) / 2.0f;

            rng = new Random();
        }

        public void Update(ref GraphicsDeviceManager graphics, ref Map map, List<Vector2> wallPositions) {
            _spritePosition += _spriteSpeed;
            var maxX = TextureAtlas.Map.Width - TextureAtlas.Bullet.Width;
            const int minX = 0;
            var maxY = TextureAtlas.Map.Height - TextureAtlas.Bullet.Height;
            const int minY = 0;

            //ricochete off walls
            foreach (var wallPosition in wallPositions) {

                var b = new BoundingSphere(new Vector3(_spritePosition + _origin, 0), TextureAtlas.Bullet.Height / 2);
                var r = new Rectangle((int)wallPosition.X, (int)wallPosition.Y, TextureAtlas.Wall.Width, TextureAtlas.Wall.Height);

                if (Collisions.Intersects(b, r)) {
                    double wy = (b.Radius + r.Height / 2) * (b.Center.Y - r.Center.Y);
                    double hx = (b.Radius + r.Width / 2) * (b.Center.X - r.Center.X);
                    if (wy > hx) {
                        if (wy > -hx && _spriteSpeed.Y < 0)
                            RicochetOrDestruction(false, (int)wallPosition.Y + TextureAtlas.Wall.Height + (int)TextureAtlas.Bullet.Height/2);
                        else if (wy <= -hx && _spriteSpeed.X > 0)
                            RicochetOrDestruction(true, (int)wallPosition.X - (int)TextureAtlas.Bullet.Width/2);
                    }
                    else {
                        if (wy > -hx && _spriteSpeed.X < 0)
                            RicochetOrDestruction(true, (int)wallPosition.X + TextureAtlas.Wall.Width + (int)TextureAtlas.Bullet.Width/2);
                        else if (wy <= -hx && _spriteSpeed.Y > 0) {
                            RicochetOrDestruction(false, (int)wallPosition.Y - (int)TextureAtlas.Bullet.Height/2);
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
            spriteBatch.Draw(TextureAtlas.Bullet, _spritePosition - _origin, Color.White);
        }

        private void RicochetOrDestruction(bool verticalWall, int border) {
            if (rng.Next(Config.RicochetProbability) == 0) {
                _spriteSpeed.X *= verticalWall ? -1 : 1;
                _spriteSpeed.Y *= verticalWall ? 1 : -1;
                AudioAtlas.Ricochet[rng.Next(AudioAtlas.Ricochet.Length)].Play();
            } else DestroyMe = true;

            if (!verticalWall) _spritePosition.Y = border;
            else _spritePosition.X = border;
        }

        public override string ToString() {
            string s1 = _origin.ToString();
            string s2 = _spritePosition.ToString();
            string s3 = _spriteSpeed.ToString();
            return "\nOrigin: " + s1 + "\nSpritePosition: " + s2 + "\nSpriteSpeed: " + s3 + "\nDestroyMe: " + DestroyMe + "\n";
        }
    }
}
