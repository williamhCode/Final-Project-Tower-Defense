﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using System;
using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Hashing;

namespace TowerDefense.Entities
{
    public abstract class Enemy : Entity
    {
        protected const string DIRECTION = "Direction";
        protected enum Direction
        {
            Left,
            Right
        }

        protected AnimationState<Enum> animationState;

        public int Health { get; private set; }
        public Boolean IsDead { get; private set; }

        public Enemy(Vector2 position, int health)
        {   
            Position = position;
            Velocity = new Vector2(0, 0);
            IsDead = false;
        }

        public abstract void Move(Vector2 goal, float dt);

        public void DecideDirection(Vector2 goal)
        {
            Vector2 direction = goal - Position;
            if (Vector2.Dot(direction, Vector2.UnitX) > 0)
            {
                animationState.SetState(DIRECTION, Direction.Right);
            }
            else
            {
                animationState.SetState(DIRECTION, Direction.Left);
            }
        }

        public abstract void ApplyFlocking(SpatialHashGrid SHG, float dt);

        public override void Update(float dt)
        {
            Position += Velocity * dt;
            CShape.Update();
            animationState.Update(dt);
        }
        
        public void Damage()
        {
            Health--;

            if (Health <= 0)
            {
                IsDead = true;

            }
        }

    }
}
