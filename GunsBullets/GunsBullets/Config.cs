namespace GunsBullets {
    class Config {
        // TODO: Remove this wherever it's used - used to load map data etc, will
        // break once the project is built and released without the source code!
        // Map tokens are now defined in Map.cs (during the loading).
        public static string ContentPath = "..\\..\\..\\..\\GunsBulletsContent\\";
        public static string WallAndAmmoPositions = "mapa.txt";
        
        //other
        public static int RicochetSounds = 2;
        public static int FireRate = 10;
        public static int BulletAppearDistanceFromPlayer = 15;
        public static float BulletSpeed = 15.0f;
        public static float PlayerMaxSpeed = 5.0f;

        public static int RicochetProbability = 4; // 1 / N, 1->100%, 5->20% etc.
        public static int MaxAmmoAmount = 100;
        
        public static int MaxNumberOfPlayers = 4;
        public static int MaxNumberOfGuests = MaxNumberOfPlayers - 1;
        public static int SendingPackagesDelay = 5;

        // These settings can be changed with the GameConfigurator:
        public static string Nickname = "Player";
        public static string IPHostname = "127.0.0.1";
        public static short Port = 8888;

        public static bool HostGame = false;
        public static bool DebugMode = true;
        public static bool FullScreen = false;
        public static bool GamePadEnabled = false;
    }
}
