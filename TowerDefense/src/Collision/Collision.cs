using Microsoft.Xna.Framework;
using System;

namespace Collision
{
    public class CollisionFuncs
    {
        private delegate bool ShapeVsShape(Shape shape1, Shape shape2, ref Vector2 mtv);

        private static ShapeVsShape[,] collisionFunctions = new ShapeVsShape[,]
        {
            { CircleVsCircle, CircleVsPolygon },
            { PolygonVsCircle, PolygonVsPolygon }
        };

        ///<summary>
        ///Checks if two shapes are colliding.
        ///</summary>
        public static bool IsColliding(Shape shape1, Shape shape2)
        {
            Vector2 mtv = Vector2.Zero;
            return collisionFunctions[(int)shape1.GetShapeType(), (int)shape2.GetShapeType()](shape1, shape2, ref mtv);
        }

        ///<summary>
        ///Checks if two shapes are colliding, with mtv as the minimum translation vector (shape2 -> shape1).
        ///</summary>
        public static bool IsColliding(Shape shape1, Shape shape2, out Vector2 mtv)
        {
            mtv = Vector2.Zero;
            return collisionFunctions[(int)shape1.GetShapeType(), (int)shape2.GetShapeType()](shape1, shape2, ref mtv);
        }

        private static bool CircleVsCircle(Shape shape1, Shape shape2, ref Vector2 mtv)
        {
            Circle circle1 = (Circle)shape1;
            Circle circle2 = (Circle)shape2;

            float r1 = circle1.Radius;
            float r2 = circle2.Radius;

            Vector2 diff = circle2.Position - circle1.Position;
            float dist = diff.Length();
            float sep = r1 + r2 - dist;

            if (sep > 0)
                return false;

            Vector2 normal;
            if (dist == 0)
                normal = new Vector2(1, 0);
            else
                normal = diff / dist;
            // minimum translation vector
            mtv = normal * Math.Abs(sep);

            return true;
        }

        private static bool CircleVsPolygon(Shape shape1, Shape shape2, ref Vector2 mtv)
        {
            Circle circle = (Circle)shape1;
            Polygon polygon = (Polygon)shape2;
            
            float min_dist = float.PositiveInfinity;
            Vector2 closest_vertex = Vector2.Zero;
            foreach (Vector2 vertex in polygon.Vertices)
            {
                float dist = (vertex - circle.Position).Length();

                if (dist < min_dist)
                {
                    min_dist = dist;
                    closest_vertex = vertex;
                }
            }

            Vector2 normal = Vector2.Normalize(closest_vertex - circle.Position);

            (float poly_min, float poly_max) = ProjVertsOnAxis(polygon.Vertices, normal);

            float circle_center = Vector2.Dot(circle.Position, normal);
            float circle_min = circle_center - circle.Radius;
            float circle_max = circle_center + circle.Radius;

            if (circle_min > poly_max || circle_max < poly_min)
                return false;

            float min_sep = poly_min - circle_max;
            Vector2 min_axis = normal;

            for (int i = 0; i < polygon.Vertices.Length; i++)
            {
                Vector2 v1 = polygon.Vertices[i];
                Vector2 v2 = polygon.Vertices[(i + 1) % polygon.Vertices.Length];

                normal = Vector2.Normalize(new Vector2(-(v2.Y - v1.Y), v2.X - v1.X));
                float sep = Vector2.Dot(v1 - circle.Position, normal) - circle.Radius;

                if (sep > 0)
                    return false;
                
                if (sep > min_sep)
                {
                    min_sep = sep;
                    min_axis = normal;
                }
            }
            
            mtv = min_axis * Math.Abs(min_sep);

            return true;
        }

        private static bool PolygonVsCircle(Shape shape1, Shape shape2, ref Vector2 mtv)
        {
            bool collided = CircleVsPolygon(shape2, shape1, ref mtv);
            mtv = -mtv;

            return collided;
        }

        private static bool PolygonVsPolygon(Shape shape1, Shape shape2, ref Vector2 mtv)
        {
            Polygon polygon1 = (Polygon)shape1;
            Polygon polygon2 = (Polygon)shape2;
            return false;
        }

        private static (float min_proj, float max_proj) ProjVertsOnAxis(Vector2[] vertices, Vector2 axis)
        {
            float min_proj = float.PositiveInfinity;
            float max_proj = float.NegativeInfinity;

            foreach (Vector2 vertex in vertices)
            {
                float proj = Vector2.Dot(vertex, axis);
                min_proj = Math.Min(min_proj, proj);
                max_proj = Math.Max(max_proj, proj);
            }

            return (min_proj, max_proj);
        }
    }
}