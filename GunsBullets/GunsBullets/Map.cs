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
        private List<Vector2> _ammoPositions;

        public List<Vector2> WallPositions => _wallPositions;
        public List<Vector2> AmmoPositions => _ammoPositions;
        public Texture2D WallTexture => _wallTexture;
        public Texture2D AmmoTexture => _ammoTexture;

        public Map(ContentManager content) {
            _mapTexture = content.Load<Texture2D>(Config.MapTexture);
            _wallTexture = content.Load<Texture2D>(Config.WallTexture);
            _ammoTexture = content.Load<Texture2D>(Config.AmmoTexture);
            Config.AmmoPosition = _ammoTexture;
            _wallPositions = new List<Vector2>();
            _ammoPositions = new List<Vector2>();

            //load map dimensions
            int height = 0;
            int width = 0;
            using (var str = new StreamReader(Config.ContentPath + Config.WallAndAmmoPositions)) {
                string firstLine = str.ReadLine();
                width = firstLine.Length + Environment.NewLine.Length; //61
                str.DiscardBufferedData();
                str.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                int allChars = str.ReadToEnd().Length;
                height = allChars / width; //20
            }

            //load all walls, ammo etc.
            using (var fileStream = new FileStream(Config.ContentPath + Config.WallAndAmmoPositions, FileMode.Open, FileAccess.Read)) {
                for (int i = 0; i <= height; i++) {
                    for (int j = 0; j < width; j++) {
                        var sign = (char)fileStream.ReadByte();
                        if (sign == Config.WallSignInTxtMap)
                            _wallPositions.Add(new Vector2(j / Config.SpacesForEachSignInTxtMap * _wallTexture.Width, i * _wallTexture.Height));
                        else if (sign == Config.AmmoSignInTxtMap)
                            _ammoPositions.Add(new Vector2(j / Config.SpacesForEachSignInTxtMap * _ammoTexture.Width, i * _ammoTexture.Height));
                    }
                }
            }
        }

        public void DrawMap(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(_mapTexture, Vector2.Zero, Color.White);
            foreach (var wallposition in _wallPositions) {
                spriteBatch.Draw(_wallTexture, wallposition, Color.White);
            }
            foreach(var ammoposition in _ammoPositions)
                spriteBatch.Draw(_ammoTexture, ammoposition, Color.White);
        }
    }
}
