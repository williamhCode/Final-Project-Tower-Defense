using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;

using System;

namespace TowerDefense.Entities
{
    public abstract class Building : Entity
    {
        public Building(Vector2 position)
        {
            Position = position;
            Velocity = new Vector2(0, 0);
        }
    }
}