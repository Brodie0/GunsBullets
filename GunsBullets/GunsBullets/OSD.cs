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
            if (playerToTrack != null) {
                // Draw the basic UI
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
