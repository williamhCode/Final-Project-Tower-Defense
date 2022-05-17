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
using TowerDefense.Map;

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
        private const int MAP_WIDTH = 100;
        private const int MAP_HEIGHT = 100;

        private const float CAMERA_SPEED = 600f;

        private MouseStateExtended mouseState;
        private KeyboardStateExtended keyboardState;
        private bool debug;

        MapHandler mapHandler = new MapHandler(MAP_WIDTH, MAP_HEIGHT, TILE_SIZE);

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
                        mapHandler.tileTypeMap[i][j] = TileType.Path;
                    }
                    else
                    {
                        mapHandler.tileTypeMap[i][j] = TileType.Grass;
                    }
                }
            }

            mapHandler.UpdateTileMap();
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
            mapHandler.LoadTileInfo(this, "Content/TileInfo.json");

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
                    mapHandler.SaveMap();
                },
                CanBeSelected = false
            });

            // save the map into a JSON file
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
                    mapHandler.tileTypeMap[xTilePos][yTilePos] = currentSelector;
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            int x = xTilePos + i;
                            int y = yTilePos + j;
                            mapHandler.UpdateTile(x, y);
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
            mapHandler.DrawMap(SpriteBatch, camera);

            // draw preview of current tile with transparency
            var screenPos = MouseExtended.GetState().Position.ToVector2();
            var worldPos = camera.ScreenToWorld(screenPos);
            var tilePos = Vector2.Floor(worldPos / TILE_SIZE);

            TileType? currTileType;
            try { currTileType = mapHandler.tileTypeMap[(int)tilePos.X][(int)tilePos.Y]; }
            catch (IndexOutOfRangeException) { currTileType = null; }

            if (currTileType.HasValue && currentSelector != TileType.Undefined && currentSelector != currTileType)
            {
                var previewPosition = tilePos * TILE_SIZE;
                SpriteBatch.Draw(mapHandler.textureDict[(int)currentSelector << 8], previewPosition, Color.White * 0.6f);
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
