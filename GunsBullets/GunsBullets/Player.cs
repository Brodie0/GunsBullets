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
using System.Text;
using System.Linq;

//TODO przesłany player nie ma informacji o texturze
namespace GunsBullets {
    [Serializable]
    class Player {
        [NonSerialized] private KeyboardState _oldKeyboardState;
        [NonSerialized] private MouseState _oldMouseState;
        public MouseState OldMouseState => _oldMouseState;

        private readonly float bodyRadius;
        private int ammo;
        private int deaths;

        public int PlayerID;
        public Texture2D PlayerTexture { get { return TextureAtlas.Player[PlayerID]; } }
        public List<Bullet> MyBullets;

        public readonly Vector2 Origin;
        public float Rotation;
        public Vector2 SpritePosition;
        public Vector2 SpriteSpeed;

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
            SpritePosition = Vector2.Zero;
            SpriteSpeed = Vector2.Zero;
            _continuousFire = false;
            _singleShot = false;
            ammo = Config.MaxAmmoAmount;
            deaths = 0;
        }

        public void UpdatePlayer(ref GraphicsDeviceManager graphics, ref Map map, ref List<Bullet> bullets, IEnumerable<Vector2> wallPositions, Texture2D wallTexture) {
            UpdateKeyboard(ref map, wallPositions, wallTexture);
            UpdateMouse(ref graphics);
            UpdateCollision(bullets);
        }


        public void DrawPlayer(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(PlayerTexture, SpritePosition + Origin, null, Color.White, Rotation, Origin, 1.0f, SpriteEffects.None, 0.0f);
        }


        private void UpdateKeyboard(ref Map map, IEnumerable<Vector2> wallPositions, Texture2D wallTexture) {
            KeyboardState newKeyboardState = Keyboard.GetState();

            if (newKeyboardState.IsKeyDown(Keys.W))
                SpriteSpeed.Y = -Config.PlayerMaxSpeed;
            else if (_oldKeyboardState.IsKeyDown(Keys.W))
                SpriteSpeed.Y = 0.0f;
            if (newKeyboardState.IsKeyDown(Keys.S))
                SpriteSpeed.Y = Config.PlayerMaxSpeed;
            else if (_oldKeyboardState.IsKeyDown(Keys.S))
                SpriteSpeed.Y = 0.0f;
            if (newKeyboardState.IsKeyDown(Keys.A))
                SpriteSpeed.X = -Config.PlayerMaxSpeed;
            else if (_oldKeyboardState.IsKeyDown(Keys.A))
                SpriteSpeed.X = 0.0f;
            if (newKeyboardState.IsKeyDown(Keys.D))
                SpriteSpeed.X = Config.PlayerMaxSpeed;
            else if (_oldKeyboardState.IsKeyDown(Keys.D))
                SpriteSpeed.X = 0.0f;

            var maxX = TextureAtlas.Map.Width - PlayerTexture.Width;
            const int minX = 0;
            var maxY = TextureAtlas.Map.Height - PlayerTexture.Height;
            const int minY = 0;

            // borders collision
            if (SpritePosition.X + SpriteSpeed.X > maxX)
                SpriteSpeed.X = 0;
            else if (SpritePosition.X + SpriteSpeed.X < minX)
                SpriteSpeed.X = 0;

            if (SpritePosition.Y + SpriteSpeed.Y > maxY)
                SpriteSpeed.Y = 0;
            else if (SpritePosition.Y + SpriteSpeed.Y < minY)
                SpriteSpeed.Y = 0;

            //wall collision
            foreach (var wallPosition in wallPositions) {
                var p = new BoundingSphere(new Vector3(SpritePosition + Origin, 0), PlayerTexture.Height / 2);
                var r = new Rectangle((int)wallPosition.X, (int)wallPosition.Y, wallTexture.Width, wallTexture.Height);

                if (Collisions.Intersects(p, r)) {
                    double wy = (p.Radius + r.Height / 2) * (p.Center.Y - r.Center.Y);
                    double hx = (p.Radius + r.Width / 2) * (p.Center.X - r.Center.X);
                    if (wy > hx) {
                        if (wy > -hx && SpriteSpeed.Y < 0)
                            SpriteSpeed.Y = 0;
                        else if( wy <=- hx && SpriteSpeed.X > 0)
                            SpriteSpeed.X = 0;
                    }
                    else {
                        if (wy > -hx && SpriteSpeed.X < 0)
                            SpriteSpeed.X = 0;
                        else if(wy <= -hx && SpriteSpeed.Y > 0)  {
                            SpriteSpeed.Y = 0;
                        }
                    }
                }
            }
            SpritePosition += SpriteSpeed;
            _oldKeyboardState = newKeyboardState;
        }

        private void UpdateMouse(ref GraphicsDeviceManager graphics) {
            var newMouseState = Mouse.GetState();
            if (IsMouseInsideWindow(graphics, newMouseState)) {
                var newX = newMouseState.X + SpritePosition.X - graphics.GraphicsDevice.Viewport.Width / 2;
                var newY = newMouseState.Y + SpritePosition.Y - graphics.GraphicsDevice.Viewport.Height / 2;

                //rotation
                var rotationTemp = Convert.ToSingle(Math.Asin(Math.Abs(SpritePosition.X - newX) /
                    (Math.Sqrt(Math.Pow(SpritePosition.X - newX, 2.0) + Math.Pow(SpritePosition.Y - newY, 2.0)))));
                if (newX > SpritePosition.X && newY < SpritePosition.Y)
                    Rotation = rotationTemp;
                else if (newX > SpritePosition.X && newY > SpritePosition.Y)
                    Rotation = Convert.ToSingle(Math.PI) - rotationTemp;
                else if (newX < SpritePosition.X && newY > SpritePosition.Y)
                    Rotation = Convert.ToSingle(2 * Math.PI) - (Convert.ToSingle(Math.PI) - rotationTemp);
                else if (newX < SpritePosition.X && newY < SpritePosition.Y)
                    Rotation = Convert.ToSingle(2 * Math.PI) - rotationTemp;

                //fire
                if (newMouseState.LeftButton == ButtonState.Pressed && ammo > 0) {
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
        }

        bool IsMouseInsideWindow(GraphicsDeviceManager graphics, MouseState ms) {
            Point pos = new Point(ms.X, ms.Y);
            return graphics.GraphicsDevice.Viewport.Bounds.Contains(pos);
        }

        private void UpdateCollision(List<Bullet> bullets) {
            //distance between player's and bullet's centres
            foreach (var bullet in bullets) {
                var distance = Convert.ToSingle(
                    Math.Sqrt(Math.Pow(bullet.SpritePosition.X - SpritePosition.X - Origin.X, 2) +
                              Math.Pow(bullet.SpritePosition.Y - SpritePosition.Y - Origin.Y, 2)));
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
                if (SpritePosition.X + Origin.X >= ammoPosition.X && SpritePosition.X + Origin.X <= ammoPosition.X + TextureAtlas.Ammo.Width &&
                    SpritePosition.Y + Origin.Y >= ammoPosition.Y && SpritePosition.Y + Origin.Y <= ammoPosition.Y + TextureAtlas.Ammo.Height)
                    return true;
            }
            return false;
        }

        private void OnHitReact() {
            AudioAtlas.DeathScream.Play();
            deaths++;
            SpritePosition = Vector2.Zero;
            ammo = Config.MaxAmmoAmount;
        }

        public override string ToString() {
            string s1 = Origin.ToString();
            string s2 = SpritePosition.ToString();
            string s3 = SpriteSpeed.ToString();
            string bullets = string.Join("\nBullet: ", MyBullets.Select(x => x.ToString()).ToArray());
            return "\nUniqueKey: " + PlayerID + "\n\nBULLETS: " + bullets + "\nOrigin: " + s1 + "\nSpritePosition: " + s2 + "\nSpriteSpeed: " + s3 + 
                "\nRotation: " + Rotation + "\nAmmoAmount: " + ammo + "\nDeathsAmount: " + deaths + "\nDestroyMe: ";
        }
    }
}
