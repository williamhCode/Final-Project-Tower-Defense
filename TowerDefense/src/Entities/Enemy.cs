using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoGame.Extended;
using System;
using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;

namespace TowerDefense.Entities
{
    public class Enemy : Entity
    {
        private const float MAX_SPEED = 300;
        private const float FRICTION = 0;
        private int spd;
        private int health;
        private Boolean isDead;
        public Enemy(Vector2 position,int speed,int size)
        {

            
            Position = position;
            Velocity = new Vector2(0, 0);
            spd = speed;
            Shape = new Circle(position, size);
            isDead = false;

        }
        public void Move(float dt)
        {
            Vector2 goal = new Vector2(0, 0);
            if (Position == Vector2.Zero)
            {
                Velocity = Velocity.MoveTowards(Vector2.Zero, FRICTION * dt);
            }
            else
            {
                double xdis = 0 - Position.X;
                double ydis = 0 - Position.Y;
                xdis = (xdis / Math.Sqrt(((xdis)*(xdis))+((ydis)*(ydis))));
                ydis = (ydis / Math.Sqrt(((xdis) * (xdis)) + ((ydis) * (ydis))));
                Vector2 direction = new Vector2((float)xdis, ((float)ydis));
                Velocity = Velocity.MoveTowards(direction * MAX_SPEED, spd * dt);
            }
            
            
            //Console.WriteLine(Position);

        }
        public override void Update(float dt)
        {
            Position += Velocity * dt;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Shape.Draw(spriteBatch, new Color(0, 0, 0), 2);
        }
        public void Damage()
        {
            health--;

            if (health <= 0)
            {
                isDead = true;

            }
        }


    }
}
