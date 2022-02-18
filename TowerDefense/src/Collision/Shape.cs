using Microsoft.Xna.Framework;

using System;
using System.Linq;

namespace Collision
{
    public interface Shape
    {
        public enum ShapeType
        {
            Circle,
            Polygon
        }

        ShapeType GetShapeType();
    }

    public class Circle : Shape
    {
        public Vector2 Position { get; set; }
        public float Radius { get; set; }

        public Circle(float radius, Vector2 position)
        {
            Radius = radius;
            Position = position;
        }

        public Shape.ShapeType GetShapeType()
        {
            return Shape.ShapeType.Circle;
        }
    }

    public class Polygon : Shape
    {
        private Vector2 position;
        public Vector2 Position
        { 
            get
            {
                return position;
            }
            set
            {
                if (value != position)
                {
                    position = value;
                    UpdateVertices();
                }
            }
        }
        private Vector2[] orgVertices;
        public Vector2[] Vertices { get; set; }
        public Vector2[] Normals { get; set; }
        
        public Polygon(Vector2[] vertices, Vector2 position)
        {
            orgVertices = vertices.OrderBy(vertex => Math.Atan2(vertex.Y, vertex.X)).ToArray();
            Vertices = new Vector2[orgVertices.Length];
            this.position = position;
            UpdateVertices();

            Normals = new Vector2[orgVertices.Length];
            for (int i = 0; i < orgVertices.Length; i++)
            {
                Vector2 v1 = orgVertices[i];
                Vector2 v2 = orgVertices[(i + 1) % orgVertices.Length];
                Vector2 normal = new Vector2(v1.Y - v2.Y, v2.X - v1.X);
                normal.Normalize();
                Normals[i] = normal;
            }
        }

        private void UpdateVertices()
        {
            for (int i = 0; i < orgVertices.Length; i++)
            {
                Vertices[i] = orgVertices[i] + Position;
            }
        }

        public Shape.ShapeType GetShapeType()
        {
            return Shape.ShapeType.Polygon;
        }
    }
}