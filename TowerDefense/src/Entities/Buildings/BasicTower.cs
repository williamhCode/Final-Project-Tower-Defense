using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Entities;
using TowerDefense.Hashing;
using TowerDefense.Projectiles;

using System;
using System.Collections.Generic;


namespace TowerDefense.Entities.Buildings
{
    public class BasicTower : Tower
    {
        public static AnimationState<string> AnimationState;
        private AnimationState<string> animationState;

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Towers";

            float frameTime = 0f;
            AnimationState = new AnimationState<string>("state");
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("BasicTower"), 48, 48, frameTime), "base");
        }

        public BasicTower(Vector2 position) : base(position)
        {
            Range = 500;
            Damage = 1;
            fireRate = 1f;
            fireTime = fireRate;
            CShape = new CRectangle(position, 32, 32);

            animationState = AnimationState.Copy();
            animationState.SetState("state", "base");
            animationState.Update(0);
        }

        public override Projectile Shoot(float dt, SpatialHashGrid SHG)
        {
            if (!CanFire(dt))
                return null;

            var entities = SHG.QueryEntitiesRange(Position, Range);

            var enemiesInRange = new List<Enemy>();

            foreach (Enemy enemy in entities)
            {
                if (Vector2.Distance(Position, enemy.Position) < Range)
                {
                    enemiesInRange.Add(enemy);
                }
            }

            if (enemiesInRange.Count == 0)
                return null;

            // find closest position in enemies in range
            Vector2 closestPosition = Vector2.Zero;
            float closestDistance = float.MaxValue;
            foreach (Enemy enemy in enemiesInRange)
            {
                float distance = Vector2.DistanceSquared(Position, enemy.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPosition = enemy.Position;
                }
            }

            var path = new StraightPath();
            var damageType = new AreaDamage(SHG, 50, 10000);
            var projectile = new Projectile(startPosition: Position, targetPosition: closestPosition, speed: 1000, damage: Damage, path, damageType, 0.25f);

            return projectile;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position, new Vector2(24, 38));
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            base.DrawDebug(spriteBatch);
            spriteBatch.DrawCircle(Position, Range, 20, Color.Black);
        }

        public override void Update(float dt)
        {
            CShape.Update();
        }
    }
}