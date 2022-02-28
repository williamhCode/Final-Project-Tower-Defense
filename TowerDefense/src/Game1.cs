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
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Camera2D camera;
        Player player;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // set canvas size
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            // init objects
            camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            player = new Player(new Vector2(0, 0));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Player.LoadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
            camera.Update();

            var direction = new Vector2(
                Convert.ToSingle(state.IsKeyDown(Keys.D)) - Convert.ToSingle(state.IsKeyDown(Keys.A)),
                Convert.ToSingle(state.IsKeyDown(Keys.W)) - Convert.ToSingle(state.IsKeyDown(Keys.S))
            );
            player.Move(direction, dt);
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            player.DecideDirection(camera.MouseToScreen(mousePosition));
            player.Update(dt);

        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: camera.Transform);

            player.Draw(_spriteBatch);

            _spriteBatch.End();
        }
    }
}
