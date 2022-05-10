using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
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
using static TowerDefense.Extensions.ExtensionMethods;

using System.Threading.Tasks;

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

        Matrix viewMatrix;

        Vector2 mouseDefaultPos = new Vector2(620, 360);


        Model model;
        Texture2D testtex;

        Ortho_Camera camera3D;
        private RenderTarget2D modelBase;
        private Viewport modelview;
        public float scale=0.4444f;
        private Player player;
        private List<Entity> entities;
        private Wall[] walls;
        private Enemy[] enemies;

        public const int TILE_SIZE = 32;
        public Dictionary<string, Texture2D> tileTextures;
        public string[][] tileMap;
        public static GraphicsDeviceManager graphics;

        private const int TILE_SIZE = 32;
        private const int MAP_SIZE = 50;
        private Dictionary<string, Texture2D> tileTextures;
        private string[][] tileMap;

        private MouseStateExtended mouseState;
        private KeyboardStateExtended keyboardState;
        private bool debug;

        public enum Selector
        {
            Wall,
            BasicTower,
            Remove,
            Bandit,
            Rock,
            Tree
        }

        private Selector currentSelector;

        public Game1()
        {
            Instance = this;
            this.IsMouseVisible = true;


        }

        protected override void Initialize()
        {
            base.Initialize();
            graphics=this.GraphicsDeviceManager;



            // create camera
            camera3D = new Ortho_Camera(new Vector3(0, 0, 2), 32, 18);
            
            Mouse.SetPosition((int)mouseDefaultPos.X, (int)mouseDefaultPos.Y);
            Content.RootDirectory = "Content/Models";
            model = Content.Load<Model>("ballista");
            Content.RootDirectory= "Content/Textures";
            testtex=Content.Load<Texture2D>("BTTexture");
            //modelview=
            //graphics.IsFullScreen=true;
            //graphics.PreferredBackBufferHeight=128;
            //graphics.PreferredBackBufferHeight=72;
            
            //graphics.ApplyChanges();

            camera = new Camera2D(this.GraphicsDeviceManager.PreferredBackBufferHeight, this.GraphicsDeviceManager.PreferredBackBufferWidth);

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

            // Implementing Perlin Noise and Biome generation into the tilemap array
            Noise NoiseMap = new TowerDefense.NoiseTest.Noise();
            float[] noiseMap = NoiseMap.GenerateNoiseMap(
                MAP_SIZE, MAP_SIZE,
                seed: 1,
                scale: 15f,
                octaves: 3,
                persistance: 1f,
                lacunarity: 1f,
                offset: Vector2.Zero
            );

            for (int i = 0; i < tileMap.Length; i++)
            {
                for (int j = 0; j < tileMap[i].Length; j++)
                {
                    float height = noiseMap[i * MAP_SIZE + j];
                    if (height <= 0.1f)
                    {
                        tileMap[i][j] = "deepwater";
                    }
                    else if (height <= 0.3f)
                    {
                        tileMap[i][j] = "water";
                    }
                    else if (height <= 0.35f)
                    {
                        tileMap[i][j] = "beach";
                    }
                    else if (height < 0.8f)
                    {
                        tileMap[i][j] = "grass";
                    }
                    else
                    {
                        tileMap[i][j] = "sand";
                    }
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
            .Where(t =>
            {
                var ns = t.Namespace;
                return ns == null ? false : ns.Contains(nameSpace, StringComparison.Ordinal);
            })
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
            string[] tileNames = new string[] { "grass"};
            foreach (string name in tileNames)
            {
                tileTextures.Add(name, Content.Load<Texture2D>("Sprites/Tiles/" + name));
            }

            Content.RootDirectory = "Content";
            
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

            
            //render target for 3d models
            modelBase=new RenderTarget2D(GraphicsDevice,198,108,false,SurfaceFormat.Alpha8,DepthFormat.Depth16);
            this.root = new Panel(Anchor.Center, new Vector2(800, 100), new Vector2(0, 300), false, true);
            this.root.ScrollBar.SmoothScrolling = true;
            root.AddChild(new VerticalSpace(2));
            this.UiSystem.Add("TestUi", this.root);

            var button1 = root.AddChild(new Button(Anchor.AutoLeft, new Vector2(80, 80), "Wall")
            {
                OnPressed = element =>
                {
                    currentSelector = Selector.Wall;
                },
                OnSelected = element =>
                {
                    currentSelector = Selector.Wall;
                    Console.WriteLine("Wall selected");
                },
                PositionOffset = new Vector2(10, 0)
            });
            var button2 = root.AddChild(new Button(Anchor.AutoInline, new Vector2(80, 80), "Tower")
            {
                OnPressed = element =>
                {
                    currentSelector = Selector.BasicTower;
                },
                PositionOffset = new Vector2(10, 0)
            });
            var button3 = root.AddChild(new Button(Anchor.AutoInline, new Vector2(80, 80), "Remove Building")
            {
                OnPressed = element =>
                {
                    currentSelector = Selector.Remove;
                },
                PositionOffset = new Vector2(10, 0)
            });
            var button4 = root.AddChild(new Button(Anchor.AutoInline, new Vector2(80, 80), "Bandit")
            {
                OnPressed = element =>
                {
                    currentSelector = Selector.Bandit;
                },
                PositionOffset = new Vector2(10, 0)
            });

            var button5 = root.AddChild(new Button(Anchor.AutoInline, new Vector2(80,80), "Rock")
            {
                OnPressed = element =>
                {
                    currentSelector = Selector.Rock;
                },
                PositionOffset = new Vector2(10, 0)
            });

            var button6 = root.AddChild(new Button(Anchor.AutoInline, new Vector2(80,80), "Tree")
            {
                OnPressed = element =>
                {
                    currentSelector = Selector.Tree;
                },
                PositionOffset = new Vector2(10, 0)
            });
        }

        protected override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

            float dt = gameTime.GetElapsedSeconds();

            // game inputs
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            keyboardState = KeyboardExtended.GetState();
            mouseState = MouseExtended.GetState();

            var mousePosition = mouseState.Position.ToVector2();
            var worldPosition = camera.ScreenToWorld(mousePosition);

            var area = root.Area;
            if (area.Contains(mousePosition.X, mousePosition.Y) && !root.IsHidden)
                goto EndMouse;

            if (mouseState.IsButtonDown(MouseButton.Left))
            {
                var position = Vector2.Floor(worldPosition / TILE_SIZE) * TILE_SIZE + new Vector2(TILE_SIZE / 2);

                int xTilePos = (int)MathF.Floor(worldPosition.X / TILE_SIZE);
                int yTilePos = (int)MathF.Floor(worldPosition.Y / TILE_SIZE);

                if (xTilePos < 0 || xTilePos >= buildingTiles.Length || yTilePos < 0 || yTilePos >= buildingTiles[xTilePos].Length)
                {
                    goto EndBuilding;
                }

                Building currBuilding = buildingTiles[xTilePos][yTilePos];

                switch (currentSelector)
                {
                    case Selector.Wall:
                        if (currBuilding == null)
                        {
                            var wall = new Wall(position);
                            entities.Add(wall);
                            buildingTiles[xTilePos][yTilePos] = wall;
                            SHGBuildings.AddEntity(wall, wall.Position);

                            var nearbyWalls = GetNearbyWalls(xTilePos, yTilePos);
                            foreach (var nearbyWall in nearbyWalls)
                            {
                                var inBetweenWall = new Wall((nearbyWall.Position + wall.Position) / 2);
                                entities.Add(inBetweenWall);
                                SHGBuildings.AddEntity(inBetweenWall, inBetweenWall.CShape);
                            }
                        }
                        break;

                    case Selector.BasicTower:
                        if (currBuilding == null)
                        {
                            var tower = new BasicTower(position);
                            entities.Add(tower);
                            buildingTiles[xTilePos][yTilePos] = tower;
                            SHGBuildings.AddEntity(tower, tower.Position);
                        }
                        break;
                    
                    case Selector.Rock:
                        if (currBuilding == null)
                        {
                            var rock = new Rock(position);
                            entities.Add(rock);
                            buildingTiles[xTilePos][yTilePos] = rock;
                            SHGBuildings.AddEntity(rock, rock.Position);
                        }
                        break;

                    case Selector.Tree:
                        if(currBuilding == null)
                        {
                            var tree = new Tree(position);
                            entities.Add(tree);
                            buildingTiles[xTilePos][yTilePos] = tree;
                            SHGBuildings.AddEntity(tree, tree.Position);
                        }
                        break;

                    case Selector.Remove:
                        if (currBuilding != null)
                        {
                            entities.Remove(currBuilding);
                            buildingTiles[xTilePos][yTilePos] = null;
                            SHGBuildings.RemoveEntityPosition(currBuilding);

                            if (currBuilding is Wall)
                            {
                                var nearbyWalls = GetNearbyWalls(xTilePos, yTilePos);
                                foreach (var nearbyWall in nearbyWalls)
                                {
                                    var tempPos = (nearbyWall.Position + currBuilding.Position) / 2;
                                    var inBetweenWall = entities.Find(e => e.Position == tempPos);
                                    if (inBetweenWall != null)
                                    {
                                        entities.Remove(inBetweenWall);
                                        SHGBuildings.RemoveEntityCShape(inBetweenWall);
                                    }
                                }
                            }
                        }
                        break;
                }

                List<Wall> GetNearbyWalls(int xTilePos, int yTilePos)
                {
                    var nearbyWalls = new List<Wall>();

                    var posXs = new int[] { -1, 1, 0, 0 };
                    var posYs = new int[] { 0, 0, -1, 1 };

                    for (int i = 0; i < 4; i++)
                    {
                        var x = xTilePos + posXs[i];
                        var y = yTilePos + posYs[i];

                        if (x >= 0 && x < buildingTiles.Length && y >= 0 && y < buildingTiles[x].Length)
                        {
                            var tempWall = buildingTiles[x][y];
                            var nearbyWall = tempWall as Wall;
                            if (nearbyWall != null)
                            {
                                nearbyWalls.Add(nearbyWall);
                            }
                        }
                    }

                    return nearbyWalls;
                }
            }
        EndBuilding:;

            if (mouseState.WasButtonJustDown(MouseButton.Left))
            {
                if (currentSelector == Selector.Bandit)
                {
                    for (int i = 0; i < 1; i++)
                    {
                        for (int j = 0; j < 1; j++)
                        {
                            entities.Add(new Bandit(worldPosition + new Vector2(i * 5, j * 5), 5));
                        }
                    }
                }
            }
        EndMouse:;

            foreach (Selector value in Enum.GetValues(typeof(Selector)))
            {
                if (keyboardState.WasKeyJustUp(Keys.D1 + (int)value))
                {
                    currentSelector = value;
                }
            }
            if (keyboardState.IsKeyDown(Keys.C))
            {
                if (currentSelector == Selector.Bandit)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            entities.Add(new Bandit(worldPosition + new Vector2(i * 5, j * 5), 5));
                        }
                    }
                }
            }

            if (keyboardState.WasKeyJustUp(Keys.E))
            {
                debug = !debug;
            }

            if (keyboardState.WasKeyJustUp(Keys.Q))
            {
                root.IsHidden = !root.IsHidden;
            }

            // player movement
            var direction = new Vector2(
                Convert.ToSingle(state.IsKeyDown(Keys.D)) - Convert.ToSingle(state.IsKeyDown(Keys.A)),
                Convert.ToSingle(state.IsKeyDown(Keys.S)) - Convert.ToSingle(state.IsKeyDown(Keys.W))
            );
            //player.Move(direction, dt);
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            player.DecideDirection(camera.ScreenToWorld(mousePosition));

            float up = 0;
            if (state.IsKeyDown(Keys.Space))
            {
                up += 1;
            }
            if (state.IsKeyDown(Keys.LeftShift))
            {
                up -= 1;
            }

             //camera3D.Move(direction.Y * dt * 5, direction.X * dt * 5, up * dt * 5);

            if (state.IsKeyDown(Keys.Left))
            {
                model_y_rotation -= 180f * dt;
            }

            if (state.IsKeyDown(Keys.Right))
            {
                model_y_rotation += 180f * dt;
            }

            
            var mouseNow = Mouse.GetState();
            if (mouseNow.X != mouseDefaultPos.X || mouseNow.Y != mouseDefaultPos.Y)
            {
                Vector2 mouseDifference;
                mouseDifference.X = mouseDefaultPos.X - mouseNow.X;
                mouseDifference.Y = mouseDefaultPos.Y - mouseNow.Y;

                // camera3D.Rotate(mouseDifference.X / 400, mouseDifference.Y / 400);

                //Mouse.SetPosition((int)mouseDefaultPos.X, (int)mouseDefaultPos.Y);
            }


            // enemy flocking
            // Parallel.ForEach(enemies, e =>
            // {
            //     e.ApplyFlocking(dt, SHGFlocking, SHGBuildings, player.Position);
            // });

            // enemy movement
            foreach (var e in enemies)
            {
                enemy.Steer(dt, SHGBuildings, player.Position);
            }

            // projectile
            var projectilesTemp = new List<Projectile>(projectiles);
            foreach (var projectile in projectilesTemp)
            {
                projectile.Update(dt);
                if (projectile.HasEnded)
                {
                    projectiles.Remove(projectile);
                }
            }

            Parallel.ForEach(towers, tower =>
            {
                var projectile = tower.Shoot(SHGEnemies);
                if (projectile != null)
                {
                    projectiles.Add(projectile);
                }
            });

            // enemy death
            var enemiesTemp = new List<Enemy>(enemies);
            foreach (var enemy in enemiesTemp)
            {
                if (enemy.IsDead)
                {
                    entities.Remove(enemy);
                }
            }
        
            // building death
            var buildingsTemp = new List<Building>(buildings);
            foreach (var building in buildingsTemp)
            {
                if (building.IsDead)
                {
                    entities.Remove(building);
                    var pos = buildingTiles.CoordinatesOf(building);
                    if (pos.HasValue)
                    {
                        buildingTiles[pos.Value.X][pos.Value.Y] = null;
                    } 
                    SHGBuildings.RemoveEntityPosition(building);
                }
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

       
        protected override async void DoDraw(GameTime gameTime)
        {
            float frameRate = 1 / gameTime.GetElapsedSeconds();

             GraphicsDevice.Clear(Color.CornflowerBlue);

             SpriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: camera.GetTransform(), blendState: BlendState.AlphaBlend);

            // // draw tilemap
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

            // // draw entities
             var entities_temp = entities.OrderBy(e => e.Position.Y).ToArray();
             foreach (var entity in entities_temp)
             {
                 entity.DrawDebug(SpriteBatch);
                 entity.Draw(SpriteBatch);
             }

             SpriteBatch.End();

            // // Drawing the Text
             SpriteBatch.Begin();
             SpriteBatch.DrawString(font, $"Frame Rate: {frameRate:N2}", new Vector2(10, 10), Color.Black);
             SpriteBatch.End();
             //Set the render target

GraphicsDevice.Viewport= (modelview);

        GraphicsDevice.SetRenderTarget(modelBase);
 
        GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

    
            scale = 1f/(108f/graphics.GraphicsDevice.Viewport.Height);
           
       

            GraphicsDevice.SetRenderTarget(modelBase);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp,depthStencilState: DepthStencilState.Default);        
            DepthStencilState dss = new DepthStencilState();
            dss.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = dss;
        

for (int i = 0; i < 2; i++)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.LightingEnabled = true;
                        effect.AmbientLightColor = new Vector3(0.8f, 0.8f, 0.8f);
                        effect.DirectionalLight0.DiffuseColor = new Vector3(0.3f, 0.3f, 0.3f);
                        effect.DirectionalLight0.Direction = new Vector3(0.0f, -1.0f, 0.0f);
                        effect.DirectionalLight0.SpecularColor = new Vector3(0.5f, 0.2f, 0.2f);
                       
                        
                        effect.View = camera3D.GetViewMatrix();
                        effect.World = Matrix.CreateRotationY(MathHelper.ToRadians(model_y_rotation)) * Matrix.CreateTranslation(0, 0, i * 4);
                        effect.Projection = camera3D.GetProjectionMatrix();
                        effect.Texture=testtex;
                        effect.TextureEnabled=true;
                    }
                    mesh.Draw();
                }
            }


            SpriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp,depthStencilState: DepthStencilState.Default);
            SpriteBatch.Draw(modelBase,Vector2.Zero,null,Color.White,0f,Vector2.Zero,scale,SpriteEffects.None, 0f);
            
                
            
            SpriteBatch.End();
            //GraphicsDevice.Clear(Color.Black);
            

            

            base.DoDraw(gameTime);
        
    }
        float model_y_rotation = 0;

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

    }
}
