using Microsoft.Xna.Framework;
using System;

namespace Collision
{
    public static class CollisionFuncs
    {
        private delegate bool ShapeVsShape(Shape shape1, Shape shape2, ref Vector2 mtv);

        private static ShapeVsShape[,] collisionFunctions = new ShapeVsShape[,]
        {
            { CircleVsCircle, CircleVsPolygon },
            { PolygonVsCircle, PolygonVsPolygon }
        };

        /// <summary>
        /// Checks if two shapes are colliding.
        /// </summary>
        public static bool IsColliding(Shape shape1, Shape shape2)
        {
            Vector2 mtv = Vector2.Zero;
            return collisionFunctions[(int)shape1.GetShapeType(), (int)shape2.GetShapeType()](shape1, shape2, ref mtv);
        }

        /// <summary>
        /// Checks if two shapes are colliding, with mtv as the minimum translation vector (from shape1 -> shape2).
        /// </summary>
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
            float overlap = r1 + r2 - dist;

            if (overlap <= 0)
                return false;

            Vector2 normal;
            if (dist == 0)
                normal = new Vector2(1, 0);
            else
                normal = diff / dist;
            // minimum translation vector
            mtv = normal * overlap;

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

            float min_overlap = circle_max - poly_min;
            Vector2 min_axis = normal;

            for (int i = 0; i < polygon.Vertices.Length; i++)
            {
                Vector2 v1 = polygon.Vertices[i];
                Vector2 v2 = polygon.Vertices[(i + 1) % polygon.Vertices.Length];

                normal = Vector2.Normalize(new Vector2(-(v2.Y - v1.Y), v2.X - v1.X));
                float overlap = circle.Radius - Vector2.Dot(v1 - circle.Position, normal);

                if (overlap <= 0)
                    return false;
                
                if (overlap < min_overlap)
                {
                    min_overlap = overlap;
                    min_axis = normal;
                }
            }
            
            mtv = min_axis * min_overlap;

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

            var p1 = polygon1;
            var p2 = polygon2;

            float min_overlap = float.PositiveInfinity;
            Vector2 min_axis = Vector2.Zero;

            for (int shapes = 0; shapes < 2; shapes++)
            {
                if (shapes == 1)
                {
                    p1 = polygon2;
                    p2 = polygon1;
                }

                for (int i = 0; i < p1.Vertices.Length; i++)
                {
                    Vector2 v1 = p1.Vertices[i];
                    Vector2 v2 = p1.Vertices[(i + 1) % p1.Vertices.Length];
                    
                    Vector2 normal = Vector2.Normalize(new Vector2(v2.Y - v1.Y, -(v2.X - v1.X)));

                    (float p1_min, float p1_max) = ProjVertsOnAxis(p1.Vertices, normal);
                    (float p2_min, float p2_max) = ProjVertsOnAxis(p2.Vertices, normal);
                    
                    if (p1_min > p2_max || p2_max < p1_min)
                        return false;

                    float overlap = Math.Min(p1_max, p2_max) - Math.Max(p1_min, p2_min);

                    if (overlap < min_overlap)
                    {
                        min_overlap = overlap;
                        min_axis = normal;
                    }
                } 
            }

            (float poly1_min, float poly1_max) = ProjVertsOnAxis(polygon1.Vertices, min_axis);
            (float poly2_min, float poly2_max) = ProjVertsOnAxis(polygon2.Vertices, min_axis);

            if (poly1_min > poly2_min)
                min_overlap = -min_overlap;
            
            mtv = min_axis * min_overlap;

            return true;
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