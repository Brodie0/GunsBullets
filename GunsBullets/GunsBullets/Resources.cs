using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GunsBullets {
    class TextureAtlas {
        public static Texture2D Map, Wall;
        public static Texture2D Bullet, Ammo;
        public static Texture2D[] Player;
        
        public static void Initialize(ContentManager content) {
            Map = content.Load<Texture2D>("map");
            Wall = content.Load<Texture2D>("wall");
            Bullet = content.Load<Texture2D>("bullet");
            Ammo = content.Load<Texture2D>("ammo-crate");

            Player = new Texture2D[4];
            for (int i = 0; i < 4; i++) Player[i] = content.Load<Texture2D>("soldier" + i.ToString());
        }
    }

    class AudioAtlas {
        public static SoundEffect Shot, Reload, DeathScream;
        public static SoundEffect[] Ricochet;

        public static void Initialize(ContentManager content) {
            Shot = content.Load<SoundEffect>("gun-pew");
            Reload = content.Load<SoundEffect>("gun-reload");
            DeathScream = content.Load<SoundEffect>("player-scream");

            Ricochet = new SoundEffect[2];
            Ricochet[0] = content.Load<SoundEffect>("ricochet0");
            Ricochet[1] = content.Load<SoundEffect>("ricochet1");
        }
    }
}
