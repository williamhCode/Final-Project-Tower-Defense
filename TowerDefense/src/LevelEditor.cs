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
using TowerDefense.NoiseTest;
using static TowerDefense.Collision.CollisionFuncs;
using TowerDefense.Collision;

using System.Diagnostics;
using System.Threading.Tasks;

//test
namespace TowerDefense
{
    public class LevelEditor : MlemGame
    {
        // variables
        public static LevelEditor Instance { get; private set; }
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

        private Vector2 start;
        private Vector2 end;
        (float dist, Vector2 intersection, Vector2 normal)? collData;

        public enum Selector
        {
            Wall,
            BasicTower,
            Remove,
            Bandit,
        }

        private Selector currentSelector;

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
            tileMap = new string[MAP_SIZE][];
            for (int i = 0; i < tileMap.Length; i++)
            {
                tileMap[i] = new string[MAP_SIZE];
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
            // button3.AddTooltip(p => this.InputHandler.IsModifierKeyDown(MLEM.Input.ModifierKey.Control) ? "AAAAAA" : string.Empty);
            var button4 = root.AddChild(new Button(Anchor.AutoInline, new Vector2(80, 80), "Bandit")
            {
                OnPressed = element =>
                {
                    currentSelector = Selector.Bandit;
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
            }
        EndBuilding:;

        EndMouse:;

            foreach (Selector value in Enum.GetValues(typeof(Selector)))
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
                root.IsHidden = !root.IsHidden;
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
