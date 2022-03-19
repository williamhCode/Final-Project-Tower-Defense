using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using System;
using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Hashing;
using Towerdefense.Entities.Components;


namespace TowerDefense.Entities
{
    public abstract class Enemy : Entity, IFaceable
    {
        public Entity Obj => this;

        protected const string DIRECTION = IFaceable.DIRECTION;
        protected enum Direction
        {
            Left,
            Right
        }

        public AnimationState<Enum> animationState { get; set; }

        public int Health { get; set; }
        public Boolean IsDead { get; private set; }

        public Enemy(Vector2 position, int health)
        {   
            Position = position;
            Velocity = new Vector2(0, 0);
            IsDead = false;
        }

        public abstract void Move(Vector2 goal, float dt);

        public void DecideDirection(Vector2 goal) => this._DecideDirection(goal);

        public abstract void ApplyFlocking(float dt, SpatialHashGrid SHG, Vector2 goal);

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
