 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
 

 
namespace TowerDefense.Entities.Buildings
{
    
    public class Importer
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        private Model model;
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        private Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 10), new Vector3(0, 0, 0), Vector3.UnitY);
        private Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800/480f, 0.1f, 100f);
 
        public Importer (string towerChoice)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = towerChoice;                
              
        }
 
        
        protected override void Initialize ()
        {
            // TODO: Add your initialization logic here
            base.Initialize ();
        }
 
        protected override void LoadContent (ContentManager Content)
        {            
            Content = new ContentManager (this.Services, "Content/Models/BuffingTower");
 
             
            model = Content.Load<Model> ("BuffingTower");
        }
 
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update (GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            if (Keyboard.GetState ().IsKeyDown (Keys.Escape)) {
                Exit ();
            }
 
            world = Matrix.CreateRotationY ((float)gameTime.TotalGameTime.TotalSeconds);
 
            base.Update (gameTime);
        }
 
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw (GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
 
            DrawModel(model, world, view, projection);
 
            base.Draw (gameTime);
        }
 
        private void DrawModel(Model model, Matrix world, Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;
                }
 
                mesh.Draw();
            }
        }
    }
}