using System;
using System.Diagnostics;

namespace GunsBullets {
    static class Program {
        static void Main(string[] args) {
            System.Windows.Forms.Application.EnableVisualStyles();
            GameConfigurator gameCfg = new GameConfigurator();
            gameCfg.ShowDialog(); // Will block until the dialog closes.

            if (gameCfg.Launch) using (MainGame game = new MainGame()) game.Run();
        }
    }
}

