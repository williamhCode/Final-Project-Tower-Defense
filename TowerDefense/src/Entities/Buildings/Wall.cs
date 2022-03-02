using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;

using System;

namespace TowerDefense.Entities
{
    public class Wall : Building
    {
        public static AnimationState AnimationState;

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Walls";

            float frameTime = 0f;
            AnimationState = new AnimationState();
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("wall_1"), 32, 50, frameTime), "full", "none");
            AnimationState.State = "full";
            AnimationState.Direction = "none";
        }

        public Wall(Vector2 position) : base(position)
        {
            animationState = AnimationState;
            animationState.Update(0);
        }

        public override void Update(float dt)
        {
            // do nothing
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position - new Vector2(16, 34));
        }
    }
}