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
    public class Player : Entity
    {
        private const float MAX_SPEED = 200;
        private const float FRICTION = 1200;
        private const float ACCELERATION = 1200;

        public static AnimationState AnimationState;

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Player";

            float frameTime = 0.05f;
            AnimationState = new AnimationState();
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player"), 32, 32, frameTime), "idle", "right");
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player"), 32, 32, frameTime, flipped: true), "idle", "left");
            AnimationState.State = "idle";
            AnimationState.Direction = "right";
        }

        public Player(Vector2 position)
        {
            Position = position;
            Velocity = new Vector2(0, 0);
            Shape = new Circle(position, 5);

            animationState = AnimationState;
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

        public void DecideDirection(Vector2 coords)
        {
            Vector2 direction = coords - Position;
            if (Vector2.Dot(direction, Vector2.UnitX) > 0)
            {
                animationState.Direction = "right";
            }
            else
            {
                animationState.Direction = "left";
            }
        }

        public override void Update(float dt)
        {
            Position += Velocity * dt;
            Shape.Update();
            animationState.Update(dt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position - new Vector2(16, 32));
        }
    }
}