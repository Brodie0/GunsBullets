using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GunsBullets {
    class Map {
        private Texture2D _mapTexture;
        private Texture2D _wallTexture;
        private List<Vector2> _wallPositions;

        public List<Vector2> WallPositions => _wallPositions;
        public Texture2D WallTexture => _wallTexture;
        public Map(ContentManager content) {
            Random rand = new Random();
            _mapTexture = content.Load<Texture2D>(Config.MapTexture);
            _wallTexture = content.Load<Texture2D>(Config.WallTexture);
            _wallPositions = new List<Vector2>();
            for (int i = 0; i < 40; i++)
                _wallPositions.Add(new Vector2(rand.Next(14) * _wallTexture.Width, rand.Next(11) * _wallTexture.Height));
            /* _wallPositions.Add(new Vector2(6 * _wallTexture.Width, 2 * _wallTexture.Height));
             _wallPositions.Add(new Vector2(5 * _wallTexture.Width, 3 * _wallTexture.Height));
             _wallPositions.Add(new Vector2(6 * _wallTexture.Width, 3 * _wallTexture.Height));
             _wallPositions.Add(new Vector2(8 * _wallTexture.Width, 9 * _wallTexture.Height));
             _wallPositions.Add(new Vector2(8 * _wallTexture.Width, 8 * _wallTexture.Height));
             _wallPositions.Add(new Vector2(8 * _wallTexture.Width, 7 * _wallTexture.Height));
             _wallPositions.Add(new Vector2(8 * _wallTexture.Width, 6 * _wallTexture.Height));
             _wallPositions.Add(new Vector2(8 * _wallTexture.Width, 3 * _wallTexture.Height));*/
        }

        public void DrawMap(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(_mapTexture, Vector2.Zero, Color.White);
            foreach (var wallposition in _wallPositions) {
                spriteBatch.Draw(_wallTexture, wallposition, Color.White);
            }
        }
    }
}
