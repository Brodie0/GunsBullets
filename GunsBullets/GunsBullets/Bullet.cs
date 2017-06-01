using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GunsBullets {
    [Serializable]
    class Bullet {
        [NonSerialized] public Player Owner;
        [NonSerialized] private Random rng = new Random();
        
        public Vector2 Position;
        private Vector2 PreviousPosition;
        private Vector2 Speed;
        public bool DestroyMe = false;

        public readonly Vector2 Origin = new Vector2(TextureAtlas.Bullet.Width / 2.0f, TextureAtlas.Bullet.Height / 2.0f);
        public float Radius { get; private set; } = (TextureAtlas.Bullet.Width / 2.0f + TextureAtlas.Bullet.Height / 2.0f) / 2.0f;

        public Bullet(ref GraphicsDeviceManager graphics, Player shooter, GameInput input) {
            Owner = shooter;

            // TODO: Initial pos should be calculated by having a BulletSpawner in front of the player.
            // Instead of looking at specific (X, Y) to aim at, this would prevent self-shooting and
            // fix odd behaviours while using thumbsticks on a gamepad.
            
            Position = new Vector2(
                (float)(Config.BulletAppearDistanceFromPlayer * Math.Cos(Owner.Rotation - (Math.PI / 2.5)) + Owner.Position.X + Owner.Origin.X),
                (float)(Config.BulletAppearDistanceFromPlayer * Math.Sin(Owner.Rotation - (Math.PI / 2.5)) + Owner.Position.Y + Owner.Origin.Y));
            PreviousPosition = Position;

            Vector2 newXY = input.AimingDirection + Owner.Position;
            Speed = (newXY - Position) * (Config.BulletSpeed / Vector2.Distance(newXY, Position));
            AudioAtlas.Shot.Play();
        }

        public void Update(ref GraphicsDeviceManager graphics, ref Map map, List<Vector2> wallPositions) {
            Position += Speed;

            var maxX = TextureAtlas.Map.Width - TextureAtlas.Bullet.Width;
            var maxY = TextureAtlas.Map.Height - TextureAtlas.Bullet.Height;

            // Ricochet off walls
            foreach (var wallPosition in wallPositions) {
                var bulletBound = new BoundingSphere(new Vector3(Position + Origin, 0),
                    TextureAtlas.Bullet.Height / 2);

                var wallBound = new BoundingBox(new Vector3(wallPosition, 0),
                    new Vector3(wallPosition + TextureAtlas.Wall.GetDimensions(), 0));
                
                if (bulletBound.Intersects(wallBound)) {
                    if (wallBound.Contains(bulletBound) == ContainmentType.Contains) {
                        // If the bullet is already WITHIN the wall, just remove it. This
                        // drastically decreases ricochet chances due to the way we handle
                        // collisions, but prevents SHOOTING THROUGH WALLS, which is nice.

                        DestroyMe = true;
                        return; // Let's look no more at anything else below.
                    }

                    var wallCenter = Vector3.Lerp(wallBound.Min, wallBound.Max, 0.5f);
                    double wy = (bulletBound.Radius + TextureAtlas.Wall.Height / 2) * (bulletBound.Center.Y - wallCenter.Y);
                    double hx = (bulletBound.Radius + TextureAtlas.Wall.Width / 2) * (bulletBound.Center.X - wallCenter.X);

                    if (wy > hx) {
                        if (wy > -hx && Speed.Y < 0)
                            RicochetOrDestruction(false, (int)wallPosition.Y + TextureAtlas.Wall.Height + (int)TextureAtlas.Bullet.Height / 2);
                        else if (wy <= -hx && Speed.X > 0)
                            RicochetOrDestruction(true, (int)wallPosition.X - (int)TextureAtlas.Bullet.Width / 2);
                    } else {
                        if (wy > -hx && Speed.X < 0)
                            RicochetOrDestruction(true, (int)wallPosition.X + TextureAtlas.Wall.Width + (int)TextureAtlas.Bullet.Width / 2);
                        else if (wy <= -hx && Speed.Y > 0) {
                            RicochetOrDestruction(false, (int)wallPosition.Y - (int)TextureAtlas.Bullet.Height / 2);
                        }
                    }

                    break; // We've found the wall we've hit!
                }
            }

            // Ricochet off borders
            if (Position.Y < 0) RicochetOrDestruction(false, 0);
            else if (Position.Y > maxY) RicochetOrDestruction(false, maxY);

            if (Position.X < 0) RicochetOrDestruction(true, 0);
            else if (Position.X > maxX) RicochetOrDestruction(true, maxX);

            PreviousPosition = Position;
        }

        public void DrawBullet(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(TextureAtlas.Bullet, Position - Origin, Color.White);
        }

        private void RicochetOrDestruction(bool verticalWall, int border) {
            if (rng.Next(Config.RicochetProbability) == 0) {
                Speed.X *= verticalWall ? -1 : 1;
                Speed.Y *= verticalWall ? 1 : -1;
                AudioAtlas.Ricochet[rng.Next(AudioAtlas.Ricochet.Length)].Play();
            } else DestroyMe = true;

            if (!verticalWall) Position.Y = border;
            else Position.X = border;
        }

        public override string ToString() {
            return $"Pos: {Position}, Origin: {Origin}, Speed: {Speed} " + (DestroyMe ? "[DESTROYABLE]" : "[OK]");
        }
    }
}
