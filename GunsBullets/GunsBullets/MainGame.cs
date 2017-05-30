using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Threading.Tasks;
using System.Net;

namespace GunsBullets {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager gdm;
        SpriteBatch spriteBatch;
        private Vector2 _cameraPosition;
        private Vector2 m_halfViewSize;
        public Matrix viewMatrix;

        private List<Player> players;
        private List<Bullet> allBullets;
        private int _fireIter;
        private Map map;
        private Interface interf;
        private bool ifPressReload;

        public MainGame() {
            gdm = new GraphicsDeviceManager(this);
            Content.RootDirectory = Config.ContentPath;
        }
      
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            players = new List<Player>();
            allBullets = new List<Bullet>();
            ifPressReload = false;
            _fireIter = 0;
            IsMouseVisible = true;
            base.Initialize();

            try {
                Window.Title = string.Format("[{0}] GunsBullets", Interface.GetLocalIPAddress());
            } catch {
                Window.Title = "[UNKNOWN!] GunsBullets";
            }
        }

        private void UpdateViewMatrix()
        {
            viewMatrix = Matrix.CreateTranslation(m_halfViewSize.X - _cameraPosition.X, m_halfViewSize.Y - _cameraPosition.Y, 0.0f);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            players.Add(new Player(Content));
            _cameraPosition = players[0].SpritePosition;
            m_halfViewSize = new Vector2(gdm.GraphicsDevice.Viewport.Width * 0.5f, gdm.GraphicsDevice.Viewport.Height * 0.5f);
            UpdateViewMatrix();
            map = new Map(Content);
            interf = new Interface();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            spriteBatch.Dispose();
            players.Clear();
            allBullets.Clear();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {

            interf.Update();
            if (interf.Hosting == true) {
                //TODO host and guest Task variables are just for checking existance of those objects, could be replaced by booleans
                //or just other ideas for checking
                if (interf.InitializeHost) {
                    lock (Content)
                        Host.instance.Start(ref players, Content);
                    Host.instance.AddNewListeningThread(Config.MaxNumberOfGuests);
                    interf.InitializeHost = false;
                }
            }
            else if (interf.StopHosting) {
                Host.instance.Stop(players);
                interf.StopHosting = false;
            }

            if (interf.Guesting) {
                if (interf.InitializeGuest) {
                    lock (Content)
                        Guest.instance.Start(players, Content);
                    Task.Factory.StartNew(() => Guest.instance.StartCommunicationThread());
                    interf.InitializeGuest = false;
                }
                else {
                    lock(Guest.instance.PlayerToSend){
                        Guest.instance.PlayerToSend = players.First();
                    }
                }
            }
            else if (interf.StopGuesting) {
                Guest.instance.Stop();
                interf.StopGuesting = false;
            }

            if (interf.ToggleFullScreen)
                gdm.ToggleFullScreen();

            Player player1 = players.First();
            player1.UpdatePlayer(ref gdm, ref map, ref allBullets, map.WallPositions, map.WallTexture);
            if (player1.UpdateReloadPosition(map.AmmoPositions, map.AmmoTexture) && Keyboard.GetState().IsKeyDown(Keys.R) && !ifPressReload) {
                ifPressReload = true;
                lock (Content)
                    player1.AmmoReload(Content);
            }
            if (player1.UpdateReloadPosition(map.AmmoPositions, map.AmmoTexture) && Keyboard.GetState().IsKeyUp(Keys.R) && ifPressReload)
                ifPressReload = false;
            //update only if window is focused
            if (IsActive) {
                //shooting
                if (player1.ContinuousFire) {
                    if (_fireIter == Config.FireRate) {
                        Bullet bullet = null;
                        lock (Content)
                            bullet = new Bullet(ref gdm, Content, player1.SpritePosition, player1.Rotation,
                            player1.OldMouseState, player1.Origin);
                        player1.DecreaseAmmo();
                        player1.MyBullets.Add(bullet);
                        _fireIter = 0;
                    }
                    else
                        _fireIter++;
                }
                else if (player1.SingleShot) {
                    Bullet bullet = null;
                    lock (Content)
                        bullet = new Bullet(ref gdm, Content, player1.SpritePosition, player1.Rotation,
                        player1.OldMouseState, player1.Origin);
                    player1.DecreaseAmmo();
                    player1.MyBullets.Add(bullet);
                }
                else if (!player1.ContinuousFire)
                    _fireIter = 0;

                _cameraPosition = player1.SpritePosition;
                UpdateViewMatrix();
            }

            player1.MyBullets.RemoveAll(b => b.DestroyMe);
            allBullets.Clear();
            lock (players) {
                foreach (Player p in players) {
                    allBullets = allBullets.Concat(p.MyBullets).ToList();
                }
            }
            foreach (var bullet in player1.MyBullets) {
                bullet.UpdateBullet(ref gdm, ref map, map.WallPositions, map.WallTexture);
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);
            // Draw the sprite. (This isn't a language construct!)
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, viewMatrix); {
                map.DrawMap(ref spriteBatch);
                lock (players) foreach (var player in players) player.DrawPlayer(ref spriteBatch);
                lock (allBullets) foreach (var bullet in allBullets) bullet.DrawBullet(ref spriteBatch);
            } spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
