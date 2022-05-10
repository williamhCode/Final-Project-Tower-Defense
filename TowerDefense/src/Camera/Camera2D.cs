using Microsoft.Xna.Framework;

using System;

using MonoGame.Extended;

namespace TowerDefense.Camera
{
    public class Camera2D
    {
        public float Width { get; private set; }
        public float Height { get; private set; }
        /// <summary>
        /// The screen's position (topleft) in world space.
        /// </summary>
        public Vector2 Pan { get; set; }

        private float _zoom;
        public float Zoom 
        { 
            get { return _zoom; }
            // set { _zoom = Math.Max(1, value); }
            set { _zoom = value; }
        }

        private readonly Vector2 halfScreenSize;

        public Camera2D(float width, float height)
        {
            Width = width;
            Height = height;
            Pan = Vector2.Zero;
            Zoom = 1;
            halfScreenSize = new Vector2(width / 2, height / 2);
        }
        
        public RectangleF GetViewport()
        {
            return new RectangleF(Pan.X, Pan.Y, Width / Zoom, Height / Zoom);
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateTranslation(new Vector3(-Pan, 0.0f)) *
                   Matrix.CreateScale(Zoom, Zoom, 1);
        }

        public void LookAt(Vector2 position)
        {
            Pan = position - halfScreenSize / Zoom;
        }

        public void Move(Vector2 amount)
        {
            Pan += amount;
        }

        public void ZoomFromCenter(float scale)
        {
            var center = Pan + halfScreenSize / Zoom;
            Zoom *= scale;
            LookAt(center);
        }

        public Vector2 WorldToScreen(Vector2 mouseCoords)
        {
            return Vector2.Transform(mouseCoords, GetTransform());
        }
    
        public Vector2 ScreenToWorld(Vector2 screenCoords)
        {
            return Vector2.Transform(screenCoords, Matrix.Invert(GetTransform()));
        }
    }
}