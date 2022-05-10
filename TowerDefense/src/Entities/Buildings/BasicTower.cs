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
            Range = 300;
            Damage = 1;
            fireRate = 10f;
            fireTime = fireRate;
            CShape = new CRectangle(position, 32, 32);

            animationState = AnimationState.Copy();
            animationState.SetState("state", "base");
            animationState.Update(0);

            Health = 10;
        }

        public override Projectile Shoot(SpatialHashGrid SHG)
        {
            if (!CanFire())
                return null;

            var enemiesInRange = GetEnemiesInRange(SHG);

            if (enemiesInRange.Count == 0)
                return null;

            // find closest position in enemies in range
            var closestEnemy = GetClosestEnemy(enemiesInRange);

            var path = new StraightPath();
            // var damageType = new AreaDamage(SHG, 50, 150);
            var damageType = new DirectDamage(closestEnemy);
            var projectile = new Projectile(startPosition: Position, targetPosition: closestEnemy.HitboxShape.Position, speed: 2000, damage: Damage, path, damageType, 0.25f);

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
    }
}