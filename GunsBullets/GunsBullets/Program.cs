using System;

namespace GunsBullets {
    static class Program {
        static void Main(string[] args) {
            using (MainGame game = new MainGame()) {
                game.Run();
            }
        }
    }
}

