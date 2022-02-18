using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Collision;

namespace TowerDefense
{
    public abstract class Entity
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }

        public Shape Shape { get; set; }

        public Entity(Vector2 position)
        {
            Position = position;
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}