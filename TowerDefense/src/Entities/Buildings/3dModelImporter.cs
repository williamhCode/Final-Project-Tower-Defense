 
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
 

 
namespace TowerDefense.Entities.Buildings
{
    
    public class Importer : Game
    {
        GraphicsDeviceManager graphics;
        
        
        private Model model;
        private Matrix world = Matrix.CreateTranslation(new Vector3(0, 0, 0));
        BasicEffect be;
        private Matrix point=Matrix.CreateTranslation(new Vector3(0,0,0));
        public Importer (string towerChoice,int x,int y)
        {
                        
              
        }          
 
        void LoadContent (ContentManager Content)
        {            
            Content = new ContentManager (this.Services, "Content/Models/BuffingTower");
 
             
            model = Content.Load<Model> ("BuffingTower");

        }
 
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
         void Update (GameTime gameTime)
        {
             
            world = Matrix.CreateRotationY ((float)gameTime.TotalGameTime.TotalSeconds);
 
            base.Update (gameTime);
        }
 
      
        protected override void Draw (GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear (Color.CornflowerBlue);
 
            DrawModel(model, world, point, point);
 
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