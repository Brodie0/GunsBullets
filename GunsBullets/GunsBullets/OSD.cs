using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GunsBullets {
    class LogEntry {
        public string Text;
        public double Duration;

        public LogEntry(string text, double duration) {
            Text = text;
            Duration = duration;
        }
    }

    class OSD {
        public static Player playerToTrack = null;
        public static List<LogEntry> log = new List<LogEntry>();

        public static void LogEvent(string text, double duration) {
            log.Add(new LogEntry(text, duration));
        }

        public static void Update(GameTime dt) {
            foreach (LogEntry entry in log) entry.Duration -= (0.001 * dt.ElapsedGameTime.Milliseconds);
            log.RemoveAll(entry => entry.Duration <= 0);
        }

        public static void Draw(ref GraphicsDeviceManager gdm, ref SpriteBatch sb, Vector2 camPosition) {
            if (playerToTrack != null) { // Draw the basic UI
                Vector2 position = new Vector2(sb.GraphicsDevice.Viewport.Width, sb.GraphicsDevice.Viewport.Height);

                Vector2 nicknameTextSize = TextureAtlas.Font.MeasureString(playerToTrack.Nickname);
                Vector2 ammoTextSize = TextureAtlas.Font.MeasureString($"{playerToTrack.Ammo}");
                Vector2 deathsTextSize = TextureAtlas.Font.MeasureString($"{playerToTrack.Deaths}");
                
                // Player's nickname
                sb.DrawString(TextureAtlas.Font, playerToTrack.Nickname, position - nicknameTextSize - new Vector2(3, 3) + camPosition, Color.WhiteSmoke);

                // Icons
                int iconSide = (int) Math.Max(ammoTextSize.Y, deathsTextSize.Y);

                // In the lower right corner, move by (-3, -3), then by (-iconSide, -iconSide),
                // then if we're working in Y, move up by another 3px and nickname's text size
                // to avoid covering it. Draw AmmoIcon on that.
                Rectangle iconDestinationRect = new Rectangle(
                    (int)position.X - 3 - iconSide + (int)camPosition.X,
                    (int)position.Y - 3 - iconSide - 3 - (int)nicknameTextSize.Y + (int)camPosition.Y,
                    iconSide, iconSide);
                sb.Draw(TextureAtlas.AmmoIcon, iconDestinationRect, Color.WhiteSmoke);

                // Move by one line up for the DeathsIcon.
                iconDestinationRect.Y -= (3 + iconSide);
                sb.Draw(TextureAtlas.DeathsIcon, iconDestinationRect, Color.WhiteSmoke);

                // Draw the ammo text, colored properly if we're low on ammo
                Vector2 ammoTextPos = position - new Vector2(6 + iconSide, 6) - ammoTextSize;
                ammoTextPos.Y -= nicknameTextSize.Y;
                sb.DrawString(TextureAtlas.Font, $"{playerToTrack.Ammo}", ammoTextPos + camPosition, playerToTrack.Ammo <= 5 ? Color.Red : Color.WhiteSmoke);

                Vector2 deathsTextPos = position - new Vector2(6 + iconSide, 9) - deathsTextSize;
                deathsTextPos.Y -= (nicknameTextSize.Y + ammoTextSize.Y);
                sb.DrawString(TextureAtlas.Font, $"{playerToTrack.Deaths}", deathsTextPos + camPosition, Color.WhiteSmoke);
            }

            if (log.Count == 0) return;

            // Draw us some log entries!
            for (int i = 0; i < log.Count; i++) {
                Vector2 position = new Vector2(0, sb.GraphicsDevice.Viewport.Height);
                position.Y -= TextureAtlas.Font.MeasureString(log[i].Text).Y * (i + 1);
                sb.DrawString(TextureAtlas.Font, log[i].Text, position + camPosition, Color.WhiteSmoke);
            }
        }
    }
}
