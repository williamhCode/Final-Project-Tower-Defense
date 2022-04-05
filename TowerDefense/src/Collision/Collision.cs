using Microsoft.Xna.Framework;
using System;
using static TowerDefense.Maths.MathFuncs;

namespace TowerDefense.Collision
{
    public static class CollisionFuncs
    {
        /// <summary>
        /// Check if two lines are intersecting + returns the intersection point.
        /// </summary>
        public static bool IsIntersecting(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2? intersection)
        {
            intersection = null;

            Vector2 p12 = p2 - p1;
            Vector2 p34 = p4 - p3;
            Vector2 p13 = p3 - p1;

            float d = Cross(p12, p34);
            if (d == 0)
                return false;
            float u = Cross(p13, p34) / d;
            float v = Cross(p13, p12) / d;

            if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
            {
                intersection = p1 + u * p12;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a polygon is intersecting with a line + return the closest intersection point.
        /// </summary>
        public static bool IsColliding(
            CPolygon poly, Vector2 start, Vector2 end, out (float sqdist, Vector2 intersection, Vector2 normal)? collData)
        {
            collData = null;
            float minDistSq = float.MaxValue;
            Vector2? minIntersection = null;
            Vector2? normal = null;

            for (int i = 0; i < poly.Vertices.Length; i++)
            {
                Vector2 p1 = poly.Vertices[i];
                Vector2 p2 = poly.Vertices[(i + 1) % poly.Vertices.Length];
                
                if (IsIntersecting(p1, p2, start, end, out Vector2? intersection))
                {
                    float sqdist = Vector2.DistanceSquared(start, intersection.Value);
                    if (sqdist < minDistSq)
                    {
                        minDistSq = sqdist;
                        minIntersection = intersection;
                        Vector2 p12 = p2 - p1;
                        normal = Vector2.Normalize(new Vector2(p12.Y, -p12.X));
                    }
                }
            }

            if (minDistSq < float.MaxValue)
            {
                collData = (minDistSq, minIntersection.Value, normal.Value);
                return true;
            }

            return false;
        }

        private delegate bool ShapeVsShape(CShape shape1, CShape shape2, ref Vector2 mtv, bool computeMtv);

        private static ShapeVsShape[,] collisionFunctions = new ShapeVsShape[,]
        {
            { CircleVsCircle, CircleVsPolygon },
            { PolygonVsCircle, PolygonVsPolygon }
        };

        /// <summary>
        /// Checks if two shapes are colliding.
        /// </summary>
        public static bool IsColliding(CShape shape1, CShape shape2)
        {
            Vector2 mtv = Vector2.Zero;
            return collisionFunctions[(int)shape1.GetShapeType(), (int)shape2.GetShapeType()](shape1, shape2, ref mtv, false);
        }

        /// <summary>
        /// Checks if two shapes are colliding, with mtv as the minimum translation vector (from shape1 -> shape2).
        /// </summary>
        public static bool IsColliding(CShape shape1, CShape shape2, out Vector2 mtv)
        {
            mtv = Vector2.Zero;
            return collisionFunctions[(int)shape1.GetShapeType(), (int)shape2.GetShapeType()](shape1, shape2, ref mtv, true);
        }

        private static bool CircleVsCircle(CShape shape1, CShape shape2, ref Vector2 mtv, bool computeMtv)
        {
            CCircle circle1 = (CCircle)shape1;
            CCircle circle2 = (CCircle)shape2;

            float r1 = circle1.Radius;
            float r2 = circle2.Radius;

            Vector2 diff = circle2.Position - circle1.Position;
            float dist = diff.Length();
            float overlap = r1 + r2 - dist;

            if (overlap <= 0)
                return false;

            if (computeMtv)
            {
                Vector2 normal;
                if (dist == 0)
                    normal = new Vector2(1, 0);
                else
                    normal = diff / dist;

                    mtv = normal * overlap;
            }

            return true;
        }

        private static bool CircleVsPolygon(CShape shape1, CShape shape2, ref Vector2 mtv, bool computeMtv)
        {
            CCircle circle = (CCircle)shape1;
            CPolygon polygon = (CPolygon)shape2;
            
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
            
            if (computeMtv)
                mtv = min_axis * min_overlap;

            return true;
        }

        private static bool PolygonVsCircle(CShape shape1, CShape shape2, ref Vector2 mtv, bool computeMtv)
        {
            bool collided = CircleVsPolygon(shape2, shape1, ref mtv, computeMtv);
            mtv = -mtv;

            return collided;
        }

        private static bool PolygonVsPolygon(CShape shape1, CShape shape2, ref Vector2 mtv, bool computeMtv)
        {
            CPolygon polygon1 = (CPolygon)shape1;
            CPolygon polygon2 = (CPolygon)shape2;

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

            if (computeMtv)
            {
                float poly1_min = MinProjVertsOnAxis(polygon1.Vertices, min_axis);
                float poly2_min = MinProjVertsOnAxis(polygon2.Vertices, min_axis);

                if (poly1_min > poly2_min)
                    min_overlap = -min_overlap;
                
                mtv = min_axis * min_overlap;
            }

            return true;
        }

        private static float MinProjVertsOnAxis(Vector2[] vertices, Vector2 axis)
        {
            float min_proj = float.PositiveInfinity;

            foreach (Vector2 vertex in vertices)
            {
                float proj = Vector2.Dot(vertex, axis);
                min_proj = Math.Min(min_proj, proj);
            }

            return min_proj;
        }

        private static (float, float) ProjVertsOnAxis(Vector2[] vertices, Vector2 axis)
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