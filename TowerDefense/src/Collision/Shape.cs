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
        public float Radius { get; }

        public Circle(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public Shape.ShapeType GetShapeType()
        {
            return Shape.ShapeType.Circle;
        }
    }

    public class Polygon : Shape
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        private Vector2[] orgVertices;
        public Vector2[] Vertices { get; private set; }

        /// <summary>
        /// Note: Vertices must be in counter-clockwise order.
        /// </summary>
        public Polygon(Vector2 position, Vector2[] vertices, float rotation = 0)
        {
            Position = position;
            Rotation = rotation;

            orgVertices = vertices;
            Vertices = new Vector2[orgVertices.Length];
            UpdateVertices();
        }

        public void UpdateVertices()
        {
            Matrix transform = Matrix.CreateRotationZ((float)Math.PI / 180 * Rotation) * Matrix.CreateTranslation(Position.X, Position.Y, 0);
            Vector2.Transform(orgVertices, ref transform, Vertices);
        }

        /// <summary>
        /// Returns a counter-clockwise sorted array of the input vertices.
        /// </summary>
        public static Vector2[] OrderCounterClockwise(Vector2[] vertices)
        {
            Vector2 centroid = vertices.Aggregate((a, b) => a + b) / vertices.Length;
            Vector2[] orderedVertices = vertices.OrderBy(v => Math.Atan2(v.Y - centroid.Y, v.X - centroid.X)).ToArray();
            return orderedVertices;
        }

        public Shape.ShapeType GetShapeType()
        {
            return Shape.ShapeType.Polygon;
        }
    }

    public class Rectangle : Polygon
    {
        public Rectangle(Vector2 position, float width, float height, float rotation = 0)
            : base(position, new Vector2[]
            {
                new Vector2(-width / 2, -height / 2),
                new Vector2(width / 2, -height / 2),
                new Vector2(width / 2, height / 2),
                new Vector2(-width / 2, height / 2)
            }, rotation)
        { }
    }
}