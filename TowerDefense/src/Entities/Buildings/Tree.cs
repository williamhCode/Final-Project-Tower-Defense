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
    public class Tree : Building
    {
        public static AnimationState<Enum> AnimationState;
        private AnimationState<Enum> animationState;

        private enum TreeType
        {
            Tree = 1
        }

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Resources";

            float frameTime = 0f;
            AnimationState = new AnimationState<Enum>("state");

            foreach (TreeType treeType in Enum.GetValues(typeof(TreeType)))
            {
                AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>($"tree{(int)treeType}"), 64, 64, frameTime), treeType);
            }
        }

        public Tree(Vector2 position) : base(position)
        {
            CShape = new CRectangle(position, 16, 16);
            animationState = AnimationState.Copy();
            
            Array values = Enum.GetValues(typeof(TreeType));
            Random random = new Random();
            TreeType state = (TreeType)values.GetValue(random.Next(values.Length));
            animationState.SetState("state", state);
            animationState.Update(0);

            Health = 10;
        }

        public override void Update(float dt)
        {
            // do nothing
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position, new Vector2(32, 60));
        }
    }
}