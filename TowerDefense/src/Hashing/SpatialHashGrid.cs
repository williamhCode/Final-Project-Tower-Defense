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

        public string PositionToKey(Vector2 position)
        {
            return (int)MathF.Floor(position.X / CellSize) + "," + (int)MathF.Floor(position.Y / CellSize);
        }

        public void SafeAdd(Entity entity, string key)
        {
            if (!HashTable.ContainsKey(key))
                HashTable.Add(key, new List<Entity>());
            HashTable[key].Add(entity); 
        }

        public void AddEntity(Entity entity)
        {
            var key = PositionToKey(entity.Position);
            SafeAdd(entity, key);
        }

        public void AddEntityCollision(Entity entity)
        {
            CShape cShape = entity.CShape;

            int minX, maxX, minY, maxY;

            if (cShape is CCircle)
            {
                CCircle circle = (CCircle)cShape;
                float width, height;
                width = height = circle.Radius * 2;
                (minX, maxX, minY, maxY) = GetRanges(circle.Position, width, height);
                
            }
            else if (cShape is CRectangle)
            {
                CRectangle rectangle = (CRectangle)cShape;
                (minX, maxX, minY, maxY) = GetCRectangleRanges(rectangle);
            }
            else
            {
                throw new Exception("Bad CShape type");
            }

            var keys = new List<string>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    string key = x + "," + y;
                    SafeAdd(entity, key);
                    keys.Add(key);
                }
            }

            entity.CShape.Keys = keys;
        }

        public List<Entity> QueryEntitiesPosition(Vector2 position)
        {
            var key = PositionToKey(position);
            if (HashTable.ContainsKey(key))
                return HashTable[key];
            else
                return new List<Entity>();
        }

        public List<Entity> QueryEntitiesRange(Vector2 position, float radius)
        {
            var entities = new List<Entity>();
            (int minX, int maxX, int minY, int maxY) = GetRanges(position, radius, radius);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    string key = x + "," + y;
                    if (HashTable.ContainsKey(key))
                        entities.AddRange(HashTable[key]);
                }
            }

            return entities;
        }
        
        /// <summary>
        /// Note: only use if the CShape.Keys is already set.
        /// </summary>
        public List<Entity> QueryEntitiesCShape(CShape cShape)
        {
            var entities = new List<Entity>();
            foreach (var key in cShape.Keys)
            {
                entities.AddRange(HashTable[key]);
            }
            return entities;
        }

        /// <summary>
        /// Note: width and height are halved
        /// </summary>
        public (int, int, int, int) GetRanges(Vector2 position, float width, float height)
        {
            int minX = (int)MathF.Floor((position.X - width) / CellSize);
            int maxX = (int)MathF.Floor((position.X + width) / CellSize);
            int minY = (int)MathF.Floor((position.Y - height) / CellSize);
            int maxY = (int)MathF.Floor((position.Y + height) / CellSize);
            return (minX, maxX, minY, maxY);
        }

        public (int, int, int, int) GetCRectangleRanges(CRectangle rectangle)
        {
            var topLeft = rectangle.Vertices[0];
            var bottomRight = rectangle.Vertices[2];
            return (
                (int)MathF.Floor(topLeft.X / CellSize),
                (int)MathF.Floor(bottomRight.X / CellSize),
                (int)MathF.Floor(topLeft.Y / CellSize),
                (int)MathF.Floor(bottomRight.Y / CellSize)
            );
        }

        public void Clear()
        {
            HashTable.Clear();
        }

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