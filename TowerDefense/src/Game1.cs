using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

using System;
using System.Linq;

using TowerDefense.Camera;
using TowerDefense.Entities;

namespace TowerDefense
{
    public class Game1 : Game
    {
        public const int defaultResolutionX = 1280;
        public const int defaultResolutionY = 720;
        public const int tilesize = 32;
        public static Texture2D playerSpriteSheet;
        public static Texture2D BanditSpriteSheet;
        public static Texture2D objectSpriteSheet;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public static ContentManager content;
        public SpriteFont font;


        Camera2D camera;
        Player player;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            content = Content;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // set canvas size
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            // init objects
            camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            player = new Player(new Vector2(0, 0));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font/Frame");
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = gameTime.GetElapsedSeconds();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.OemPlus))
            {
                camera.Zoom += 0.1f;
            }
            if (state.IsKeyDown(Keys.OemMinus))
            {
                camera.Zoom -= 0.1f;
            }

            var direction = new Vector2(
                Convert.ToSingle(state.IsKeyDown(Keys.D)) - Convert.ToSingle(state.IsKeyDown(Keys.A)),
                Convert.ToSingle(state.IsKeyDown(Keys.W)) - Convert.ToSingle(state.IsKeyDown(Keys.S))
            );
            player.Move(direction, dt);
            player.Update(dt);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            float frameRate = 1/ (float)gameTime.ElapsedGameTime.TotalSeconds;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: camera.getTransform());

            player.Draw(spriteBatch);
            spriteBatch.DrawString(font, "Frame Rate: " + frameRate, new Vector2(10, 10), Color.Black);

            spriteBatch.End();

            base.Draw(gameTime);

        }
    }
}
