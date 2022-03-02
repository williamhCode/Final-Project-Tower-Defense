using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;

using System;

namespace TowerDefense.Entities
{
    public abstract class Building : Entity
    {
        public Building(Vector2 position)
        {
            Shape = new Collision.Rectangle(position, 32, 32);
            Position = position;
            Velocity = new Vector2(0, 0);
        }

        public override void Update(float dt)
        {
            animationState.Update(dt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Shape.Draw(spriteBatch, new Color(0, 0, 0), 1);

            animationState.Sprite.Draw(spriteBatch, Position);
        }
    }
}