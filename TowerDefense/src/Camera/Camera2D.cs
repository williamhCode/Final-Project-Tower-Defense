using Microsoft.Xna.Framework;

using System;

namespace TowerDefense.Camera
{
    public class Camera2D
    {
        private Matrix baseTransform;

        public Matrix Transform { get; private set; }
        public Vector2 Pan { get; set; }

        private float _zoom;
        public float Zoom 
        { 
            get { return _zoom; }
            set { _zoom = Math.Max(2, value); }
        }

        public Camera2D(float width, float height)
        {
            baseTransform = Matrix.Identity;
            baseTransform *= Matrix.CreateScale(1, 1, 1);

            Pan = Vector2.Zero;
            Zoom = 2;
        }

        public void Update()
        {
            Transform = baseTransform * Matrix.CreateScale(Zoom, Zoom, 1) * Matrix.CreateTranslation(Pan.X, Pan.Y, 0);
        }

        // public Vector2 ScreenToMouse(Vector2 mouseCoords)
        // {
            
        // }
    
        public Vector2 MouseToScreen(Vector2 screenCoords)
        {
            return Vector2.Transform(screenCoords, Matrix.Invert(Transform));
        }
    }
}