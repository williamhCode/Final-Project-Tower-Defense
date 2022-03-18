// using Microsoft.Xna.Framework;

// namespace TowerDefense.Projectiles
// {
//     public abstract class ProjectileComponent
//     {
//         public Projectile Obj { get; }

//         public Vector2 StartPosition => Obj.StartPosition;
//         public Vector2 TargetPosition => Obj.TargetPosition;
//         public float Speed => Obj.Speed;
//         public float Damage => Obj.Damage;
//         public float TimeElapsed => Obj.TimeElapsed;
//     }

//     public abstract class Path : ProjectileComponent
//     {
//         public abstract (Vector2 position, float angle) GetCurrentState();
//     }

//     public class StraightPath : Path
//     {
//         public override (Vector2 position, float angle) GetCurrentState()
//         {
//             var position = Obj.StartPosition + Obj.Velocity * Obj.TimeElapsed;
//             var angle = MathHelper.ToDegrees(Obj.Velocity.ToAngle());
//             return (position, angle);
//         }
//     }

//     public class Projectile
//     {
//         public Vector2 StartPosition { get; set; }
//         public Vector2 TargetPosition { get; set; }

//         public float Speed { get; set; }
//         public float Damage { get; set; }

//         public float TimeElapsed { get; set; }

//         private IPath path;

//         public Projectile(Vector2 startPosition, Vector2 targetPosition, float speed, float damage, IPath path)
//         {
//             StartPosition = startPosition;
//             TargetPosition = targetPosition;
//             Speed = speed;
//             Damage = damage;
//             this.path = path;
//         }



        

//     }
// }