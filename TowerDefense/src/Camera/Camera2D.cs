using Microsoft.Xna.Framework;

using System;
using System.Diagnostics;

namespace TowerDefense.Camera
{
    public class Camera2D
    {
        public float Width { get; set; }
        public float Height { get; set; }
        /// <summary>
        /// The screen's position (topleft) in world space.
        /// </summary>
        public Vector2 Pan { get; set; }

        private float _zoom;
        public float Zoom 
        { 
            get { return _zoom; }
            set { _zoom = Math.Max(2, value); }
        }

        public Camera2D(float width, float height)
        {
            Width = width;
            Height = height;
            Pan = Vector2.Zero;
            Zoom = 2;
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateTranslation(new Vector3(-Pan, 0.0f)) *
                   Matrix.CreateScale(Zoom, Zoom, 1);
        }

        public void LookAt(Vector2 position)
        {
            Pan = position - new Vector2(Width / 2, Height / 2) / Zoom;
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