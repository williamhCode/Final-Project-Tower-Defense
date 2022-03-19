using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

using MonoGame.Extended;

using TowerDefense.Maths;
using TowerDefense.Entities;
using TowerDefense.Hashing;
using TowerDefense.Sprite;

namespace TowerDefense.Projectiles
{
    public abstract class ProjectileComponent
    {
        public Projectile Obj { get; set; }

        public Vector2 StartPosition => Obj.StartPosition;
        public Vector2 TargetPosition => Obj.TargetPosition;
        public float Speed => Obj.Speed;
        public int Damage => Obj.Damage;
        public float TimeElapsed => Obj.TimeElapsed;

        public Vector2 Position => Obj.Position;
        public float Angle => Obj.Angle;

        public ProjectileComponent SetObj(Projectile obj)
        {
            Obj = obj;
            Initialize();
            return this;
        }

        public virtual void Initialize()
        {

        }
    }

    public abstract class Path : ProjectileComponent
    {
        public abstract (Vector2 position, float angle) GetCurrentState();
    }

    public class StraightPath : Path
    {
        private Vector2 speed;
        private float angle;

        public StraightPath()
        {
        }

        public override void Initialize()
        {
            speed = (TargetPosition - StartPosition).Normalized() * Speed;
            angle = MathF.Atan2(TargetPosition.Y - StartPosition.Y, TargetPosition.X - StartPosition.X);
        }

        public override (Vector2, float) GetCurrentState()
        {
            var position = Obj.StartPosition + speed * TimeElapsed;
            return (position, angle);
        }
    }

    public abstract class DamageType : ProjectileComponent
    {
        public abstract void ApplyDamage(float dt);
    }

    public class DirectDamage : DamageType
    {
        private Enemy target;

        public DirectDamage(Enemy target)
        {
            this.target = target;
        }

        public override void ApplyDamage(float dt)
        {
            target.Health -= Damage;
        }
    }

    public class AreaDamage : DamageType
    {
        private SpatialHashGrid SHG;
        private float radius;
        private float radiusSq;
        private float knockback;

        public AreaDamage(SpatialHashGrid SHG, float splashRadius, float knockbackForce)
        {
            this.SHG = SHG;
            radius = splashRadius;
            radiusSq = radius * radius;
            knockback = knockbackForce;
        }

        public override void ApplyDamage(float dt)
        {
            foreach (Enemy enemy in SHG.QueryEntitiesRange(Position, radius))
            {
                var diff = enemy.Position - Position;
                if (diff.LengthSquared() <= radiusSq)
                {
                    enemy.Health -= Damage;
                    var direction = diff.Normalized();
                    enemy.Velocity += direction * knockback * dt;
                }
            }
        }
    }

    public class Projectile
    {
        public Vector2 StartPosition { get; set; }
        public Vector2 TargetPosition { get; set; }
        public float Speed { get; set; }
        public int Damage { get; set; }
        public float TimeElapsed { get; set; }
        public bool HasEnded { get; set; }

        private Path path;
        private DamageType damageType;

        public Vector2 Position { get; set; }
        public float Angle { get; set; }

        // private AnimatedSprite projectileSprite;
        // private AnimatedSprite hitSprite;
        private bool hasHit;
        private float hitTime;
        private float hitTimeElasped;

        // public Projectile(Vector2 startPosition, Vector2 targetPosition, float speed, int damage, 
        // Path pathIn, DamageType damageTypeIn, AnimatedSprite projectileSprite, AnimatedSprite hitSprite)
        public Projectile(Vector2 startPosition, Vector2 targetPosition, float speed, int damage, 
        Path pathIn, DamageType damageTypeIn, float hitTime)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            Speed = speed;
            Damage = damage;
            TimeElapsed = 0;
            HasEnded = false;

            path = (Path)pathIn.SetObj(this);
            damageType = (DamageType)damageTypeIn.SetObj(this);

            (Position, Angle) = path.GetCurrentState();

            // this.projectileSprite = projectileSprite;
            // this.hitSprite = hitSprite;
            hasHit = false;
            this.hitTime = hitTime;
            hitTimeElasped = 0;
        }

        public bool ReachedTarget()
        {
            return Vector2.Dot(TargetPosition - Position, TargetPosition - StartPosition) < 0;
        }

        public void Update(float dt)
        {
            if (hasHit)
            {
                hitTimeElasped += dt;
                if (hitTimeElasped >= hitTime)
                {
                    HasEnded = true;
                }
            }
            else
            {
                if (ReachedTarget())
                {
                    Position = TargetPosition;
                    damageType.ApplyDamage(dt);
                    hasHit = true;
                }
                TimeElapsed += dt;
                (Position, Angle) = path.GetCurrentState();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // var offset = new Vector2(projectileSprite.Width / 2, projectileSprite.Height / 2);
            // if (hasHit)
            // {
            //     hitSprite.Draw(spriteBatch, Position, offset);
            // }
            // else
            // {
            //     projectileSprite.Draw(spriteBatch, Position, offset, Angle);
            // }
            if (hasHit)
            {
                spriteBatch.DrawCircle(Position, 15, 20, Color.Red, 1);
            }
            else
            {
                spriteBatch.DrawCircle(Position, 8, 20, Color.Red, 1);
            }
        }
    }
}