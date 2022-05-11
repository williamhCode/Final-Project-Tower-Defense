using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using TowerDefense.Entities.Buildings;
using TowerDefense.Hashing;
using TowerDefense.Projectiles;
using TowerDefense.NoiseTest;

using Newtonsoft.Json.Linq;

//test
namespace TowerDefense
{
    public class LevelEditor : MlemGame
    {
        // variables
        public static LevelEditor Instance { get; private set; }
        public SpriteFont font;
        private Camera2D camera;
        private Panel bottomPanel;

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
        private const int MAP_WIDTH = 50;
        private const int MAP_HEIGHT = 50;

        private const float CAMERA_SPEED = 600f;

        private MouseStateExtended mouseState;
        private KeyboardStateExtended keyboardState;
        private bool debug;

        private TileType[][] tileTypeMap;
        private int[][] tileMap;
        Dictionary<int, Texture2D> textureDict = new Dictionary<int, Texture2D>();
        readonly (int X, int Y)[] neighborOffsets = new[] { (-1, -1), (0, -1), (1, -1), (-1, 0), (1, 0), (-1, 1), (0, 1), (1, 1) };

        public enum TileType
        {
            Undefined = -1,
            Grass,
            Path,
        }

        private TileType currentSelector;

        public LevelEditor()
        {
            Instance = this;
            this.IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // tile map initialization
            tileTypeMap = new TileType[MAP_HEIGHT][];
            for (int i = 0; i < tileTypeMap.Length; i++)
            {
                tileTypeMap[i] = new TileType[MAP_WIDTH];
            }
            for (int i = 0; i < tileTypeMap.Length; i++)
            {
                for (int j = 0; j < tileTypeMap[i].Length; j++)
                {
                    tileTypeMap[i][j] = TileType.Undefined;
                }
            }

            tileMap = new int[MAP_HEIGHT][];
            for (int i = 0; i < tileMap.Length; i++)
            {
                tileMap[i] = new int[MAP_WIDTH];
            }
            for (int i = 0; i < tileMap.Length; i++)
            {
                for (int j = 0; j < tileMap[i].Length; j++)
                {
                    tileMap[i][j] = -1;
                }
            }

            Noise NoiseMap = new TowerDefense.NoiseTest.Noise();
            float[] noiseMap = NoiseMap.GenerateNoiseMap(
                MAP_WIDTH, MAP_HEIGHT,
                seed: 1,
                scale: 7f,
                octaves: 1,
                persistance: 1f,
                lacunarity: 1f,
                offset: Vector2.Zero
            );

            // Generate Biomes
            for (int i = 0; i < MAP_WIDTH; i++)
            {
                for (int j = 0; j < MAP_HEIGHT; j++)
                {
                    float height = noiseMap[i * MAP_WIDTH + j];
                    if (height <= 0.7f)
                    {
                        tileTypeMap[i][j] = TileType.Path;
                    }
                    else
                    {
                        tileTypeMap[i][j] = TileType.Grass;
                    }
                }
            }

            // Update tile map
            for (int i = 0; i < MAP_WIDTH; i++)
            {
                for (int j = 0; j < MAP_HEIGHT; j++)
                {
                    UpdateTile(i, j);
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

            // tile loading and configuration
            dynamic tileInfo = JObject.Parse(File.ReadAllText("Content/TileInfo.json"));

            Content.RootDirectory = tileInfo.RootDirectory;

            dynamic dataList = tileInfo.DataList;
            for (int i = 0; i < dataList.Count; i++)
            {
                dynamic data = dataList[i];

                Texture2D spriteSheet = Content.Load<Texture2D>((string)data.TileName);

                int tileSize = data.TileSize;
                int width = data.Width;
                int height = data.Height;

                dynamic configList = data.ConfigList;
                for (int j = 0; j < configList.Count; j++)
                {
                    string config = configList[j];
                    if (config == "None") continue;
                    config = config.Replace(",", "");
                    config = config.Remove(4, 1);
                    config = new string(config.Reverse().ToArray());

                    int value = Convert.ToInt32(config, 2);
                    value |= i << 8;

                    Texture2D texture = new Texture2D(GraphicsDevice, tileSize, tileSize);
                    // set texture data from sprite sheet
                    Color[] textureData = new Color[tileSize * tileSize];
                    int x = (j % width) * tileSize;
                    int y = (j / width) * tileSize;
                    spriteSheet.GetData(0, new Rectangle(x, y, tileSize, tileSize), textureData, 0, tileSize * tileSize);
                    texture.SetData(textureData);

                    textureDict.Add(value, texture);
                }
            }

            // load fonts
            Content.RootDirectory = "Content";
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
            this.UiSystem.AutoScaleWithScreen = false;
            this.UiSystem.GlobalScale = 1;

            bottomPanel = new Panel(Anchor.Center, new Vector2(800, 100), new Vector2(0, 300), false, true);
            bottomPanel.ScrollBar.SmoothScrolling = true;
            bottomPanel.AddChild(new VerticalSpace(2));
            this.UiSystem.Add("TestUi", bottomPanel);

            var button1 = bottomPanel.AddChild(new Button(Anchor.AutoLeft, new Vector2(80, 80), "Grass")
            {
                OnSelected = element =>
                {
                    currentSelector = TileType.Grass;
                },
                PositionOffset = new Vector2(10, 0)
            });
            var button2 = bottomPanel.AddChild(new Button(Anchor.AutoInline, new Vector2(80, 80), "Path")
            {
                OnSelected = element =>
                {
                    currentSelector = TileType.Path;
                },
                PositionOffset = new Vector2(10, 0)
            });
            var button3 = bottomPanel.AddChild(new Button(Anchor.AutoInline, new Vector2(80, 80), "Erase")
            {
                OnSelected = element =>
                {
                    currentSelector = TileType.Undefined;
                },
                PositionOffset = new Vector2(10, 0)
            });

            var buttonFileSave = this.UiSystem.Add("FileSave", new Button(Anchor.AutoRight, new Vector2(60, 30), "Save")
            {
                OnPressed = element =>
                {
                    SaveMap();
                },
                CanBeSelected = false
            });

            // save the map into a JSON file
            void SaveMap()
            {
                Console.WriteLine("Saving map...");
            }

        }

        void UpdateTile(int xPos, int yPos)
        {
            TileType tileType;
            try { tileType = tileTypeMap[xPos][yPos]; }
            catch (IndexOutOfRangeException) { return; }

            if (tileType == TileType.Undefined) return;

            // nB = neighborsBool
            var nB = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                var offset = neighborOffsets[i];
                int x = xPos + offset.X;
                int y = yPos + offset.Y;

                try { nB[i] = tileType == tileTypeMap[x][y]; }
                catch (IndexOutOfRangeException) { nB[i] = false; }
            }

            // corner tiles only valid if its surrounded by the same tile
            nB[0] = nB[0] && nB[1] && nB[3];
            nB[2] = nB[2] && nB[1] && nB[4];
            nB[5] = nB[5] && nB[3] && nB[6];
            nB[7] = nB[7] && nB[4] && nB[6];

            // convert nB to int
            int nBInt = 0;
            for (int i = 0; i < 8; i++)
            {
                if (nB[i])
                    nBInt |= 1 << i;
            }
            nBInt |= (int)tileType << 8;

            if (!textureDict.ContainsKey(nBInt))
            {
                nBInt = (int)tileType << 8;
            }

            tileMap[xPos][yPos] = nBInt;
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

            var area = bottomPanel.Area;
            if (area.Contains(mousePosition.X, mousePosition.Y) && !bottomPanel.IsHidden)
                goto EndMouse;

            if (mouseState.IsButtonDown(MouseButton.Left))
            {
                var position = Vector2.Floor(worldPosition / TILE_SIZE) * TILE_SIZE + new Vector2(TILE_SIZE / 2);

                int xTilePos = (int)MathF.Floor(worldPosition.X / TILE_SIZE);
                int yTilePos = (int)MathF.Floor(worldPosition.Y / TILE_SIZE);

                if (xTilePos >= 0 && xTilePos < MAP_WIDTH && yTilePos >= 0 && yTilePos < MAP_HEIGHT)
                {
                    tileTypeMap[xTilePos][yTilePos] = currentSelector;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            int x = xTilePos + i;
                            int y = yTilePos + j;
                            UpdateTile(x, y);
                        }
                    }
                }
            }

        EndMouse:;

            foreach (TileType value in Enum.GetValues(typeof(TileType)))
            {
                if (keyboardState.WasKeyJustUp(Keys.D1 + (int)value))
                {
                    currentSelector = value;
                }
            }

            if (keyboardState.WasKeyJustUp(Keys.E))
            {
                debug = !debug;
            }

            if (keyboardState.WasKeyJustUp(Keys.Q))
            {
                var rootElements = UiSystem.GetRootElements();
                foreach (var e in rootElements)
                {
                    e.Element.IsHidden = !e.Element.IsHidden;
                }
            }

            // camera
            if (keyboardState.IsKeyDown(Keys.OemPlus))
            {
                camera.ZoomFromCenter(1.1f);
            }
            if (keyboardState.IsKeyDown(Keys.OemMinus))
            {
                camera.ZoomFromCenter(1 / 1.1f);
            }

            var direction = new Vector2(
                Convert.ToSingle(keyboardState.IsKeyDown(Keys.D)) - Convert.ToSingle(keyboardState.IsKeyDown(Keys.A)),
                Convert.ToSingle(keyboardState.IsKeyDown(Keys.S)) - Convert.ToSingle(keyboardState.IsKeyDown(Keys.W))
            );

            camera.Move(direction / camera.Zoom * dt * CAMERA_SPEED);
        }

        protected override void DoDraw(GameTime gameTime)
        {
            float frameRate = 1 / gameTime.GetElapsedSeconds();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: camera.GetTransform(), blendState: BlendState.AlphaBlend);

            // draw tilemap
            var viewport = camera.GetViewport();
            int xStart = (int)Math.Floor(viewport.X / TILE_SIZE);
            int yStart = (int)Math.Floor(viewport.Y / TILE_SIZE);
            int xEnd = (int)Math.Ceiling((viewport.X + viewport.Width) / TILE_SIZE);
            int yEnd = (int)Math.Ceiling((viewport.Y + viewport.Height) / TILE_SIZE);

            xStart = Math.Max(xStart, 0);
            yStart = Math.Max(yStart, 0);
            xEnd = Math.Min(xEnd, MAP_WIDTH);
            yEnd = Math.Min(yEnd, MAP_HEIGHT);

            for (int row = xStart; row < xEnd; row++)
            {
                for (int col = yStart; col < yEnd; col++)
                {
                    if (tileTypeMap[row][col] != TileType.Undefined)
                    {
                        SpriteBatch.Draw(textureDict[tileMap[row][col]], new Vector2(TILE_SIZE * row, TILE_SIZE * col), Color.White);
                    }
                }
            }

            // draw preview of current tile with transparency
            var screenPos = MouseExtended.GetState().Position.ToVector2();
            var worldPos = camera.ScreenToWorld(screenPos);
            var tilePos = Vector2.Floor(worldPos / TILE_SIZE);

            TileType? currTileType;
            try { currTileType = tileTypeMap[(int)tilePos.X][(int)tilePos.Y]; }
            catch (IndexOutOfRangeException) { currTileType = null; }

            if (currTileType.HasValue && currentSelector != TileType.Undefined && currentSelector != currTileType)
            {
                var previewPosition = tilePos * TILE_SIZE;
                SpriteBatch.Draw(textureDict[(int)currentSelector << 8], previewPosition, Color.White * 0.6f);
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
