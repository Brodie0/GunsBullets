using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace GunsBullets {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainGame : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager gdm;
        SpriteBatch spriteBatch;

        private List<Player> players;
        private List<Bullet> bullets;
        private int _fireIter;
        private Map map;

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
            players = new List<Player>();
            bullets = new List<Bullet>();
            _fireIter = 0;

            IsMouseVisible = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            base.LoadContent();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            players.Add(new Player(Content));
            map = new Map(Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            spriteBatch.Dispose();
            bullets.Clear();
            Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) Exit();

            foreach (var player in players) {
                player.UpdatePlayer(ref gdm, ref bullets, map.WallPositions, map.WallTexture);
                
                if (player.ContinuousFire) {
                    if (_fireIter == Config.FireRate) {
                        var bullet = new Bullet(Content, player.SpritePosition, player.Rotation,
                            player.OldMouseState, player.Origin);
                        player.DecreaseAmmo();
                        bullets.Add(bullet);
                        _fireIter = 0;
                    } else _fireIter++;
                } else if (player.SingleShot) {
                    var bullet = new Bullet(Content, player.SpritePosition, player.Rotation,
                        player.OldMouseState, player.Origin);
                    player.DecreaseAmmo();
                    bullets.Add(bullet);
                } else if (!player.ContinuousFire)
                    _fireIter = 0;

            }

            players.RemoveAll(player => player.DestroyMe);
            
            foreach (var bullet in bullets) {
                bullet.UpdateBullet(ref gdm, map.WallPositions, map.WallTexture);
            }

            bullets.RemoveAll(bullet => bullet.DestroyMe);
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the sprite. (This isn't a language construct!)
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend); {
                map.DrawMap(ref spriteBatch);
                foreach (var player in players) player.DrawPlayer(ref spriteBatch);
                foreach (var bullet in bullets) bullet.DrawBullet(ref spriteBatch);
            } spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}
