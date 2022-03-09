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
using TowerDefense.Camera;
using TowerDefense.Entities;

namespace TowerDefense
{
    public class Game1 : MlemGame
    {
        // constants
        public static Game1 Instance {get; private set;}
        public SpriteFont font;
        private Camera2D camera;
        private Player player;
        private Panel root;
        

        public Game1()
        {
            Instance = this;
            this.IsMouseVisible = true;   
        }

        protected override void Initialize()
        {
            base.Initialize();
            camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            player = new Player(new Vector2(300, 300));
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

            this.UiSystem.AutoScaleReferenceSize = new Point(1280,720);
            this.UiSystem.AutoScaleWithScreen = true;
            this.UiSystem.GlobalScale = 5;

            font = Content.Load<SpriteFont>("Font/Frame");
            Player.LoadContent(Content);


            var style = new UntexturedStyle(this.SpriteBatch)
            {
                Font = new GenericSpriteFont(LoadContent<SpriteFont>("Font/Frame")),
            };

            this.UiSystem.Style = style;

            this.root = new Panel(Anchor.Center, new Vector2(100,100), Vector2.Zero, false, true);
            this.root.ScrollBar.SmoothScrolling = true;
            this.UiSystem.Add("TestUi", this.root);

            var box = new Panel(Anchor.Center, new Vector2(100,1), Vector2.Zero, setHeightBasedOnChildren: true);
            box.AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is some example text"));
            box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 20), "Okay") 
            {
                OnPressed = close => this.UiSystem.Remove("InfoBox"),
                PositionOffset = new Vector2(0, 1)
            });
            this.UiSystem.Add("InfoBox", box);
        }
        
        protected override void DoUpdate(GameTime gameTime)
        {
            base.DoUpdate(gameTime);

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

            this.UiSystem.Update(gameTime);
        }

        protected override void DoDraw(GameTime gameTime)
        {
            this.UiSystem.DrawEarly(gameTime, this.SpriteBatch);

            float frameRate = 1 / gameTime.GetElapsedSeconds();
            
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Drawing the player
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, rasterizerState: RasterizerState.CullNone, transformMatrix: camera.Transform, blendState: BlendState.AlphaBlend);
            player.Draw(SpriteBatch);
            SpriteBatch.End();

            // Drawing the Text
            SpriteBatch.Begin();
            SpriteBatch.DrawString(font, $"Frame Rate: {frameRate:N2}", new Vector2(10, 10), Color.Black);
            SpriteBatch.End();

            base.DoDraw(gameTime);
            this.UiSystem.Draw(gameTime, this.SpriteBatch);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

    }
}
