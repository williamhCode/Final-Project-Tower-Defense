using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using MLEM.Startup;
using MLEM.Font;
using MonoGame.Framework.Utilities;
using MonoGame.Extended;
using MonoGame.Extended.Input;

using TowerDefense.Camera;
using TowerDefense.Entities;
using TowerDefense.Entities.Enemies;
using TowerDefense.Entities.Buildings;
using TowerDefense.Hashing;
using TowerDefense.Projectiles;
using Towerdefense.Entities.Components;
using TowerDefense.NoiseTest;
using static TowerDefense.Collision.CollisionFuncs;
using static TowerDefense.Extensions.ExtensionMethods;

using System.Threading.Tasks;

//test
namespace TowerDefense
{
    public class Game1 : MlemGame
    {
        // variables
        public static Game1 Instance { get; private set; }
        public SpriteFont font;
        private Camera2D camera;
        private Panel root;

        private Player player;
        private List<Entity> entities;
        private Building[][] buildingTiles;
        private Building[] buildings;
        private Tower[] towers;
        private Enemy[] enemies;
        private List<Projectile> projectiles;

        private SpatialHashGrid SHGBuildings;
        private SpatialHashGrid SHGFlocking;
        private SpatialHashGrid SHGEnemies;

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
            camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // entities initialization
            player = new Player(new Vector2(300, 300));
            entities = new List<Entity> {
                player,
            };

            buildings = entities.OfType<Building>().ToArray();
            towers = buildings.OfType<Tower>().ToArray();
            enemies = entities.OfType<Enemy>().ToArray();
            projectiles = new List<Projectile>();

            // tile map initialization
            tileMap = new string[MAP_SIZE][];
            for (int i = 0; i < tileMap.Length; i++)
            {
                tileMap[i] = new string[MAP_SIZE];
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

            // Generate Biomes
            for (int i = 0; i < MAP_SIZE; i++)
            {
                for (int j = 0; j < MAP_SIZE; j++)
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

            buildingTiles = new Building[tileMap.Length][];
            for (int i = 0; i < buildingTiles.Length; i++)
            {
                buildingTiles[i] = new Building[tileMap[i].Length];
            }

            SHGBuildings = new SpatialHashGrid(32);
            foreach (var building in buildings)
            {
                SHGBuildings.AddEntity(building, building.Position);
            }

            SHGFlocking = new SpatialHashGrid(90);

            SHGEnemies = new SpatialHashGrid(90);

            debug = false;
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

            Content.RootDirectory = "Content/Sprites/Tiles";
            string[] tileNames = new string[] { "grass", "snow", "water", "beach", "sand", "deepwater" };
            foreach (string name in tileNames)
            {
                tileTextures.Add(name, Content.Load<Texture2D>(name));
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
                // = Color.Black,
                // ButtonHoveredColor = Color.Red,
                //TextScale = 0.1f,
                // SelectionIndicator = new MLEM.Textures.NinePatch(new MLEM.Textures.TextureRegion(tex, new Rectangle(0, 0, 100, 100)), 4),
            };

            this.UiSystem.Style = style;
            this.UiSystem.AutoScaleReferenceSize = new Point(1280, 720);
            this.UiSystem.AutoScaleWithScreen = false;
            this.UiSystem.GlobalScale = 1;

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

            // update type arrays
            buildings = entities.OfType<Building>().ToArray();
            towers = buildings.OfType<Tower>().ToArray();
            enemies = entities.OfType<Enemy>().ToArray();

            // update spatial hash grids
            SHGFlocking.Clear();
            foreach (var enemy in enemies)
            {
                SHGFlocking.AddEntity(enemy, enemy.Position);
            }

            SHGEnemies.Clear();
            foreach (var enemy in enemies)
            {
                SHGEnemies.AddEntity(enemy, enemy.HitboxShape);
            }

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
                Convert.ToSingle(keyboardState.IsKeyDown(Keys.D)) - Convert.ToSingle(keyboardState.IsKeyDown(Keys.A)),
                Convert.ToSingle(keyboardState.IsKeyDown(Keys.S)) - Convert.ToSingle(keyboardState.IsKeyDown(Keys.W))
            );

            player.Move(dt, direction);
            player.DecideDirection(worldPosition);

            // enemy flocking
            // Parallel.ForEach(enemies, e =>
            // {
            //     e.ApplyFlocking(dt, SHGFlocking, SHGBuildings, player.Position);
            // });

            // enemy movement
            foreach (var enemy in enemies)
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
                var buildings = SHGBuildings.QueryEntities(e.CShape);
                buildings = buildings.OrderBy(w => (w.Position - e.Position).LengthSquared()).ToList();

                foreach (var building in buildings)
                {
                    if (IsColliding(building.CShape, e.CShape, out Vector2 mtv))
                    {
                        e.Position += mtv;
                        e.CShape.Update();
                    }
                }
            }

            foreach (IHitboxComponent e in entitiesToCheck)
            {
                e.UpdateHitbox();
            }

            // camera
            if (keyboardState.IsKeyDown(Keys.OemPlus))
            {
                camera.Zoom *= 1.1f;
            }
            if (keyboardState.IsKeyDown(Keys.OemMinus))
            {
                camera.Zoom /= 1.1f;
            }
            camera.LookAt(player.Position);
        }

        protected override void DoDraw(GameTime gameTime)
        {
            float frameRate = 1 / gameTime.GetElapsedSeconds();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            var projectilesLookup = projectiles.ToLookup(p => p.HasHit);

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

            // projectiles have hit get drawn below
            foreach (var projectile in projectilesLookup[true])
            {
                projectile.Draw(SpriteBatch);
            }

            // draw entities
            var entities_temp = entities.OrderBy(e => e.Position.Y).ToArray();
            foreach (var entity in entities_temp)
            {
                if (debug)
                    entity.DrawDebug(SpriteBatch);
                entity.Draw(SpriteBatch);
            }

            // projectiles have not hit get drawn above
            foreach (var projectile in projectilesLookup[false])
            {
                projectile.Draw(SpriteBatch);
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
