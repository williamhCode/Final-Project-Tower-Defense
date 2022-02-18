using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Collision;
using CF = Collision.CollisionFuncs;

namespace TowerDefense
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Circle circle1;
        Polygon poly1;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // resize canvas
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            // init objects
            circle1 = new Circle(100, new Vector2(100, 200));

            var vertices = new Vector2[] 
            {
                new Vector2(-30, -30),
                new Vector2(-30, 30),
                new Vector2(30, 30),
                new Vector2(30, -30),
            };
            poly1 = new Polygon(vertices, new Vector2(200, 300));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            MouseState mouseState = Mouse.GetState();
            circle1.Position = new Vector2(mouseState.X, mouseState.Y);

            // TODO: Add your update logic here

            Vector2 mtv;
            CF.IsColliding(circle1, poly1, out mtv);
            poly1.Position += mtv;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            _spriteBatch.DrawCircle(circle1.Position, circle1.Radius, 20, new Color(0, 0, 0), 2);
            _spriteBatch.DrawPolygon(Vector2.Zero, poly1.Vertices, new Color(0, 0, 0), 2);
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
