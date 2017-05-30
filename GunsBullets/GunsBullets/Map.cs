using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GunsBullets {
    class Map {
        public List<Vector2> WallPositions;
        public List<Vector2> AmmoPositions;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Map(ContentManager content) {
            WallPositions = new List<Vector2>();
            AmmoPositions = new List<Vector2>();

            // Load our map:
            string line = "# Lines starting with the # symbol will be ignored!";
            using (var sr = new StreamReader(Config.ContentPath + Config.WallAndAmmoPositions)) {
                do { line = sr.ReadLine(); } while (string.IsNullOrWhiteSpace(line) || line[0] == '#');
                string[] header = line.Split(' ');

                Width = Int32.Parse(header[0]);
                Height = Int32.Parse(header[1]);

                for (int y = 0; y < Height; y++) {
                    do { line = sr.ReadLine(); } while (string.IsNullOrWhiteSpace(line) || line[0] == '#');
                    string[] mapObjects = line.Split(' ');

                    for (int x = 0; x < Width; x++) {
                        int thing = Int32.Parse(mapObjects[x]);
                        switch (thing) {
                        case 1:
                            WallPositions.Add(new Vector2(x * TextureAtlas.Wall.Width, y * TextureAtlas.Wall.Height));
                            break;
                        case 2:
                            AmmoPositions.Add(new Vector2(x * TextureAtlas.Ammo.Width, y * TextureAtlas.Ammo.Height));
                            break;
                        }
                    }
                }
            }
        }

        public void DrawMap(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(TextureAtlas.Map, Vector2.Zero, Color.White);
            foreach (var wallPos in WallPositions) spriteBatch.Draw(TextureAtlas.Wall, wallPos, Color.White);
            foreach (var ammoPos in AmmoPositions) spriteBatch.Draw(TextureAtlas.Ammo, ammoPos, Color.White);
        }
    }
}
