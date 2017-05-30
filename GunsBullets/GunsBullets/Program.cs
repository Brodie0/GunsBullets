using System;

namespace GunsBullets {
    static class Program {
        static void Main(string[] args) {
            try {
                System.Windows.Forms.Application.EnableVisualStyles();
                GameConfigurator gameCfg = new GameConfigurator();
                gameCfg.ShowDialog(); // Will block until the dialog closes.

                if (gameCfg.Launch) {
                    using (MainGame game = new MainGame()) game.Run();
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}

