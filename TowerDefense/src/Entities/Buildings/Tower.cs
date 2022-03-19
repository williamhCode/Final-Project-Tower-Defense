using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Hashing;
using TowerDefense.Projectiles;

using System;
using System.Collections.Generic;


namespace TowerDefense.Entities.Buildings
{
    public abstract class Tower : Building
    {
        public int Range { get; set; }
        public int Damage { get; set; }

        protected float fireRate;
        protected float fireTime;

        public Tower(Vector2 position) : base(position)
        {
            Position = position;
            Velocity = new Vector2(0, 0);
        }

        public bool CanFire(float dt)
        {
            fireTime += dt;
            if (fireTime >= 1 / fireRate)
            {
                fireTime = 0;
                return true;
            }
            return false;
        }

        public abstract Projectile Shoot(float dt, SpatialHashGrid SHG);
    }
}