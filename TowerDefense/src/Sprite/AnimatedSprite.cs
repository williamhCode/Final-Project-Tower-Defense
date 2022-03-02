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

        private int currentFrame;
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

            currentFrame = 0;
            totalFrames = rows * columns;
            time = 0;
        }

        public void Reset()
        {
            time = 0;
            currentFrame = 0;
        }

        public void Update(float dt)
        {
            time += dt;
            currentFrame = (int)(time / FrameTime);
            if (currentFrame == totalFrames)
            {
                time = 0;
                currentFrame = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            int row = currentFrame / columns;
            int column = currentFrame % columns;
            Rectangle sourceRectangle = new Rectangle(Width * column, Height * row, Width, Height);

            var flip = Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(Texture, position, sourceRectangle, Color.White, 0, Vector2.Zero, 1, flip, 0);
        }
    }
}