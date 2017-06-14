using System;
using System.IO;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GunsBullets {
    public class Wall {
        public Vector2 Position, Size, Center;
        public BoundingBox Bound;
        public bool ClippedInto = false;

        public Wall(Vector2 pos, Vector2 size) {
            Position = pos;
            Size = size;
            Center = Vector2.Lerp(Position, Position + Size, 0.5f);
            Bound = new BoundingBox(new Vector3(Position, 0), new Vector3(Position + Size, 0));
        }

        public Wall(float x, float y, float width, float height) : this(new Vector2(x, y), new Vector2(width, height)) {
            // Everthing gets calc'd in the constructor above! :)
        }
    }

    class Map {
        public List<Vector2> SpawnPositions;
        public List<Vector2> AmmoPositions;
        public List<Wall> Walls;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Map(ContentManager content) {
            Walls = new List<Wall>();
            AmmoPositions = new List<Vector2>();
            SpawnPositions = new List<Vector2>();

            // Load our map:
            string line = "# Lines starting with the # symbol will be ignored!";
            using (var sr = new StreamReader(Config.ContentPath + Config.WallAndAmmoPositions)) {
                do { line = sr.ReadLine(); } while (string.IsNullOrWhiteSpace(line) || line[0] == '#');
                string[] header = line.Split(' ');

                Width = Int32.Parse(header[0]);
                Height = Int32.Parse(header[1]);

                int UnitWidth = TextureAtlas.Wall.Width, UnitHeight = TextureAtlas.Wall.Height;

                for (int y = 0; y < Height; y++) {
                    do { line = sr.ReadLine(); } while (string.IsNullOrWhiteSpace(line) || line[0] == '#');
                    string[] mapObjects = line.Split(' ');

                    for (int x = 0; x < Width; x++) {
                        int thing = Int32.Parse(mapObjects[x]);
                        switch (thing) {
                        case 1:
                            Walls.Add(new Wall(
                                x * UnitWidth,
                                y * UnitHeight,
                                TextureAtlas.Wall.Width,
                                TextureAtlas.Wall.Height));
                            break;
                        case 2:
                            AmmoPositions.Add(new Vector2(x * UnitWidth, y * UnitHeight));
                            break;
                        case 9:
                            SpawnPositions.Add(new Vector2(x * UnitWidth, y * UnitHeight));
                            break;
                        }
                    }
                }

                //     Top (3)
                //   +----10----+
                // L |XXXXXXXXXX| R  Our map is loaded, but we also need walls for all the borders.
                // 1 3          3 2  From our width and height add 4 more bounding walls. They're
                // L |          | R  all 100 pixels wide/high, spanning the entire width/height.
                //   +----10----+
                //    Bottom (4)

                Walls.Add(new Wall(-100.0f, -100.0f,
                    100.0f, 200.0f + (Height * TextureAtlas.Wall.Height))); // 1
                Walls.Add(new Wall(TextureAtlas.Wall.Width * Width, -100.0f,
                    100.0f, 200.0f + (Height * TextureAtlas.Wall.Height))); // 2
                Walls.Add(new Wall(-100.0f, -100.0f,
                    200.0f + (Width * TextureAtlas.Wall.Width), 100.0f)); // 3
                Walls.Add(new Wall(-100.0f, (Height * TextureAtlas.Wall.Height),
                    200.0f + (Width * TextureAtlas.Wall.Width), 100.0f)); // 4
            }
        }

        public void DrawMap(ref SpriteBatch spriteBatch) {
            spriteBatch.Draw(TextureAtlas.Map, Vector2.Zero, Color.White);
            foreach (var wall in Walls) spriteBatch.Draw(TextureAtlas.Wall, wall.Position, Color.White);
            foreach (var ammoPos in AmmoPositions) spriteBatch.Draw(TextureAtlas.Ammo, ammoPos, Color.White);

            if (Config.DebugMode) {
                foreach (var wall in Walls) {
                    Color debugDrawColor = Color.Green;
                    if (wall.ClippedInto) {
                        wall.ClippedInto = false;
                        debugDrawColor = Color.MidnightBlue;
                    }

                    spriteBatch.Draw(TextureAtlas.DummyTexture,
                        new Rectangle((int)wall.Bound.Min.X, (int)wall.Bound.Min.Y,
                        (int)wall.Size.X, (int)wall.Size.Y),
                        debugDrawColor);
                }
            }
        }
    }
}
