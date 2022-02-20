using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

using System;
using System.Linq;

using Collision;
using CF = Collision.CollisionFuncs;

namespace TowerDefense
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        Circle circle1, circle2;
        Polygon poly1, poly2;

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
            poly1 = new Collision.Rectangle(new Vector2(400, 300), 80, 80);
            poly2 = new Collision.Rectangle(new Vector2(200, 300), 80, 80, rotation:30);
            // circle1 = new Circle(new Vector2(200, 200), 50);
            // circle2 = new Circle(new Vector2(400, 300), 50);

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
            poly1.Position = new Vector2(mouseState.X, mouseState.Y);
            poly1.UpdateVertices();

            // TODO: Add your update logic here
            Vector2 mtv;
            if (CF.IsColliding(poly1, poly2, out mtv))
            {
                poly2.Position += mtv;
                poly2.UpdateVertices();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            // _spriteBatch.DrawCircle(circle1.Position, circle1.Radius, 20, new Color(0, 0, 0), 2);
            // _spriteBatch.DrawCircle(circle2.Position, circle2.Radius, 20, new Color(0, 0, 0), 2);
            _spriteBatch.DrawPolygon(Vector2.Zero, poly1.Vertices, new Color(0, 0, 0), 2);
            _spriteBatch.DrawPolygon(Vector2.Zero, poly2.Vertices, new Color(0, 0, 0), 2);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
