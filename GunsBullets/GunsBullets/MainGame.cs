using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GunsBullets {
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
        private GameInput input;

        private Host host;
        private Guest guest;

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
            _fireIter = 0;
            IsMouseVisible = true;
            base.Initialize();

            try {
                Window.Title = string.Format("[{0}:{1}] GunsBullets", Utilities.GetLocalIPAddress(), Config.Port);
            } catch {
                Window.Title = "[UNKNOWN!] GunsBullets";
            }

            if (Config.HostGame) {
                host = new Host(ref players);
                host.AddListeners();
            } else { // Guest!
                guest = new Guest(ref players);
                Task.Factory.StartNew(() => guest.StartCommunicationThread());
            }
        }

        private void UpdateViewMatrix() {
            viewMatrix = Matrix.CreateTranslation(m_halfViewSize.X - _cameraPosition.X, m_halfViewSize.Y - _cameraPosition.Y, 0.0f);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            base.LoadContent();
            TextureAtlas.Initialize(Content, gdm.GraphicsDevice);
            AudioAtlas.Initialize(Content);

            map = new Map(Content);
            input = new GameInput(gdm);
            
            spriteBatch = new SpriteBatch(GraphicsDevice);
            players.Add(new Player(Content, Config.Nickname, map));
            OSD.playerToTrack = players[0];

            _cameraPosition = players[0].Position;
            m_halfViewSize = new Vector2(gdm.GraphicsDevice.Viewport.Width * 0.5f, gdm.GraphicsDevice.Viewport.Height * 0.5f);
            UpdateViewMatrix();
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

            input.Update();
            OSD.Update(gameTime);

            if (!Config.HostGame) {
                lock (guest.PlayerToSend) {
                    guest.PlayerToSend = players.First();
                }
            }

            if (input.ToggleFullScreen) {
                gdm.ToggleFullScreen();
                input.ToggleFullScreen = false;
            }

            Player localPlayer = players.First();
            localPlayer.UpdatePlayer(ref gdm, input, map, allBullets);
            
            _cameraPosition = localPlayer.Position;
            UpdateViewMatrix();

            
            if (IsActive) { // update only if window is focused
                if (localPlayer.ContinuousFire) { //shooting
                    if (_fireIter == Config.FireRate) {
                        Bullet bullet = new Bullet(ref gdm, localPlayer, input);
                        localPlayer.Ammo--;
                        localPlayer.MyBullets.Add(bullet);
                        _fireIter = 0;
                    } else _fireIter++;
                } else if (localPlayer.SingleShot) {
                    Bullet bullet = new Bullet(ref gdm, localPlayer, input);
                    localPlayer.Ammo--;
                    localPlayer.MyBullets.Add(bullet);
                } else if (!localPlayer.ContinuousFire) {
                    _fireIter = 0;
                }
            }

            localPlayer.MyBullets.RemoveAll(b => b.DestroyMe);
            allBullets.Clear();
            lock (players) {
                foreach (Player p in players) {
                    allBullets = allBullets.Concat(p.MyBullets).ToList();
                }
            }

            foreach (var bullet in localPlayer.MyBullets) {
                bullet.Update(ref gdm, ref map, map.Walls);
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
                lock (allBullets) foreach (var bullet in allBullets) if (bullet != null) bullet.DrawBullet(ref spriteBatch);
                OSD.Draw(ref gdm, ref spriteBatch, _cameraPosition - m_halfViewSize);
            } spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
