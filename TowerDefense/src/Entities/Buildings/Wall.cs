using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;

using System;

namespace TowerDefense.Entities.Buildings
{
    public class Wall : Building
    {
        
        public static AnimationState<string> AnimationState;
        private AnimationState<string> animationState;

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Walls";

            float frameTime = 0f;
            AnimationState = new AnimationState<string>("state");
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("wall_2"), 16, 24, frameTime), "full");
            AnimationState.SetState("state", "full");
        }

        public Wall(Vector2 position) : base(position)
        {
            CShape = new Collision.CRectangle(position, 16f, 16f);
            animationState = AnimationState.Copy();
            animationState.Update(0);
        }

        public override void Update(float dt)
        {
            // do nothing
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position - new Vector2(8, 16));
        }
    }
}