using Microsoft.Xna.Framework;

namespace TowerDefense.Camera
{
    public class Camera2D
    {
        private Vector2 offset;
        Matrix _baseTransform;

        public Vector2 Pan { get; set; }
        public float Zoom { get; set; }

        public Camera2D(float width, float height)
        {
            offset = new Vector2(width / 2, height / 2);

            _baseTransform = Matrix.Identity;
            _baseTransform *= Matrix.CreateScale(1, -1, 1);

            Pan = Vector2.Zero;
            Zoom = 1;
        }

        public Matrix getTransform()
        {
            Vector2 translation = offset + Pan;
            return _baseTransform * Matrix.CreateScale(Zoom, Zoom, 1) * Matrix.CreateTranslation(translation.X, translation.Y, 0);
        }

        // public Vector2 MouseToScreen(Vector2 mouseCoords)
        // {
            
        // }
    
        // public Vector2 ScreenToMouse(Vector2 screenCoords)
        // {
            
        // }
    }
}