using Microsoft.Xna.Framework;

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

        public bool CanFire()
        {
            if (fireTime >= 1 / fireRate)
            {
                fireTime = 0;
                return true;
            }
            return false;
        }

        public List<Enemy> GetEnemiesInRange(SpatialHashGrid SHG)
        {
            var entities = SHG.QueryEntities(Position, Range);

            var enemiesInRange = new List<Enemy>();
            foreach (Enemy enemy in entities)
            {
                if (Vector2.Distance(Position, enemy.Position) < Range)
                {
                    enemiesInRange.Add(enemy);
                }
            }

            return enemiesInRange;
        }

        public Enemy GetClosestEnemy(List<Enemy> enemiesInRange)
        {
            Enemy closestEnemy = null;
            float closestDistance = float.MaxValue;
            foreach (Enemy enemy in enemiesInRange)
            {
                float distance = Vector2.Distance(Position, enemy.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }

        public abstract Projectile Shoot(SpatialHashGrid SHG);

        public override void Update(float dt)
        {
            fireTime += dt;
        }
    }
}