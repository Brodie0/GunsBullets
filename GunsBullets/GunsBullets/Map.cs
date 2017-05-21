using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GunsBullets {
    class Map {
        public readonly Texture2D _mapTexture;
        private Texture2D _wallTexture;
        private Texture2D _ammoTexture;
        private List<Vector2> _wallPositions;
        private List<Vector2> _ammoPosition;

        public List<Vector2> WallPositions => _wallPositions;
        public Texture2D WallTexture => _wallTexture;
        public Map(ContentManager content) {
            Random rand = new Random();
            var filestream = new FileStream("..\\..\\..\\..\\GunsBulletsContent\\mapa.txt", FileMode.Open, FileAccess.Read);
            char sign;
            _mapTexture = content.Load<Texture2D>(Config.MapTexture);
            _wallTexture = content.Load<Texture2D>(Config.WallTexture);
            _ammoTexture = content.Load<Texture2D>(Config.AmmoTexture);
            Config.AmmoPosition = _ammoTexture;
            _wallPositions = new List<Vector2>();
            _ammoPosition = new List<Vector2>();
            
            _ammoPosition.Add(new Vector2(_ammoTexture.Width, 0));
            _ammoPosition.Add(new Vector2(28*_ammoTexture.Width, 0));
            _ammoPosition.Add(new Vector2(_ammoTexture.Width, 20*_ammoTexture.Height));
            _ammoPosition.Add(new Vector2(28*_ammoTexture.Width, 20 * _ammoTexture.Height));
            for (int i=0; i < 21; i++)
                for(int j=0; j < 61; j++)
                {
                    sign = (char)filestream.ReadByte();
                    if(sign == '1')
                        _wallPositions.Add(new Vector2(j/2 * _wallTexture.Width, i * _wallTexture.Height));
                }
        }

        public void DrawMap(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(_mapTexture, Vector2.Zero, Color.White);
            foreach (var wallposition in _wallPositions) {
                spriteBatch.Draw(_wallTexture, wallposition, Color.White);
            }
            foreach(var ammoposition in _ammoPosition)
                spriteBatch.Draw(_ammoTexture, ammoposition, Color.White);
        }
    }
}
