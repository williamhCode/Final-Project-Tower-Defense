using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Entities;

using System;
using System.Collections.Generic;


namespace TowerDefense.Entities.Buildings
{
    public abstract class Tower : Building
    {
        public int Range { get; set; }
        public int Damage { get; set; }

        public Tower(Vector2 position) : base(position)
        {
            Position = position;
            Velocity = new Vector2(0, 0);
        }

        public abstract void Shoot(float dt, List<Enemy> enemies);

        public abstract void DetectEnemy(float dt, List<Enemy> enemies);
    }
}