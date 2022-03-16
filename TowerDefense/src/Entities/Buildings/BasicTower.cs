using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Entities;
using TowerDefense.Hashing;

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
            AnimationState.SetState("state", "base");
            
        }

        public BasicTower(Vector2 position) : base(position)
        {
            Position = position;
            Velocity = new Vector2(0, 0);
            Range = 100;
            Damage = 2;
            CShape = new CRectangle(position, 32, 32);

            animationState = AnimationState;
        }

        public override void Shoot(float dt, List<Enemy> enemies)
        {
            //throw new NotImplementedException();
        }

        public override void DetectEnemy(float dt, List<Enemy> enemies)
        {
            //throw new NotImplementedException();
            //eventually return all enemies in range
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position - new Vector2(24, 32));
        }

        public override void Update(float dt)
        {
            CShape.Update();
            AnimationState.Update(dt);
        }
    }
}