using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;



namespace TowerDefense.Entities.Enemies
{
    public class Bandit : Enemy
    {
        private int speed;
        private int size;

        public Bandit(Vector2 Position) : base(Position, 50, 20)
        {

        }

        public void Attack()
        {

        }

    }
}