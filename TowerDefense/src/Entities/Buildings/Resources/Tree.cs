using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Entities.Components;

using System;

namespace TowerDefense.Entities.Buildings.Resources
{
    public class Tree : Resource
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

            var vertices = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(64, 0),
                new Vector2(64, 40),
                new Vector2(43, 40),
                new Vector2(43, 64),
                new Vector2(22, 64),
                new Vector2(22, 40),
                new Vector2(0, 40),
            };
            HitboxShape = new CPolygon(position, CPolygon.OrderCounterClockwise(vertices));
            HitboxOffset = new Vector2(32, 60);
            UpdateHitbox();
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