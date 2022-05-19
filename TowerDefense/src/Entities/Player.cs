using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Entities.Components;
using static TowerDefense.Entities.Components.IFaceableComponent;

using System;
using System.Linq;

namespace TowerDefense.Entities
{
    public class Player : Entity, IFaceableComponent, IHitboxComponent
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

        private const string DIRECTION = IFaceableComponent.DIRECTION;
        // private enum Direction
        // {
        //     Left,
        //     Right
        // }

        private const float MAX_SPEED = 200;
        private const float FRICTION = 2000;
        private const float ACCELERATION = 2000;

        public static AnimationState<Enum> AnimationState;
        public AnimationState<Enum> animationState { get; set; }

        public CShape HitboxShape { get; set; }
        public Vector2 HitboxOffset { get; set; }


        public static AnimatedSprite Axe;
        private AnimatedSprite axe;

        private Vector2 axeDir;
        private Vector2 axePos;
        private Vector2 axeOffset;
        private float axeAngle;
        private bool axeReverse;
        private float hitAngle;

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Player";

            float frameTime = 0.07f;
            AnimationState = new AnimationState<Enum>(PLAYER_STATE, DIRECTION);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player_idle"), 32, 32, frameTime), PlayerState.Idle, Direction.Right);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player_idle"), 32, 32, frameTime, flipped: true), PlayerState.Idle, Direction.Left);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player_walk"), 32, 32, frameTime, offset: 1), PlayerState.Walking, Direction.Right);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("player_walk"), 32, 32, frameTime, offset: 1, flipped: true), PlayerState.Walking, Direction.Left);

            Axe = new AnimatedSprite(content.Load<Texture2D>("player_arm"), 32, 32, 0);
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

            HitboxShape = new CRectangle(position, 20, 32);
            HitboxOffset = new Vector2(0, 14);

            axe = Axe;
            axeDir = Vector2.Zero;
            axePos = Vector2.Zero;
            axeOffset = Vector2.Zero;
            hitAngle = 0;
            axeReverse = false;
        }

        public void Move(float dt, Vector2 direction)
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

        public void SetAxeDir(Vector2 mousePos)
        {
            axeDir = mousePos - (Position + new Vector2(0, -24));
            if (axeDir.LengthSquared() == 0)
                axeDir = new Vector2(0, -1);
            else
                axeDir = axeDir.Normalized();
        }

        private void update_axe()
        {
            axeReverse = animationState.GetState(DIRECTION).Equals(Direction.Left);
            axePos = Position + new Vector2(axeReverse ? -2 : 2, -13);
            axeOffset = new Vector2(axeReverse ? 32 - 8 : 8, 22);

            float dir = axeDir.Dot(new Vector2(1, 0));
            axeAngle = dir > 0 ? axeDir.ToAngle() - MathF.PI/2 - hitAngle : axeDir.ToAngle() + MathF.PI/2 + hitAngle;
        }

        public void UpdateHitbox() => this._UpdateHitbox();

        public void DecideDirection(Vector2 goal) => this._DecideDirection(goal);

        public override void Update(float dt)
        {
            // axeAngle += dt * 1;
            Position += Velocity * dt;
            CShape.Update();
            UpdateHitbox();
            animationState.Update(dt);

            update_axe();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            axe.Draw(spriteBatch, axePos, axeOffset, axeAngle, axeReverse);
            animationState.Sprite.Draw(spriteBatch, Position, new Vector2(16, 32));
            // spriteBatch.DrawPoint(axePos, Color.Red, 1);
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            base.DrawDebug(spriteBatch);
            HitboxShape.Draw(spriteBatch, Color.Blue, 1);
        }
    }
}