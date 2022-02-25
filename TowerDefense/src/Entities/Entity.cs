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
        public Collision.Shape Shape { get; set; }

        protected AnimationState animationState;

        public abstract void Update(float dt);
        public abstract void Draw(SpriteBatch spriteBatch);
    }


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
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player"), 32, 32, frameTime), "idle", "up");
            AnimationState.SetState("idle");
            AnimationState.SetDirection("up");
        }

        public Player(Vector2 position)
        {
            Shape = new Circle(position, 10);
            Position = position;
            Velocity = new Vector2(0, 0);

            animationState = AnimationState;
        }

        // private Direction DecideDirection(Vector2 direction)
        // {
        //     float topRange = 80;
        //     float bottomRange = 80;

        //     float[] bounds = new float[] {
        //         0,
        //         90 - topRange / 2,
        //         90 + topRange / 2,
        //         270 - bottomRange / 2,
        //         270 + bottomRange / 2
        //     };

        //     Direction[] directions = new Direction[]{
        //         Direction.Right,
        //         Direction.Up,
        //         Direction.Left,
        //         Direction.Down,
        //         Direction.Right
        //     };
            
        //     float angle = ((MathF.Atan2(direction.Y, direction.X) / MathF.PI * 180) + 360) % 360;
        //     Direction dir = 0;
        //     for (int i = 0; i < 5; i++)
        //     {
        //         if (angle >= bounds[i])
        //             dir = directions[i];
        //         else
        //             break;
        //     }
        //     return dir;
        // }

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

            animationState.Update(dt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Shape.Draw(spriteBatch, new Color(0, 0, 0), 1);

            animationState.GetSprite().Draw(spriteBatch, Position);
        }
    }
}