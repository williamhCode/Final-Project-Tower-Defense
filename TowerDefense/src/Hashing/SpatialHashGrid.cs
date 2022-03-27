using System;
using System.Collections.Generic;
using TowerDefense.Entities;
using Microsoft.Xna.Framework;

using TowerDefense.Collision;
using System.Linq;

namespace TowerDefense.Hashing
{
    public class SpatialHashGrid
    {
        public float CellSize { get; set; }
        public Dictionary<string, List<Entity>> HashTable;

        public SpatialHashGrid(int cellSize)
        {
            CellSize = cellSize;
            HashTable = new Dictionary<string, List<Entity>>();
        }

        private string PositionToKey(Vector2 position)
        {
            return (int)MathF.Floor(position.X / CellSize) + "," + (int)MathF.Floor(position.Y / CellSize);
        }

        private void SafeAdd(Entity entity, string key)
        {
            if (!HashTable.ContainsKey(key))
                HashTable.Add(key, new List<Entity>());
            HashTable[key].Add(entity); 
        }

        public void RemoveEntityPosition(Entity entity)
        {
            string key = PositionToKey(entity.Position);
            if (HashTable.ContainsKey(key))
            {
                HashTable[key].Remove(entity);
                if (HashTable[key].Count == 0)
                    HashTable.Remove(key);
            }
        }

        public void RemoveEntityCShape(Entity entity)
        {
            var (minX, maxX, minY, maxY) = GetCShapeRanges(entity.CShape);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    string key = x + "," + y;
                    if (HashTable.ContainsKey(key))
                    {
                        HashTable[key].Remove(entity);
                        if (HashTable[key].Count == 0)
                            HashTable.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Adds an entity to the grid using a position.
        /// </summary>
        public void AddEntity(Entity entity, Vector2 position)
        {
            var key = PositionToKey(position);
            SafeAdd(entity, key);
        }

        /// <summary>
        /// Adds an entity to the grid using a collision shape.
        /// </summary>
        public void AddEntity(Entity entity, CShape cShape)
        {
            var (minX, maxX, minY, maxY) = GetCShapeRanges(cShape);

            // var keys = new List<string>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    string key = x + "," + y;
                    SafeAdd(entity, key);
                    // keys.Add(key);
                }
            }

            // entity.CShape.Keys = keys;
        }

        /// <summary>
        /// Queries entities in the grid using a position.
        /// </summary>
        public List<Entity> QueryEntities(Vector2 position)
        {
            var key = PositionToKey(position);
            if (HashTable.ContainsKey(key))
                return HashTable[key];
            else
                return new List<Entity>();
        }

        /// <summary>
        /// Queries entities in the grid using a position and range.
        /// </summary>
        public List<Entity> QueryEntities(Vector2 position, float range)
        {
            var entities = new List<Entity>();
            var (minX, maxX, minY, maxY) = GetRanges(position, range, range);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    string key = x + "," + y;
                    if (HashTable.ContainsKey(key))
                        entities.AddRange(HashTable[key].Except(entities));
                }
            }

            return entities;
        }

        /// <summary>
        /// Queries entities in the grid using a collision shape.
        /// </summary>
        public List<Entity> QueryEntities(CShape cShape)
        {
            var entities = new List<Entity>();
            var (minX, maxX, minY, maxY) = GetCShapeRanges(cShape);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    string key = x + "," + y;
                    if (HashTable.ContainsKey(key))
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
            HashTable.Clear();
        }

        /// <summary>
        /// Combines two grids.
        /// </summary>
        public void Append(SpatialHashGrid other)
        {
            foreach (var key in other.HashTable.Keys)
            {
                if (!HashTable.ContainsKey(key))
                    HashTable.Add(key, new List<Entity>());
                HashTable[key].AddRange(other.HashTable[key]);
            }
        }
    }
}