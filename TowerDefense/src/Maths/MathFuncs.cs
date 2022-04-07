using Microsoft.Xna.Framework;

using System;

namespace TowerDefense.Maths
{
    public static class MathFuncs
    {
        public static Vector2 MoveTowards(this Vector2 curr, Vector2 target, float force)
        {
            Vector2 diff = target - curr;
            Vector2 change = diff.Normalized() * force;
            // clamp value based to the difference
            change = (diff.LengthSquared() < change.LengthSquared() ? diff : change);
            return curr + change;
        }

        /// <summary>
        /// Note: returns Vector2.Zero if Vector2 is Zero.
        /// </summary>
        public static Vector2 Normalized(this Vector2 curr)
        {
            if (curr == Vector2.Zero)
                return Vector2.Zero;
            return Vector2.Normalize(curr);
        }

        public static float InvLerp(this float min, float max, float value)
        {
            return (value - min) / (max - min);
        }
    }
}