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

        private int rows;
        private int columns;

        private int currentFrame;
        private int totalFrames;
        private float time;
 
        public AnimatedSprite(Texture2D texture, int width, int height, float frameTime)
        {
            Texture = texture;
            Width = width;
            Height = height;
            FrameTime = frameTime;

            rows = Texture.Height / height;
            columns = Texture.Width / width;

            currentFrame = 0;
            totalFrames = rows * columns;
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
            spriteBatch.Draw(Texture, position - new Vector2(Width / 2, 0), sourceRectangle, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);
        }
    }
}