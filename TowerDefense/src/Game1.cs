using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Coroutine;
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
using MonoGame.Extended.Input;

using TowerDefense.Camera;
using TowerDefense.Entities;
using TowerDefense.Entities.Enemies;
using TowerDefense.Entities.Buildings;
using TowerDefense.Hashing;
using TowerDefense.Projectiles;
using Towerdefense.Entities.Components;
using static TowerDefense.Collision.CollisionFuncs;

using System.Diagnostics;
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
        private Dictionary<string, Texture2D> tileTextures;
        private string[][] tileMap;

        private MouseStateExtended mouseState;
        private KeyboardStateExtended keyboardState;

<<<<<<< Updated upstream
        private bool debug;
=======
        public enum DebugSelector
        {
            Bandit,
            BasicTower,
            Wall,
            None
        }
>>>>>>> Stashed changes

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
                //TextScale = 0.1f,
            };

            this.UiSystem.Style = style;
            this.UiSystem.AutoScaleReferenceSize = new Point(1280, 720);
            this.UiSystem.AutoScaleWithScreen = false;
            this.UiSystem.GlobalScale = 1;

            
            this.root = new Panel(Anchor.Center, new Vector2(1200,100), new Vector2(0,300), false, true);
            this.root.ScrollBar.SmoothScrolling = true;
            this.UiSystem.Add("TestUi", this.root);
            
            float timesPressed = 0f;
            /*
            var box = new Panel(Anchor.Center, new Vector2(100,1), Vector2.Zero, setHeightBasedOnChildren: true);
            //var bar1 = box.AddChild(new ProgressBar(Anchor.AutoLeft, new Vector2(1,8), MLEM.Misc.Direction2.Right, 10));
            //CoroutineHandler.Start(WobbleProgressBar(bar1));
            var button1 = box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 20), "Okay") 
            {
                OnPressed = element => 
                {
                    //this.UiSystem.Remove("TestUi");
                    //this.UiSystem.Remove("InfoBox");
                    timesPressed += 1f;
                    //CoroutineHandler.Start(WobbleButton(element));
                }, 
                PositionOffset = new Vector2(0, 1)
            });
            this.UiSystem.Add("InfoBox", box);
            */
            var box = root.AddChild(new Panel(Anchor.AutoCenter, new Vector2(100,100), Vector2.Zero, setHeightBasedOnChildren: false));
            var button1 = box.AddChild(new Button(Anchor.AutoCenter, new Vector2(20, 20), "Okay")
            {
                OnPressed = element =>
                {
                    timesPressed += 1f;
                },
                PositionOffset = new Vector2(0, 1)
            });
            
        }

        private List<Wall> GetNearbyWalls(int xTilePos, int yTilePos)
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

            var tileKeys = new Keys[] { Keys.D1, Keys.D2, Keys.D3 };
            if (keyboardState.GetPressedKeys().Intersect(tileKeys).Any())
            {
                var position = Vector2.Floor(worldPosition / TILE_SIZE) * TILE_SIZE + new Vector2(TILE_SIZE / 2);

                int xTilePos = (int)MathF.Floor(worldPosition.X / TILE_SIZE);
                int yTilePos = (int)MathF.Floor(worldPosition.Y / TILE_SIZE);
                
                if (xTilePos < 0 || xTilePos >= buildingTiles.Length || yTilePos < 0 || yTilePos >= buildingTiles[xTilePos].Length)
                {
                    goto End;
                }

                Building currBuilding = buildingTiles[xTilePos][yTilePos];

                if (keyboardState.IsKeyDown(Keys.D1))
                {
                    // check if entites has building with same position
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
                }
                if (keyboardState.IsKeyDown(Keys.D2))
                {
                    if (currBuilding == null)
                    {
                        var tower = new BasicTower(position);
                        entities.Add(tower);
                        buildingTiles[xTilePos][yTilePos] = tower;
                        SHGBuildings.AddEntity(tower, tower.Position);
                    }
                }
                if (keyboardState.IsKeyDown(Keys.D3))
                {
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
                }
            }
            End:;

            if (keyboardState.WasKeyJustUp(Keys.D4))
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        entities.Add(new Bandit(worldPosition + new Vector2(i * 5, j * 5), 5));
                    }
                }
            }
            if (keyboardState.WasKeyJustUp(Keys.E))
            {
                debug = !debug;
            }

            // player movement
            var direction = new Vector2(
                Convert.ToSingle(keyboardState.IsKeyDown(Keys.D)) - Convert.ToSingle(keyboardState.IsKeyDown(Keys.A)),
                Convert.ToSingle(keyboardState.IsKeyDown(Keys.S)) - Convert.ToSingle(keyboardState.IsKeyDown(Keys.W))
            );

            player.Move(dt, direction);
            player.DecideDirection(worldPosition);

            // enemy flocking
            Parallel.ForEach(enemies, e =>
            {
                e.ApplyFlocking(dt, SHGFlocking, player.Position);
            });

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

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Parallel.ForEach(towers, tower =>
            {
                var projectile = tower.Shoot(SHGEnemies);
                if (projectile != null)
                {
                    projectiles.Add(projectile);
                }
            });

            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalSeconds);
            Console.WriteLine(enemies.Length);

            // enemy death
            var enemiesTemp = new List<Enemy>(enemies);
            foreach (var enemy in enemiesTemp)
            {
                if (enemy.IsDead)
                {
                    entities.Remove(enemy);
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

        private static IEnumerator<Wait> WobbleButton(Element button)
        {
            var counter = 0f;
            while (counter < 4 * Math.PI && button.Root != null)
            {
                button.Transform = Matrix.CreateTranslation((float)Math.Sin(counter / 2) * 2 * button.Scale, 0, 0);
                counter += 0.1f;
                yield return new Wait(0.01f);
            }
            button.Transform = Matrix.Identity;
        }

        private static IEnumerator<Wait> WobbleProgressBar(ProgressBar bar)
        {
            var reducing = false;
            while (bar.Root != null)
            {
                if (reducing)
                {
                    bar.CurrentValue -= 0.1f;
                    if (bar.CurrentValue <= 0)
                        reducing = false;
                }
                else
                {
                    bar.CurrentValue += 0.1f;
                    if (bar.CurrentValue >= bar.MaxValue)
                        reducing = true;
                }
                yield return new Wait(0.01f);
            }
        }
    }
}
