using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TowerDefense.Sprite
{
    public class AnimatedSprite
    {
        public Texture2D Texture { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float FrameTime { get; set; }
        public bool Flipped { get; set; }

        private int rows;
        private int columns;

        public int CurrentFrame { get; set; }
        private int totalFrames;
        private float time;

        /// <param name="texture">The texture to use for the sprite</param>
        /// <param name="width">The width of each frame</param>
        /// <param name="height">The height of each frame</param>
        /// <param name="frameTime">The amount of time to display each frame</param>
        /// <param name="flipped">Whether or not to flip the sprite horizontally</param>
        public AnimatedSprite(Texture2D texture, int width, int height, float frameTime, bool flipped = false)
        {
            Texture = texture;
            Width = width;
            Height = height;
            FrameTime = frameTime;
            Flipped = flipped;

            rows = Texture.Height / height;
            columns = Texture.Width / width;

            CurrentFrame = 0;
            totalFrames = rows * columns;
            time = 0;
        }

        public void Reset(int offset=0)
        {
            CurrentFrame = offset;
            time = CurrentFrame * FrameTime;
        }

        public void Update(float dt)
        {
            time += dt;
            CurrentFrame = (int)(time / FrameTime);
            if (CurrentFrame == totalFrames)
            {
                time = 0;
                CurrentFrame = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            int row = CurrentFrame / columns;
            int column = CurrentFrame % columns;
            Rectangle sourceRectangle = new Rectangle(Width * column, Height * row, Width, Height);

            var flip = Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(Texture, position, sourceRectangle, Color.White, 0, Vector2.Zero, 1, flip, 0);
        }
    }
}