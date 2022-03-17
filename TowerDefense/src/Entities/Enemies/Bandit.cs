using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Hashing;


namespace TowerDefense.Entities.Enemies
{
    public class Bandit : Enemy
    {
        private const string BANDIT_STATE = "PlayerState";
        private enum BanditState
        {
            Idle,
            Walking,
            Attacking,
            Dead
        }

        private const float MAX_SPEED = 100;
        private const float FRICTION = 1200;
        private const float ACCELERATION = 1200;


        public static AnimationState<Enum> AnimationState;

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Bandit";

            float frameTime = 0.05f;
            AnimationState = new AnimationState<Enum>(BANDIT_STATE, DIRECTION);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("bandit"), 32, 32, frameTime), BanditState.Idle, Direction.Right);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("bandit"), 32, 32, frameTime, flipped: true), BanditState.Idle, Direction.Left);
        }

        public Bandit(Vector2 position, int health) : base(position, health)
        {
            // CShape = new CCircle(position, 5);
            CShape = new CRectangle(position, 18, 7);

            animationState = AnimationState.Copy();
            animationState.SetState(BANDIT_STATE, BanditState.Idle);
            animationState.SetState(DIRECTION, Direction.Right);
        }

        public override void Move(Vector2 goal, float dt)
        {
            Vector2 direction = (goal - Position).Normalized();
            if ((goal - Position).Length() < 10)
            {
                Velocity = Velocity.MoveTowards(Vector2.Zero, FRICTION * dt);
            }
            else
            {
                Velocity = Velocity.MoveTowards(direction * MAX_SPEED, ACCELERATION * dt);
            }
            DecideDirection(goal);
        }

        private const float COHESION_DIST = 60;
        private const float COHESION_FACTOR = 10f;
        private const float COHESION_SENSTIVITY = 0.01f;

        private const float ALIGNMENT_DIST = 60;
        private const float ALIGNMENT_FACTOR = 0.2f;
        private const float ALIGNMENT_SENSTIVITY = 0.3f;

        private const float SEPARATION_DIST = 60;
        private const float SEPARATION_FACTOR = 3f;
        private const float SEPARATION_SENSTIVITY = 100;


        public override void ApplyFlocking(SpatialHashGrid SHG, Vector2 goal, float dt)
        {
            var entitiesToCheck = SHG.QueryEntitiesRange(Position, SEPARATION_DIST);
            entitiesToCheck.Remove(this);

            var cohesion = Vector2.Zero;
            var alignment = Vector2.Zero;
            var separation = Vector2.Zero;

            foreach (var e in entitiesToCheck)
            {
                var sqdist = Vector2.DistanceSquared(Position, e.Position);
                var dist = MathF.Sqrt(sqdist);

                if (sqdist < MathF.Pow(COHESION_DIST, 2))
                {
                    cohesion += (e.Position - Position) / 
                    (dist / COHESION_SENSTIVITY + sqdist);
                }
                if (sqdist < MathF.Pow(ALIGNMENT_DIST, 2))
                {
                    alignment += (e.Velocity - Velocity).Normalized() /
                    (1 / ALIGNMENT_SENSTIVITY + dist);
                }
                if (sqdist < MathF.Pow(SEPARATION_DIST, 2))
                {
                    separation += (Position - e.Position) / 
                    (dist / SEPARATION_SENSTIVITY + sqdist);
                }
            }
            
            Vector2 direction = (goal - Position).Normalized();
            DecideDirection(goal);

            var force = 
            cohesion * COHESION_FACTOR +
            alignment * ALIGNMENT_FACTOR +
            separation * SEPARATION_FACTOR +
            direction * 0.15f;

            Velocity += force * 1000 * dt;

            if (Velocity.Length() > MAX_SPEED)
            {
                Velocity = Velocity.Normalized() * MAX_SPEED;
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position - new Vector2(16, 32));
        }
    }
}