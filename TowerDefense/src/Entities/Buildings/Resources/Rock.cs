using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;

using System;

namespace TowerDefense.Entities.Buildings.Resources
{
    public class Rock : Resource
    {
        public static AnimationState<Enum> AnimationState;
        private AnimationState<Enum> animationState;

        private enum RockType
        {
            Rock = 1,
            BigRock,
            FaceRock,
            ThinkRock,
            Boulder
        }

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Resources";

            float frameTime = 0f;
            AnimationState = new AnimationState<Enum>("state");

            foreach(RockType rockType in Enum.GetValues(typeof(RockType)))
            {
                AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>($"rock{(int)rockType}"), 32, 32, frameTime), rockType);
            }
        }

        public Rock(Vector2 position) : base(position)
        {
            CShape = new CRectangle(position, 16, 16);
            animationState = AnimationState.Copy();

            Array values = Enum.GetValues(typeof(RockType));
            Random random = new Random();
            RockType state = (RockType)values.GetValue(random.Next(values.Length));
            animationState.SetState("state", state);
            animationState.Update(0);

            Health = 10;
            HitboxShape = new CRectangle(position, 32, 32);
            HitboxOffset = new Vector2(0, 8);
            UpdateHitbox();
        }

        public override void Update(float dt)
        {
            // do nothing
        }
 
        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position, new Vector2(16, 24));
        }
    }
}