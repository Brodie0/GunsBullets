using System;

using Microsoft.Xna.Framework;

namespace GunsBullets {
    class Collisions {
        public static bool Intersects(BoundingSphere circle, Rectangle rect) {
            Vector2 circleDistance;
            circleDistance.X = Math.Abs(circle.Center.X - rect.Center.X);
            circleDistance.Y = Math.Abs(circle.Center.Y - rect.Center.Y);

            if (circleDistance.X > (rect.Width / 2 + circle.Radius)) { return false; }
            if (circleDistance.Y > (rect.Height / 2 + circle.Radius)) { return false; }

            if (circleDistance.X <= (rect.Width / 2)) { return true; }
            if (circleDistance.Y <= (rect.Height / 2)) { return true; }

            double arg1 = Math.Pow(circleDistance.X - rect.Width / 2, 2);
            double arg2 = Math.Pow(circleDistance.Y - rect.Height / 2, 2);
            double cornerDistance_sq = arg1 + arg2;

            return (cornerDistance_sq <= Math.Pow(circle.Radius, 2));
        }
    }
}
