using System;

namespace GunsBullets {
    static class Program {
        static void Main(string[] args) {
            try {
                using (MainGame game = new MainGame(args)) {
                    game.Run();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}

