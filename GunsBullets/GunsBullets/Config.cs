using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GunsBullets {
    class Config {
        public static string ContentPath = "..\\..\\..\\..\\GunsBulletsContent\\";
        //textures
        public static string[] PlayerTexture = { "uberSoldier1", "uberSoldier2", "uberSoldier3", "uberSoldier4" };
        public static string BulletTexture = "bullet1";
        public static string MapTexture = "map2";
        public static string WallTexture = "wall";
        public static string AmmoTexture = "ammo";
        public static string WallAndAmmoPositions = "mapa.txt";
        //sounds
        public static string Sound_Shot = "m4a1single";
        public static string Sound_Reload = "gunreload";
        public static string Sound_Ricochet1 = "ricochet1";
        public static string Sound_Ricochet2 = "ricochet2";
        public static string Sound_DeathScream = "wilhelmScream";
        //other
        public static int RicochetesSoundsAmount = 2;
        public static int FireRate = 10;
        public static int BulletAppearDistanceFromPlayer = 15;
        public static float BulletSpeed = 15.0f;
        public static float PlayerMaxSpeed = 5.0f;

        public static int RicochetProbability = 4; // 1 / N, 1->100%, 5->20% etc.
        public static int MaxAmmoAmount = 100;
        public static Texture2D AmmoPosition = null;
        public static char WallSignInTxtMap = '1';
        public static char AmmoSignInTxtMap = '2';
        public static short SpacesForEachSignInTxtMap = 2;
        public static short Port = 8888;
        public static string Localhost = "127.0.0.1";
        public static int MaxNumberOfPlayers = 4;
        public static int MaxNumberOfGuests = MaxNumberOfPlayers - 1;
        public static int SendingPackagesDelay = 5;

    }
}
