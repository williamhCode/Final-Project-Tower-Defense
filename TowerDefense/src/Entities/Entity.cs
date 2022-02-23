using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

using TowerDefense.Sprite;
using TowerDefense.Math;

using Collision;

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
                Shape.Position = value;
            }
        }
        public Vector2 Velocity { get; set; }
        public virtual Collision.Shape Shape { get; set; }
        protected AnimatedSprite _sprite;

        public abstract void Update(float dt);
        public abstract void Draw(SpriteBatch spriteBatch);
    }

    
    public class Player : Entity
    {
        private const float MAX_SPEED = 300;
        private const float FRICTION = 1000;
        private const float ACCELERATION = 1000;

        public Player(Vector2 position)
        {
            Shape = new Circle(position, 20);
            Position = position;
            Velocity = new Vector2(0, 0);

            // _sprite = new AnimatedSprite()
        }

        public void Move(Vector2 direction, float dt)
        {
            direction = direction.Normalized();
            if (direction == Vector2.Zero)
            {
                Velocity = Velocity.MoveTowards(Vector2.Zero, FRICTION * dt);
            }
            else
            {
                Velocity = Velocity.MoveTowards(direction * MAX_SPEED, ACCELERATION * dt);
            }
        }

        public override void Update(float dt)
        {
            Position += Velocity * dt;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Shape.Draw(spriteBatch, new Color(0, 0, 0), 2);
        }
    }
}