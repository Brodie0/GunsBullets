using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace GunsBullets {
    [Serializable]
    class Player {
        private readonly float bodyRadius;
        private int ammo;
        private int deaths;

        public int PlayerID;
        public Texture2D PlayerTexture { get { return TextureAtlas.Player[PlayerID]; } }
        public List<Bullet> MyBullets;

        public readonly Vector2 Origin;
        public float Rotation;
        public Vector2 Position;

        [NonSerialized] private bool _continuousFire;
        public bool ContinuousFire => _continuousFire;

        [NonSerialized] private bool _singleShot;
        public bool SingleShot => _singleShot;
        
        public void DecreaseAmmo() { ammo--; }

        public Player(ContentManager content) {
            PlayerID = 0;
            MyBullets = new List<Bullet>();
            Origin = new Vector2(PlayerTexture.Width / 2.0f, PlayerTexture.Height / 2.0f);
            bodyRadius = (PlayerTexture.Width / 2.0f + PlayerTexture.Height / 2.0f) / 2.0f;
            Position = Vector2.Zero;
            _continuousFire = false;
            _singleShot = false;
            ammo = Config.MaxAmmoAmount;
            deaths = 0;
        }

        public void UpdatePlayer(ref GraphicsDeviceManager graphics, GameInput input, ref Map map, ref List<Bullet> bullets, IEnumerable<Vector2> wallPositions) {
            var PositionDelta = Vector2.Zero;
            PositionDelta.X = input.MovementDirection.X * Config.PlayerMaxSpeed;
            PositionDelta.Y = input.MovementDirection.Y * Config.PlayerMaxSpeed;
            
            var maxX = TextureAtlas.Map.Width - PlayerTexture.Width;
            var maxY = TextureAtlas.Map.Height - PlayerTexture.Height;

            // Collision: Map Borders
            if (Position.X + PositionDelta.X > maxX) PositionDelta.X = 0;
            else if (Position.X + PositionDelta.X < 0) PositionDelta.X = 0;

            if (Position.Y + PositionDelta.Y > maxY) PositionDelta.Y = 0;
            else if (Position.Y + PositionDelta.Y < 0) PositionDelta.Y = 0;

            // Collision: Walls
            foreach (var wallPosition in wallPositions) {
                var p = new BoundingSphere(new Vector3(Position + Origin, 0), PlayerTexture.Height / 2);
                var r = new Rectangle((int)wallPosition.X, (int)wallPosition.Y, TextureAtlas.Wall.Width, TextureAtlas.Wall.Height);

                if (Collisions.Intersects(p, r)) {
                    double wy = (p.Radius + r.Height / 2) * (p.Center.Y - r.Center.Y);
                    double hx = (p.Radius + r.Width / 2) * (p.Center.X - r.Center.X);
                    if (wy > hx) {
                        if (wy > -hx && PositionDelta.Y < 0) PositionDelta.Y = 0;
                        else if (wy <= -hx && PositionDelta.X > 0) PositionDelta.X = 0;
                    } else {
                        if (wy > -hx && PositionDelta.X < 0) PositionDelta.X = 0;
                        else if (wy <= -hx && PositionDelta.Y > 0)  PositionDelta.Y = 0;
                    }
                }
            }

            // Final position delta calculated!
            Position += PositionDelta;
            
            // Aiming and shooting:
            var newX = input.AimingDirection.X + Position.X;
            var newY = input.AimingDirection.Y + Position.Y;

            // Rotation
            var rotationTemp = Convert.ToSingle(Math.Asin(Math.Abs(Position.X - newX) /
                (Math.Sqrt(Math.Pow(Position.X - newX, 2.0) + Math.Pow(Position.Y - newY, 2.0)))));

            if (newX > Position.X && newY < Position.Y)
                Rotation = rotationTemp;
            else if (newX > Position.X && newY > Position.Y)
                Rotation = Convert.ToSingle(Math.PI) - rotationTemp;
            else if (newX < Position.X && newY > Position.Y)
                Rotation = Convert.ToSingle(2 * Math.PI) - (Convert.ToSingle(Math.PI) - rotationTemp);
            else if (newX < Position.X && newY < Position.Y)
                Rotation = Convert.ToSingle(2 * Math.PI) - rotationTemp;

            // Firing
            if (input.Shoot && ammo > 0) {
                if (!input.ShootPreviously) _singleShot = true;
                else _continuousFire = true;
            } else {
                _singleShot = false;
                _continuousFire = false;
            }

            UpdateCollision(bullets);
        }


        public void DrawPlayer(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(PlayerTexture, Position + Origin, null, Color.White, Rotation, Origin, 1.0f, SpriteEffects.None, 0.0f);
        }
        
        private void UpdateCollision(List<Bullet> bullets) {
            //distance between player's and bullet's centres
            foreach (var bullet in bullets) {
                var distance = Convert.ToSingle(
                    Math.Sqrt(Math.Pow(bullet.SpritePosition.X - Position.X - Origin.X, 2) +
                              Math.Pow(bullet.SpritePosition.Y - Position.Y - Origin.Y, 2)));
                if (!(distance > bullet.Radius + bodyRadius)) {
                    OnHitReact();
                    bullet.DestroyMe = true;
                }
            }
        }

        public void AmmoReload(ContentManager content) {
            ammo = Config.MaxAmmoAmount;
            AudioAtlas.Reload.Play();
        }

        public bool UpdateReloadPosition(IEnumerable<Vector2> ammoPositions) {
            foreach (var ammoPosition in ammoPositions) {
                if (Position.X + Origin.X >= ammoPosition.X && Position.X + Origin.X <= ammoPosition.X + TextureAtlas.Ammo.Width &&
                    Position.Y + Origin.Y >= ammoPosition.Y && Position.Y + Origin.Y <= ammoPosition.Y + TextureAtlas.Ammo.Height)
                    return true;
            }
            return false;
        }

        private void OnHitReact() {
            OSD.LogEvent(Config.Nickname + " died!", 5.0);
            AudioAtlas.DeathScream.Play();
            deaths++;
            Position = Vector2.Zero;
            ammo = Config.MaxAmmoAmount;
        }

        public override string ToString() {
            return $"[ID {PlayerID}] Pos: {Position}, Rotation: {Rotation}, Origin: {Origin} - Deaths: {deaths}, Ammo: {ammo}, Bullets: {MyBullets.Count}";
        }
    }
}
