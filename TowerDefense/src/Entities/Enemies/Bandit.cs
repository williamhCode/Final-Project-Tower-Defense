﻿using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Hashing;
using System.Threading.Tasks;


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

        private const float MAX_SPEED = 30;
        private const float FRICTION = 1000;
        private const float ACCELERATION = 300;


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
            CShape = new CRectangle(position, 16, 6);

            animationState = AnimationState.Copy();
            animationState.SetState(BANDIT_STATE, BanditState.Idle);
            animationState.SetState(DIRECTION, Direction.Right);

            HitboxShape = new CRectangle(position, 20, 32);
            YHitboxOffset = 14;
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
        private const float SEPARATION_FACTOR = 3.2f;
        private const float SEPARATION_SENSTIVITY = 100;


        public override void ApplyFlocking(float dt, SpatialHashGrid SHG, Vector2 goal)
        {
            var entitiesToCheck = SHG.QueryEntities(Position, SEPARATION_DIST);
            entitiesToCheck.Remove(this);

            var cohesion = Vector2.Zero;
            var alignment = Vector2.Zero;
            var separation = Vector2.Zero;

            Parallel.ForEach(entitiesToCheck, e =>
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
            });
            
            Vector2 direction = (goal - Position).Normalized();
            DecideDirection(goal);

            var force = 
            cohesion * COHESION_FACTOR +
            alignment * ALIGNMENT_FACTOR +
            separation * SEPARATION_FACTOR +
            direction * 0.2f;

            Velocity = Velocity.MoveTowards(MAX_SPEED * force.Normalized(), ACCELERATION * dt);

            // randomize velocity if its undefined
            if (!float.IsFinite(Velocity.X))
            {
                var rand = new Random();
                var angle = (float)rand.NextDouble() * MathF.PI * 2;
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * MAX_SPEED;
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position, new Vector2(16, 32));
        }
    }
}