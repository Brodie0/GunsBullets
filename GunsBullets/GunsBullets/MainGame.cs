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
        private List<Bullet> bullets;
        private int _fireIter;
        private Map map;
        private Interface interf;
        private bool ifPressReload;

        public MainGame() {
            gdm = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
      
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            players = new List<Player>(Config.MaxNumberOfPlayers);
            bullets = new List<Bullet>();
            ifPressReload = false;
            _fireIter = 0;
            IsMouseVisible = true;
            base.Initialize();
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
            bullets.Clear();
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
                    Host.instance.Start();
                    for (int i = 0; i < Config.MaxNumberOfPlayers; i++) {
                        Task.Factory.StartNew(() => Host.instance.AddNewListeningThread());
                    }
                    interf.InitializeHost = false;
                }
                else if (Host.instance.Guests.Count != 0) {
                    Console.WriteLine("NO HOST TEZ");
                    if (players.Count > 1)
                        players.RemoveRange(1, players.Count - 1);
                    lock (Host.instance.Guests) {
                        players = players.Concat(Host.instance.Guests).ToList();
                    }
                    foreach (Player player in players) {
                        player.DeathScream = Content.Load<SoundEffect>(Config.Sound_DeathScream);
                        player.PlayerTexture = Content.Load<Texture2D>(Config.PlayerTexture);
                    }
                }
            }
            else if (interf.StopHosting) {
                Host.instance.Stop();
                interf.StopHosting = false;
            }

            if (interf.Guesting) {
                if (interf.InitializeGuest) {
                    Guest.instance.PlayerToSend = players.First();
                    Task.Factory.StartNew(() => Guest.instance.Start());
                    interf.InitializeGuest = false;
                }
                else {
                    Console.WriteLine("NO GUEST DZIALA");
                    players.First().ServerIdentificationNumber = Guest.instance.ServerIdentificationNumber;
                    Guest.instance.PlayerToSend = players.First();
                    Console.WriteLine(players.First().ToString());
                }
            }
            else if (interf.StopGuesting) {
                Guest.instance.Stop();
                interf.StopGuesting = false;
            }

            if (interf.ToggleFullScreen)
                gdm.ToggleFullScreen();

            //update only if window is focused
            if (IsActive) {
                Player player = players.First();
                player.UpdatePlayer(ref gdm, ref map, ref bullets, map.WallPositions, map.WallTexture);
                if (player.UpdateReloadPosition(map.AmmoPositions, map.AmmoTexture) && Keyboard.GetState().IsKeyDown(Keys.R) && !ifPressReload) {
                    ifPressReload = true;
                    player.AmmoReload(Content);
                }
                if (player.UpdateReloadPosition(map.AmmoPositions, map.AmmoTexture) && Keyboard.GetState().IsKeyUp(Keys.R) && ifPressReload)
                    ifPressReload = false;

                if (player.ContinuousFire) {
                    if (_fireIter == Config.FireRate) {
                        var bullet = new Bullet(ref gdm, Content, player.SpritePosition, player.Rotation,
                            player.OldMouseState, player.Origin);
                        player.DecreaseAmmo();
                        bullets.Add(bullet);
                        _fireIter = 0;
                    }
                    else
                        _fireIter++;
                }
                else if (player.SingleShot) {
                    var bullet = new Bullet(ref gdm, Content, player.SpritePosition, player.Rotation,
                        player.OldMouseState, player.Origin);
                    player.DecreaseAmmo();
                    bullets.Add(bullet);
                }
                else if (!player.ContinuousFire)
                    _fireIter = 0;

                _cameraPosition = players[0].SpritePosition;
                UpdateViewMatrix();

                players.RemoveAll(player1 => player1.DestroyMe);
                bullets.RemoveAll(bullet => bullet.DestroyMe);
                foreach (var bullet in bullets) {
                    bullet.UpdateBullet(ref gdm, ref map, map.WallPositions, map.WallTexture);
                }
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
                spriteBatch.DrawString(Content.Load<SpriteFont>("font"), "TEKST JAKIS", new Vector2(-300, 0), Color.White);

                foreach (var player in players) player.DrawPlayer(ref spriteBatch);
                foreach (var bullet in bullets) bullet.DrawBullet(ref spriteBatch);
            } spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
