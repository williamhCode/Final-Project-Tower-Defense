using Microsoft.Xna.Framework;

namespace TowerDefense
{
    public static class MathFuncs
    {
        public static Vector2 MoveTowards(this Vector2 curr, Vector2 target, float intensity)
        {
            return curr + Vector2.Ceiling((target - curr).Normalized() * intensity);
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
    }
}