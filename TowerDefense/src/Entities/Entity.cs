using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;

using System;

namespace TowerDefense.Entities
{
    public abstract class Entity
    {
        private Vector2 _position;
        public Vector2 Position
        {
            get { return _position; }
            // changing the position also changes its shape's position
            set
            {
                _position = value;
                if (Shape != null)
                    Shape.Position = value;
            }
        }
        public Vector2 Velocity { get; set; }
        public Collision.Shape Shape { get; set; }

        protected AnimationState animationState;

        public abstract void Update(float dt);

        public abstract void Draw(SpriteBatch spriteBatch);
        
        public void DrawDebug(SpriteBatch spriteBatch)
        {
            Shape.Draw(spriteBatch, Color.Black, 1);
        }
    }
}