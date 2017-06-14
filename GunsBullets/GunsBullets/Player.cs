using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GunsBullets {
    [Serializable]
    class Player {
        public int PlayerID;
        public readonly string Nickname;
        public int Deaths;
        public int Ammo;
        
        public Texture2D PlayerTexture { get { return TextureAtlas.Player[PlayerID]; } }
        public readonly float Radius;
        
        public readonly Vector2 Origin;
        public float Rotation;
        public Vector2 Position;

        private Vector2 spawnPosition;
        public List<Bullet> MyBullets;

        [NonSerialized] private bool _continuousFire;
        public bool ContinuousFire => _continuousFire;

        [NonSerialized] private bool _singleShot;
        public bool SingleShot => _singleShot;

        public Player(ContentManager content, string nick, Map map) {
            PlayerID = 0;
            Nickname = nick;
            MyBullets = new List<Bullet>();
            Origin = new Vector2(PlayerTexture.Width / 2.0f, PlayerTexture.Height / 2.0f);
            Radius = (PlayerTexture.Width / 2.0f + PlayerTexture.Height / 2.0f) / 2.0f;
            _continuousFire = false;
            _singleShot = false;
            Ammo = Config.MaxAmmoAmount;
            Deaths = 0;

            var rng = new Random();
            var spawnID = rng.Next(map.SpawnPositions.Count);
            spawnPosition = map.SpawnPositions[spawnID];
            Position = Vector2.One + Origin + spawnPosition;
        }
        
        public void UpdatePlayer(ref GraphicsDeviceManager graphics, GameInput input, Map map, List<Bullet> bullets) {
            Vector2 PositionDelta = input.MovementDirection * Config.PlayerMaxSpeed;
            
            if (PositionDelta != Vector2.Zero) {
                // Collision: Walls
                var playerBound = new BoundingSphere(new Vector3(Position + PositionDelta, 0), Radius);
                foreach (var wall in map.Walls) {
                    if (playerBound.Contains(wall.Bound) != ContainmentType.Disjoint) {
                        bool ZeroX = false, ZeroY = false;
                        if (PositionDelta.X != 0) {
                            var PositionX = Position + (PositionDelta * Vector2.UnitX);
                            var BoundForX = new BoundingSphere(new Vector3(PositionX, 0), Radius);
                            if (BoundForX.Contains(wall.Bound) != ContainmentType.Disjoint) ZeroX = true;
                        }
                        
                        if (PositionDelta.Y != 0) {
                            var PositionY = Position + (PositionDelta * Vector2.UnitY);
                            var BoundForY = new BoundingSphere(new Vector3(PositionY, 0), Radius);
                            if (BoundForY.Contains(wall.Bound) != ContainmentType.Disjoint) ZeroY = true;
                        }

                        if (ZeroX) PositionDelta.X = 0;
                        if (ZeroY) PositionDelta.Y = 0;
                        
                        playerBound.Center = new Vector3(Position + PositionDelta, 0);
                        if (playerBound.Contains(wall.Bound) != ContainmentType.Disjoint) {
                            Console.WriteLine($"[CollisionFail] Player {PlayerID} at {Position} + {PositionDelta} hits wall: {wall.Bound}!");
                            //PositionDelta = Vector2.Zero;
                        }

                        wall.ClippedInto = true;
                    } else wall.ClippedInto = false;
                }

                // Final position delta calculated!
                Position += PositionDelta;
            }

            // Reloading
            if (input.Reload && CanReload(map.AmmoPositions) && Ammo != Config.MaxAmmoAmount) {
                Ammo = Config.MaxAmmoAmount;
                AudioAtlas.Reload.Play();
            }
            
            // Aiming and shooting:
            Rotation = (float)(2 * Math.PI) - (float) (Math.Atan2(input.AimingDirection.X, input.AimingDirection.Y) + Math.PI);
            
            if (input.Shoot && Ammo > 0 && input.AimingDirection != Vector2.Zero) {
                if (!input.ShootPreviously) _singleShot = true;
                else _continuousFire = true;
            } else {
                _singleShot = false;
                _continuousFire = false;
            }

            UpdateCollision(bullets);
        }


        public void DrawPlayer(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(PlayerTexture, Position, null, Color.White, Rotation, Origin, 1.0f, SpriteEffects.None, 0.0f);
        }
        
        private void UpdateCollision(List<Bullet> bullets) {
            //distance between player's and bullet's centres
            foreach (var bullet in bullets) {
                var distance = Convert.ToSingle(
                    Math.Sqrt(Math.Pow(bullet.Position.X - Position.X, 2) +
                              Math.Pow(bullet.Position.Y - Position.Y, 2)));
                if (!(distance > bullet.Radius + Radius)) {
                    OnHitReact();
                    bullet.DestroyMe = true;
                }
            }
        }
        
        public bool CanReload(IEnumerable<Vector2> ammoPositions) {
            foreach (var ammoPosition in ammoPositions) {
                if (Position.X >= ammoPosition.X && Position.X <= ammoPosition.X + TextureAtlas.Ammo.Width &&
                    Position.Y >= ammoPosition.Y && Position.Y <= ammoPosition.Y + TextureAtlas.Ammo.Height)
                    return true;
            }
            return false;
        }

        private void OnHitReact() {
            OSD.LogEvent(Nickname + " died!", 5.0);
            AudioAtlas.DeathScream.Play();
            Deaths++;
            Ammo = Config.MaxAmmoAmount;

            Position = Vector2.One + Origin + spawnPosition;
        }

        public override string ToString() {
            return $"[ID {PlayerID}] Pos: {Position}, Rotation: {Rotation} - Deaths: {Deaths}, Ammo: {Ammo}, Bullets: {MyBullets.Count}";
        }
    }
}
