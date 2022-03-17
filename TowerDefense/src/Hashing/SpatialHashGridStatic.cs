using System;
using System.Collections.Generic;
using TowerDefense.Entities;
using Microsoft.Xna.Framework;

using TowerDefense.Collision;
using System.Linq;

namespace TowerDefense.Hashing
{
    public class SpatialHashGridStatic
    {
        public float CellSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<Entity>[] HashTable;

        private float convFactor;

        public SpatialHashGridStatic(int cellSize, int width, int height)
        {
            CellSize = cellSize;
            Width = width;
            Height = height;
            HashTable = new List<Entity>[width * height];
            for (int i = 0; i < HashTable.Length; i++)
                HashTable[i] = new List<Entity>();

            convFactor = 1 / CellSize;
        }

        private int PositionToKey(Vector2 position)
        {
            int gridCell = (int)(position.X * convFactor) + (int)(position.Y * convFactor) * Width;
            return gridCell;
        }

        private int PositionToKey(int x, int y)
        {
            int gridCell = x + y * Width;
            return gridCell;
        }

        /// <summary>
        /// Adds an entity to the grid using its position.
        /// </summary>
        public void AddEntityPosition(Entity entity)
        {
            var key = PositionToKey(entity.Position);
            HashTable[key].Add(entity);
        }

        /// <summary>
        /// Adds an entity to the grid using its collision shape.
        /// </summary>
        public void AddEntityCShape(Entity entity)
        {
            CShape cShape = entity.CShape;

            var (minX, maxX, minY, maxY) = GetCShapeRanges(cShape);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var key = PositionToKey(x, y);
                    HashTable[key].Add(entity);
                }
            }
        }

        /// <summary>
        /// Queries entities in the grid using a position.
        /// </summary>
        public List<Entity> QueryEntitiesPosition(Vector2 position)
        {
            var key = PositionToKey(position);
            return HashTable[key];
        }

        /// <summary>
        /// Queries entities in the grid using a position and range.
        /// </summary>
        public List<Entity> QueryEntitiesRange(Vector2 position, float range)
        {
            var entities = new List<Entity>();
            var (minX, maxX, minY, maxY) = GetRanges(position, range, range);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var key = PositionToKey(x, y);
                    entities.AddRange(HashTable[key].Except(entities));
                }
            }

            return entities;
        }

        /// <summary>
        /// Queries entities in the grid using a collision shape.
        /// </summary>
        public List<Entity> QueryEntitiesCShape(CShape cShape)
        {
            var entities = new List<Entity>();
            var (minX, maxX, minY, maxY) = GetCShapeRanges(cShape);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var key = PositionToKey(x, y);
                    entities.AddRange(HashTable[key].Except(entities));
                }
            }
            return entities;
        }

        /// <summary>
        /// Note: only use if the CShape.Keys is already set.
        /// </summary>
        // public List<Entity> QueryEntitiesCollision(Entity entity)
        // {
        //     var entities = new List<Entity>();
        //     foreach (var key in entity.CShape.Keys)
        //     {
        //         entities.AddRange(HashTable[key].Except(entities));
        //     }
        //     entities.Remove(entity);
        //     Console.WriteLine(string.Join(", ", entities));
        //     return entities;
        // }

        /// <summary>
        /// Note: width and height are halved.
        /// </summary>
        private (int, int, int, int) GetRanges(Vector2 position, float width, float height)
        {
            int minX = (int)MathF.Floor((position.X - width) / CellSize);
            int maxX = (int)MathF.Floor((position.X + width) / CellSize);
            int minY = (int)MathF.Floor((position.Y - height) / CellSize);
            int maxY = (int)MathF.Floor((position.Y + height) / CellSize);
            return (minX, maxX, minY, maxY);
        }

        private (int, int, int, int) GetCShapeRanges(CShape cShape)
        {
            int minX, maxX, minY, maxY;

            if (cShape is CCircle)
            {
                CCircle cCircle = (CCircle)cShape;
                (minX, maxX, minY, maxY) = GetCCircleRanges(cCircle);
            }
            else if (cShape is CRectangle)
            {
                CRectangle cRectangle = (CRectangle)cShape;
                (minX, maxX, minY, maxY) = GetCRectangleRanges(cRectangle);
            }
            else
            {
                throw new Exception("Bad CShape type");
            }

            return (minX, maxX, minY, maxY);
        }

        private (int, int, int, int) GetCCircleRanges(CCircle cCircle)
        {
            return GetRanges(cCircle.Position, cCircle.Radius, cCircle.Radius);
        }

        private (int, int, int, int) GetCRectangleRanges(CRectangle cRectangle)
        {
            var topLeft = cRectangle.Vertices[0];
            var bottomRight = cRectangle.Vertices[2];
            return (
                (int)MathF.Floor(topLeft.X / CellSize),
                (int)MathF.Floor(bottomRight.X / CellSize),
                (int)MathF.Floor(topLeft.Y / CellSize),
                (int)MathF.Floor(bottomRight.Y / CellSize)
            );
        }

        /// <summary>
        /// Clears the grid.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < HashTable.Length; i++)
                HashTable[i] = new List<Entity>();
        }
    }
}