using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GunsBullets {
    class Config {
        public static string ContentPath = "..\\..\\..\\Content\\Content";
        public static float BulletSpeed = 15.0f;
        public static float PlayerMaxSpeed = 5.0f;
        public static string PlayerTexture = "soldier1";
        public static string BulletTexture = "bullet1";
        public static int FireRate = 5;
        public static int BulletAppearDistanceFromPlayer = 15;
        public static string BulletSoundEffect = "m4a1single";
        public static string AmmoReloadSound = "gunreload";
        public static int RicochetesSoundsAmount = 2;
        public static string Ricochet1 = "ricochet1";
        public static string Ricochet2 = "ricochet2";
        public static string DeathScream = "wilhelmScream";
        public static string MapTexture = "map2";
        public static string WallTexture = "wall";
        public static string AmmoTexture = "__Ammo-256";
        public static int RicochetProbability = 2; // 1 / N, 1->100%, 5->20% etc.
        public static int AmmoAmount = 100;
        public static Texture2D AmmoPosition = null;
    }
}
