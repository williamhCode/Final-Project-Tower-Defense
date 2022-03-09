using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TowerDefense.Camera;
using TowerDefense.Entities;
using TowerDefense.Entities.Buildings;
using static TowerDefense.Collision.CollisionFuncs;

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
        private SpriteBatch _spriteBatch;
        public static ContentManager content;
        public SpriteFont font;

        Camera2D camera;
        Player player;
        Wall[] walls;
        List<Entity> entities;
        
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            // set canvas size
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            // init objects
            camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            player = new Player(new Vector2(300, 300));
            entities = new List<Entity> {
                player,
            };
            // add walls to entities
            for (int i = 0; i < 10; i++)
            {
                entities.Add(new Wall(new Vector2(i * 16 + 100, 100)));
                entities.Add(new Wall(new Vector2(100, (i+1) * 16 + 100)));
            }

            walls = entities.OfType<Wall>().ToArray();
        }
        
        /// <summary>
        /// Gets all Types in the given namespace including sub-namespaces.
        /// </summary>
        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return 
            assembly.GetTypes()
                    .Where(t => t.Namespace.Contains(nameSpace, StringComparison.Ordinal))
                    .ToArray();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font/Frame");

            // loads all content by invoking the LoadContent method of each class in Entities
            var classes = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "TowerDefense.Entities");

            foreach (var c in classes)
            {
                var loadContent = c.GetMethod("LoadContent");
                if (loadContent != null)
                {
                    loadContent.Invoke(null, new object[] { Content });
                }
            }
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
                Convert.ToSingle(state.IsKeyDown(Keys.S)) - Convert.ToSingle(state.IsKeyDown(Keys.W))
            );
            player.Move(direction, dt);
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            player.DecideDirection(camera.MouseToScreen(mousePosition));
            player.Update(dt);

            var temp_walls = walls.OrderBy(w => (w.Position - player.Position).LengthSquared()).ToArray();

            foreach (var wall in temp_walls)
            {
                if (IsColliding(wall.Shape, player.Shape, out Vector2 mtv))
                {
                    player.Position += mtv;
                    player.Shape.Update();
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            float frameRate = 1 / gameTime.GetElapsedSeconds();
            
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: camera.Transform);
            
            var entities_temp = entities.OrderBy(e => e.Position.Y).ToArray();
            foreach (var entity in entities_temp)
            {
                entity.DrawDebug(_spriteBatch);
                entity.Draw(_spriteBatch);
            }

            _spriteBatch.End();


            _spriteBatch.Begin();

            _spriteBatch.DrawString(font, $"Frame Rate: {frameRate:N2}", new Vector2(10, 10), Color.Black);

            _spriteBatch.End();
        }
    }
}
