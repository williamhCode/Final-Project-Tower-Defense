using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Components;

using System;
using System.Linq;

namespace TowerDefense.Entities
{
    public class Player : Entity, IFaceable
    {
        public Entity Obj => this;

        private const string PLAYER_STATE = "PlayerState";
        private enum PlayerState
        {
            Idle,
            Walking,
            Attacking,
            Dead
        }

        private const string DIRECTION = IFaceable.DIRECTION;
        private enum Direction
        {
            Left,
            Right
        }

        private const float MAX_SPEED = 200;
        private const float FRICTION = 2000;
        private const float ACCELERATION = 2000;

        public static AnimationState<Enum> AnimationState;
        public AnimationState<Enum> animationState { get; set; }

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Player";

            float frameTime = 0.07f;
            AnimationState = new AnimationState<Enum>(PLAYER_STATE, DIRECTION);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player_idle"), 32, 32, frameTime), PlayerState.Idle, Direction.Right);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player_idle"), 32, 32, frameTime, flipped: true), PlayerState.Idle, Direction.Left);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player_walk"), 32, 32, frameTime, offset: 1), PlayerState.Walking, Direction.Right);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player_walk"), 32, 32, frameTime, offset: 1, flipped: true), PlayerState.Walking, Direction.Left);
        }

        public Player(Vector2 position)
        {
            Position = position;
            Velocity = new Vector2(0, 0);
            // CShape = new CCircle(position, 5);
            CShape = new CRectangle(position, 16, 6);

            animationState = AnimationState.Copy();
            animationState.SetState(PLAYER_STATE, PlayerState.Idle);
            animationState.SetState(DIRECTION, Direction.Right);
        }

        public void Move(Vector2 direction, float dt)
        {
            direction = direction.Normalized();
            if (direction == Vector2.Zero)
            {
                animationState.SetState(PLAYER_STATE, PlayerState.Idle);
                Velocity = Velocity.MoveTowards(Vector2.Zero, FRICTION * dt);
            }
            else
            {
                animationState.SetState(PLAYER_STATE, PlayerState.Walking);
                Velocity = Velocity.MoveTowards(direction * MAX_SPEED, ACCELERATION * dt);
            }
        }

        public void DecideDirection(Vector2 goal) => this._DecideDirection(goal);

        public override void Update(float dt)
        {
            Position += Velocity * dt;
            CShape.Update();
            animationState.Update(dt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position - new Vector2(16, 32));
        }
    }
}