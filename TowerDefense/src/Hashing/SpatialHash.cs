using System;
using System.Collections.Generic;
using TowerDefense.Entities;
using Microsoft.Xna.Framework;

namespace TowerDefense.Hashing
{
    public class SpatialHash
    {
        public int CellSize { get; set; }
        private Dictionary<string, List<Entity>> hashTable;

        public SpatialHash(int cellSize)
        {
            CellSize = cellSize;
            hashTable = new Dictionary<string, List<Entity>>();
        }

        public string PositionToKey(Vector2 position)
        {
            return Math.Floor(position.X / CellSize) + "," + Math.Floor(position.Y / CellSize);
        }

        public void AddEntity(Entity entity)
        {
            var key = PositionToKey(entity.Position);
            if (!hashTable.ContainsKey(key))
                hashTable.Add(key, new List<Entity>());
            hashTable[key].Add(entity);    
        }

        public void RemoveEntity(Entity entity)
        {
            var key = PositionToKey(entity.Position);
            if (hashTable.ContainsKey(key))
                hashTable[key].Remove(entity);
        }

        public List<Entity> GetEntities(Vector2 position)
        {
            var key = PositionToKey(position);
            if (hashTable.ContainsKey(key))
                return hashTable[key];
            return new List<Entity>();
        }

        public List<Entity> QueryEntities(Vector2 position, float radius)
        {
            var entities = new List<Entity>();
            var minX = Math.Floor(position.X / CellSize) - 1;
            var maxX = Math.Floor(position.X / CellSize) + 1;
            var minY = Math.Floor(position.Y / CellSize) - 1;
            var maxY = Math.Floor(position.Y / CellSize) + 1;

            for (var x = minX; x <= maxX; x++)
            {
                for (var y = minY; y <= maxY; y++)
                {
                    var key = x + "," + y;
                    if (hashTable.ContainsKey(key))
                        entities.AddRange(hashTable[key]);
                }
            }

            return entities;
        }

        public void Clear()
        {
            hashTable.Clear();
        }
    }
}