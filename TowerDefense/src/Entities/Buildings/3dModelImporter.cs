using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace TowerDefense.Entities.Buildings
{

    public class Importer : Game
    {
        GraphicsDeviceManager graphics;
        

        //Camera
       
        Matrix projectionMatrix;
        
        Matrix worldMatrix;

        //Geometric info
        Model model;

        //Orbit
        bool orbit = false;

        public Importer()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content/Models";
        }

        protected override void Initialize()
        {
            base.Initialize();

            //Setup Camera
           
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                               MathHelper.ToRadians(45f), graphics.
                               GraphicsDevice.Viewport.AspectRatio,
                1f, 1000f);
            Vector3 mid =new Vector3(0,0,0);
            worldMatrix = Matrix.CreateWorld(mid, Vector3.
                          Forward, Vector3.Up);

            model = Content.Load<Model>("BuffingTower");
        }

        protected override void LoadContent()
        {
            
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
           

            
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach(ModelMesh mesh in model.Meshes)
            {
                foreach(BasicEffect effect in mesh.Effects)
                {
                    //effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(1f, 0, 0);
                    effect.View = worldMatrix;
                    effect.World = worldMatrix;
                    effect.Projection = projectionMatrix;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}