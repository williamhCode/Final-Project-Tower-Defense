﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;


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
            AnimationState.SetState(BANDIT_STATE, BanditState.Idle);
            AnimationState.SetState(DIRECTION, Direction.Right);
        }

        public Bandit(Vector2 position, int health) : base(position, health)
        {
            CShape = new CCircle(position, 5);

            animationState = AnimationState;
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