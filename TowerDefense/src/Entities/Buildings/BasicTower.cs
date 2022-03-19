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
            Range = 200;
            Damage = 2;
            fireRate = 5f;
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
            Projectile projectile = null;

            foreach (Enemy enemy in entities)
            {
                if (Vector2.Distance(Position, enemy.Position) < Range)
                {
                    var path = new StraightPath();
                    var damageType = new DirectDamage(enemy);
                    projectile = new Projectile(Position, enemy.Position, speed: 600, damage: 1, path, damageType, 2);
                    break;
                }
            }

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