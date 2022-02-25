using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
 
namespace TowerDefense.Sprite
{
    public class AnimatedSprite
    {
        public Texture2D Texture { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public float FrameTime { get; set; }

        private int currentFrame;
        private int totalFrames;

        private float time;
 
        public AnimatedSprite(Texture2D texture, int rows, int columns, float frameTime)
        {
            Texture = texture;
            Rows = rows;
            Columns = columns;
            FrameTime = frameTime;

            currentFrame = 0;
            totalFrames = Rows * Columns;
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
            int width = Texture.Width / Columns;
            int height = Texture.Height / Rows;
            int row = currentFrame / Columns;
            int column = currentFrame % Columns;
            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            spriteBatch.Draw(Texture, position - new Vector2(width / 2, 0), sourceRectangle, Color.White, 0, Vector2.Zero, 1, SpriteEffects.FlipVertically, 0);
        }
    }
}