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
        
        public static Game1 Instance { get; private set; }
        public SpriteFont font;
        private Camera2D camera;
        private Panel root;
        
         Matrix projectionMatrix;
        
        Matrix worldMatrix;
        Vector3 camTarget;
        Vector3 camPosition;
        
        Matrix viewMatrix;
        

        Model model;


        
        private Player player;
        private List<Entity> entities;
        private Wall[] walls;
        private Enemy[] enemies;

        public const int TILE_SIZE = 32;
        public Dictionary<string, Texture2D> tileTextures;
        public string[][] tileMap;
        public static GraphicsDeviceManager graphics;
        

        public Game1()
        {
            Instance = this;
            this.IsMouseVisible = true;
            
            

            camTarget = new Vector3(0f, 0f, 0f);
            camPosition = new Vector3(0f, 0f, -5);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f), 180,
                1f, 1000f);
                viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, 
                         new Vector3(0f, 1f, 0f));// Y up
            worldMatrix = Matrix.CreateWorld(camTarget, Vector3.
                          Forward, Vector3.Up);
                          
        }

        protected override void Initialize()
        {
            base.Initialize();
            

            
           
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(90),
                this.GraphicsDevice.Viewport.AspectRatio,
                1, 10);
            Vector3 translation =new Vector3(1,0,-1);
            Vector3 rotation =new Vector3(0,1,0);
            Vector3 scale =new Vector3(1,1,1);
            worldMatrix = Matrix.CreateWorld(translation,rotation,scale);
            Content.RootDirectory="content/Models";
            Matrix RotationMatrix=Matrix.CreateFromAxisAngle(translation, 20);
            //worldMatrix.Rotate=new Vector3(1,-2,0);
            //model = Content.Load<Model>("cannon");
            
            camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // entities initialization
            player = new Player(new Vector2(300, 300));
            entities = new List<Entity> {
                player,
                // new Bandit(new Vector2(100, 100), 10),
            };
            for (int i = 0; i < 10; i++)
            {
                entities.Add(new Wall(new Vector2(i * 16 + 100, 100)));
                entities.Add(new Wall(new Vector2(100, (i + 1) * 16 + 100)));
            }
            walls = entities.OfType<Wall>().ToArray();
            enemies = entities.OfType<Enemy>().ToArray();
            

            // tile map initialization
            tileMap = new string[20][];
            for (int i = 0; i < tileMap.Length; i++)
            {
                tileMap[i] = new string[20];
            }

            for (int i = 0; i < tileMap.Length; i++)
            {
                for (int j = 0; j < tileMap[i].Length; j++)
                {
                    tileMap[i][j] = "grass";
                }
            }
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
            if (PlatformInfo.MonoGamePlatform == MonoGamePlatform.DesktopGL)
            {
                this.GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
                this.GraphicsDeviceManager.PreferredBackBufferHeight = 720;
                this.GraphicsDeviceManager.ApplyChanges();
            }

            base.LoadContent();

            // load tile textures
            tileTextures = new Dictionary<string, Texture2D>();

            Content.RootDirectory = "Content";
            string[] tileNames = new string[] { "grass", "dirt" };
            foreach (string name in tileNames)
            {
                tileTextures.Add(name, Content.Load<Texture2D>("Sprites/Tiles/" + name));
            }

            // load fonts
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

            // UI initialization
            var style = new UntexturedStyle(this.SpriteBatch)
            {
                Font = new GenericSpriteFont(LoadContent<SpriteFont>("Font/Frame")),
            };
            this.UiSystem.Style = style;
            this.UiSystem.AutoScaleReferenceSize = new Point(1280, 720);
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
                    if (IsColliding(wall.CShape, e.CShape, out Vector2 mtv))
                    {
                        e.Position += mtv;
                        e.CShape.Update();
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

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: camera.GetTransform(), blendState: BlendState.AlphaBlend);

            // draw tilemap
            for (int row = 0; row < tileMap.Length; row++)
            {
                for (int col = 0; col < tileMap[row].Length; col++)
                {
                    var tile = tileMap[row][col];
                    if (tile != null)
                    {
                        SpriteBatch.Draw(tileTextures[tile], new Vector2(TILE_SIZE * row, TILE_SIZE * col), Color.White);
                    }
                }
            }

            // draw entities
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

            GraphicsDevice.Clear(Color.CornflowerBlue);

            /*foreach(ModelMesh mesh in model.Meshes)
            {
                foreach(BasicEffect effect in mesh.Effects)
                {
                    //effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(1f, 0, 0);
                    effect.View = worldMatrix;
                    effect.World = worldMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();
            }*/
            

            base.DoDraw(gameTime);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

    }
}
