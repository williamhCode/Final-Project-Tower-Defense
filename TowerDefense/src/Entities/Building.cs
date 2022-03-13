using Microsoft.Xna.Framework;

using System;

namespace TowerDefense.Entities
{
    public abstract class Building : Entity
    {
        public int Health { get; set; }

        public Building(Vector2 position)
        {
            Position = position;
            Velocity = new Vector2(0, 0);
        }

        public bool IsDead()
        {
            return Health <= 0;
        }
    }
}