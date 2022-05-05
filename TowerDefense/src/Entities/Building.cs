using Microsoft.Xna.Framework;

using System;

namespace TowerDefense.Entities
{
    public abstract class Building : Entity
    {
        public int Health { get; set; }

        public Boolean IsDead => Health <= 0;

        public Building(Vector2 position)
        {
            Position = position;
            Velocity = new Vector2(0, 0);
        }

    }
}