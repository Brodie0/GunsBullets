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

        public Map(ContentManager content) {
            WallPositions = new List<Vector2>();
            AmmoPositions = new List<Vector2>();

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
                            WallPositions.Add(new Vector2(j / Config.SpacesForEachSignInTxtMap * TextureAtlas.Wall.Width, i * TextureAtlas.Wall.Height));
                        else if (sign == Config.AmmoSignInTxtMap)
                            AmmoPositions.Add(new Vector2(j / Config.SpacesForEachSignInTxtMap * TextureAtlas.Ammo.Width, i * TextureAtlas.Ammo.Height));
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
