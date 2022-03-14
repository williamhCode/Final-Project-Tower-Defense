using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;

using System;
using System.Linq;
using System.Collections.Generic;

namespace TowerDefense.Collision
{
    public abstract class CShape
    {
        public List<string> Keys { get; set; }

        public enum ShapeType
        {
            Circle,
            Polygon
        }

        public Vector2 Position { get; set; }

        public abstract void Draw(SpriteBatch spriteBatch, Color color, float thickness);

        public abstract ShapeType GetShapeType();

        public abstract void Update();
    }

    public class CCircle : CShape
    {
        public float Radius { get; }

        public CCircle(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public override void Draw(SpriteBatch spriteBatch, Color color, float thickness)
        {
            spriteBatch.DrawCircle(Position, Radius, 20, color, thickness);
        }

        public override CShape.ShapeType GetShapeType()
        {
            return CShape.ShapeType.Circle;
        }

        public override void Update()
        {
            // do nothing
        }
    }

    public class CPolygon : CShape
    {
        public float Rotation { get; set; }
        private Vector2[] _orgVertices;
        public Vector2[] Vertices { get; private set; }

        /// <summary>
        /// Note: Vertices must be in counter-clockwise order.
        /// </summary>
        public CPolygon(Vector2 position, Vector2[] vertices, float rotation = 0)
        {
            Position = position;
            Rotation = rotation;

            _orgVertices = vertices;
            Vertices = new Vector2[_orgVertices.Length];
            Update();
        }

        public override void Update()
        {
            Matrix transform = Matrix.CreateRotationZ(MathF.PI / 180 * Rotation) * Matrix.CreateTranslation(Position.X, Position.Y, 0);
            Vector2.Transform(_orgVertices, ref transform, Vertices);
        }

        public override void Draw(SpriteBatch spriteBatch, Color color, float thickness)
        {
            spriteBatch.DrawPolygon(Vector2.Zero, Vertices, color, thickness);
        }

        public override CShape.ShapeType GetShapeType()
        {
            return CShape.ShapeType.Polygon;
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
    }

    public class CRectangle : CPolygon
    {
        public CRectangle(Vector2 position, float width, float height, float rotation = 0)
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