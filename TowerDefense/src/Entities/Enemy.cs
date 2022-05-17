using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using System;
using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Hashing;
using TowerDefense.Entities.Components;


namespace TowerDefense.Entities
{
    public abstract class Enemy : Entity, IFaceableComponent, IHitboxComponent
    {
        public Entity Obj => this;

        protected const string DIRECTION = IFaceableComponent.DIRECTION;
        protected enum Direction
        {
            Left,
            Right
        }

        public AnimationState<Enum> animationState { get; set; }

        public int Health { get; set; }
        public Boolean IsDead => Health <= 0;

        public CShape HitboxShape { get; set; }
        public Vector2 HitboxOffset { get; set; }

        public Enemy(Vector2 position, int health)
        {   
            Position = position;
            Velocity = new Vector2(0, 0);
            Health = health;
        }

        public abstract void Move(Vector2 goal, float dt);

        public void UpdateHitbox() => this._UpdateHitbox();

        public void DecideDirection(Vector2 goal) => this._DecideDirection(goal);

        public abstract void ApplyFlocking(float dt, SpatialHashGrid SHGFlocking, SpatialHashGrid SHGBuildings, Vector2 goal);

        public abstract void Steer(float dt, SpatialHashGrid SHGBuildings, Vector2 goal);

        public override void Update(float dt)
        {
            Position += Velocity * dt;
            CShape.Update();
            UpdateHitbox();
            animationState.Update(dt);
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            base.DrawDebug(spriteBatch);
            HitboxShape.Draw(spriteBatch, Color.Blue, 1);
        }
    }
}
