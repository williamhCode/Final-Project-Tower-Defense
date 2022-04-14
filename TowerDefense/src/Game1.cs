using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.Extended;

using System;
using System.Collections.Generic;
using System.Linq;

using TowerDefense.Camera;
using TowerDefense.Entities;
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

        private Dictionary<string, SoundEffect> soundEffects;
        private MouseStateExtended mouseState;
        private KeyboardStateExtended keyboardState;

        Wall[] walls;
        
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

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Font/Frame");
            // TODO: use this.Content to load your game content here
            Player.LoadContent(Content);
            Wall.LoadContent(Content);

            // load sound effects
            soundEffects = new Dictionary<string, SoundEffect>();
            string[] effectNames = new string[] {"playerwalk", "enemyappear", "wood", "basictower"};
            foreach (string name in effectNames)
            {
                soundEffects.Add(name, Content.Load<SoundEffect>("Effects/" + name));
            }

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
            this.UiSystem.AutoScaleWithScreen = true;
            this.UiSystem.GlobalScale = 5;

            /*
            this.root = new Panel(Anchor.TopLeft, new Vector2(100,100), Vector2.Zero, false, true);
            this.root.ScrollBar.SmoothScrolling = true;
            this.UiSystem.Add("TestUi", this.root);
            float timesPressed = 0f;
            var box = new Panel(Anchor.Center, new Vector2(100,1), Vector2.Zero, setHeightBasedOnChildren: true);
            var bar1 = box.AddChild(new ProgressBar(Anchor.AutoLeft, new Vector2(1,8), MLEM.Misc.Direction2.Right, 10));
            CoroutineHandler.Start(WobbleProgressBar(bar1));
            var button1 = box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 20), "Okay") 
            {
                OnPressed = element => 
                {
                    //this.UiSystem.Remove("TestUi");
                    //this.UiSystem.Remove("InfoBox");
                    timesPressed += 1f;
                    CoroutineHandler.Start(WobbleButton(element));
                }, 
                PositionOffset = new Vector2(0, 1)
            });
            this.UiSystem.Add("InfoBox", box);
            */
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

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float dt = gameTime.GetElapsedSeconds();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState state = Keyboard.GetState();
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
                Console.WriteLine("Tile position: " + xTilePos + ", " + yTilePos);
                Building currBuilding = buildingTiles[xTilePos][yTilePos];

                if (keyboardState.IsKeyDown(Keys.D1))
                {
                    // check if entites has building with same position
                    if (currBuilding == null)
                    {
                        var wall = new Wall(position);
                        entities.Add(wall);
                        buildingTiles[xTilePos][yTilePos] = wall;
                        SHGBuildings.AddEntityPosition(wall);

                        var nearbyWalls = GetNearbyWalls(xTilePos, yTilePos);
                        foreach (var nearbyWall in nearbyWalls)
                        {
                            var inBetweenWall = new Wall((nearbyWall.Position + wall.Position) / 2);
                            entities.Add(inBetweenWall);
                            SHGBuildings.AddEntityCShape(inBetweenWall);
                        }
                        soundEffects["wood"].Play();
                    }
                }
                if (keyboardState.IsKeyDown(Keys.D2))
                {
                    if (currBuilding == null)
                    {
                        var tower = new BasicTower(position);
                        entities.Add(tower);
                        buildingTiles[xTilePos][yTilePos] = tower;
                        SHGBuildings.AddEntityPosition(tower);
                        soundEffects["basictower"].Play();
                    }
                }
                if (keyboardState.IsKeyDown(Keys.D3))
                {
                    if (currBuilding != null)
                    {
                        entities.Remove(currBuilding);
                        buildingTiles[xTilePos][yTilePos] = null;
                        SHGBuildings.RemoveEntityPosition(currBuilding);
                    }
                }
            }

            if (state.IsKeyDown(Keys.OemPlus))
            {
                camera.Zoom += 0.1f;
            }
            if (state.IsKeyDown(Keys.OemMinus))
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        entities.Add(new Bandit(worldPosition + new Vector2(i * 5, j * 5), 5));
                    }
                }
                soundEffects["enemyappear"].Play();
            }

            if (keyboardState.WasKeyJustUp(Keys.E))
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

            foreach (var wall in walls)
            player.Move(dt, direction);
            player.DecideDirection(worldPosition);
            if (keyboardState.WasKeyJustUp(Keys.W)
            || keyboardState.WasKeyJustUp(Keys.A)
            || keyboardState.WasKeyJustUp(Keys.S) 
            || keyboardState.WasKeyJustUp(Keys.D)) {
                soundEffects["playerwalk"].Play();
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // enemy flocking
            foreach (var e in enemies)
            {
                e.ApplyFlocking(dt, SHGFlocking, player.Position);
            }

            sw.Stop();
            // Console.WriteLine(sw.Elapsed.TotalSeconds);

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

            // tower shooting
            foreach (var tower in towers)
            {
                var projectile = tower.Shoot(dt, SHGEnemies);
                if (projectile != null)
                {
                    projectiles.Add(projectile);
                }
            }

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
