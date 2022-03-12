using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using MLEM.Startup;
using MLEM.Font;
using MonoGame.Framework.Utilities;
using MonoGame.Extended;

using System.Reflection;

using TowerDefense.Camera;
using TowerDefense.Entities;
using TowerDefense.Entities.Enemies;
using TowerDefense.Entities.Buildings;
using static TowerDefense.Collision.CollisionFuncs;

using System.Diagnostics;

namespace TowerDefense
{
    public class Game1 : MlemGame
    {
        // variables
        public static Game1 Instance {get; private set;}
        public SpriteFont font;
        private Camera2D camera;
        private Panel root;

        private Player player;
        private List<Entity> entities;
        private Wall[] walls;
        private Enemy[] enemies;

        private double totalTime = 0.0f;
        private int count = 0;
        int c = 0;

        public Game1()
        {
            Instance = this;
            this.IsMouseVisible = true;   
        }

        protected override void Initialize()
        {
            base.Initialize();
            camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // entities initialization
            player = new Player(new Vector2(300, 300));
            entities = new List<Entity> {
                player,
                new Bandit(new Vector2(100, 100), 10),
            };
            for (int i = 0; i < 10; i++)
            {
                entities.Add(new Wall(new Vector2(i * 16 + 100, 100)));
                entities.Add(new Wall(new Vector2(100, (i+1) * 16 + 100)));
            }
            walls = entities.OfType<Wall>().ToArray();
            enemies = entities.OfType<Enemy>().ToArray();
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
            if(PlatformInfo.MonoGamePlatform == MonoGamePlatform.DesktopGL)
            {
                this.GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
                this.GraphicsDeviceManager.PreferredBackBufferHeight = 720;
                this.GraphicsDeviceManager.ApplyChanges();
            }

            base.LoadContent();

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

            var style = new UntexturedStyle(this.SpriteBatch)
            {
                Font = new GenericSpriteFont(LoadContent<SpriteFont>("Font/Frame")),
            };
            this.UiSystem.Style = style;
            this.UiSystem.AutoScaleReferenceSize = new Point(1280,720);
            this.UiSystem.AutoScaleWithScreen = true;
            this.UiSystem.GlobalScale = 5;

            /*
            this.root = new Panel(Anchor.TopLeft, new Vector2(100,100), Vector2.Zero, false, true);
            this.root.ScrollBar.SmoothScrolling = true;
            this.UiSystem.Add("TestUi", this.root);
            float timesPressed = 0f;
            var box = new Panel(Anchor.Center, new Vector2(100,1), Vector2.Zero, setHeightBasedOnChildren: true);
            var bar1 = box.AddChild(new ProgressBar(Anchor.Center, new Vector2(100,10), MLEM.Misc.Direction2.Right, 100f, timesPressed));
            box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 20), "Okay") 
            {
                OnPressed = close => 
                {
                    this.UiSystem.Remove("TestUi");
                    this.UiSystem.Remove("InfoBox");
                },  
                OnPressed = increase => timesPressed += 1f,
                PositionOffset = new Vector2(0, 1)
            });
            this.UiSystem.Add("InfoBox", box);
            */
        }
        
        protected override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            float dt = gameTime.GetElapsedSeconds();

            // game inputs
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState state = Keyboard.GetState();
            
            // player movement
            var direction = new Vector2(
                Convert.ToSingle(state.IsKeyDown(Keys.D)) - Convert.ToSingle(state.IsKeyDown(Keys.A)),
                Convert.ToSingle(state.IsKeyDown(Keys.S)) - Convert.ToSingle(state.IsKeyDown(Keys.W))
            );
            player.Move(direction, dt);
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            player.DecideDirection(camera.ScreenToWorld(mousePosition));

            // enemy movement
            foreach (var e in enemies)
            {
                e.Move(player.Position, dt);
            }

            // updates
            foreach (var e in entities)
            {
                e.Update(dt);
            }

            // collision detection and resolution
            // create new list from Player and Enemy entities
            var entitiesToCheck = entities.Where(e => 
            {
                if (e is Player || e is Enemy)
                    return true;
                return false;
            }
            ).ToArray();

            foreach (var e in entitiesToCheck)
            {
                var temp_walls = walls.OrderBy(w => (w.Position - e.Position).LengthSquared()).ToArray();

                foreach (var wall in temp_walls)
                {
                    if (IsColliding(wall.Shape, e.Shape, out Vector2 mtv))
                    {
                        e.Position += mtv;
                        e.Shape.Update();
                    }
                }
            }

            // camera
            if (state.IsKeyDown(Keys.OemPlus))
            {
                camera.Zoom *= 1.1f;
            }
            if (state.IsKeyDown(Keys.OemMinus))
            {
                camera.Zoom /= 1.1f;
            }
            camera.LookAt(player.Position);
        }

        protected override void DoDraw(GameTime gameTime)
        {
            float frameRate = 1 / gameTime.GetElapsedSeconds();
            
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Drawing the player
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: camera.GetTransform(), blendState: BlendState.AlphaBlend);

            var entities_temp = entities.OrderBy(e => e.Position.Y).ToArray();
            foreach (var entity in entities_temp)
            {
                entity.DrawDebug(SpriteBatch);
                entity.Draw(SpriteBatch);
            }

            SpriteBatch.End();

            // Drawing the Text
            SpriteBatch.Begin();
            SpriteBatch.DrawString(font, $"Frame Rate: {frameRate:N2}", new Vector2(10, 10), Color.Black);
            SpriteBatch.End();
            
            base.DoDraw(gameTime);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

    }
}
