using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

using System;
using System.Linq;

using TowerDefense.Entities;
using TowerDefense.Math;
using TowerDefense.Sprite;

using Collision;
using CF = Collision.CollisionFuncs;

namespace TowerDefense
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player player;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // set canvas size
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            // init objects
            player = new Player(new Vector2(500, 500));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = gameTime.GetElapsedSeconds();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState state = Keyboard.GetState();
            
            var direction = new Vector2(
                Convert.ToSingle(state.IsKeyDown(Keys.Right)) - Convert.ToSingle(state.IsKeyDown(Keys.Left)),
                Convert.ToSingle(state.IsKeyDown(Keys.Down)) - Convert.ToSingle(state.IsKeyDown(Keys.Up))
            );
            player.Move(direction, dt);
            player.Update(dt);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            player.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
