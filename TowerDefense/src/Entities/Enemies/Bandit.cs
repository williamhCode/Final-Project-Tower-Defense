using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using static TowerDefense.Collision.CollisionFuncs;
using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using TowerDefense.Hashing;
using static TowerDefense.Maths.MathFuncs;

using MonoGame.Extended;

namespace TowerDefense.Entities.Enemies
{
    public class Bandit : Enemy
    {
        private const string BANDIT_STATE = "PlayerState";
        private enum BanditState
        {
            Idle,
            Walking,
            Attacking,
            Dead
        }

        private const float MAX_SPEED = 50;
        private const float FRICTION = 1000;
        private const float ACCELERATION = 100;

        public static AnimationState<Enum> AnimationState;

        public static void LoadContent(ContentManager content)
        {
            content.RootDirectory = "Content/Sprites/Bandit";

            float frameTime = 0.05f;
            AnimationState = new AnimationState<Enum>(BANDIT_STATE, DIRECTION);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("bandit"), 32, 32, frameTime), BanditState.Idle, Direction.Right);
            AnimationState.AddSprite(new AnimatedSprite(content.Load<Texture2D>("bandit"), 32, 32, frameTime, flipped: true), BanditState.Idle, Direction.Left);
        }

        public Bandit(Vector2 position, int health) : base(position, health)
        {
            CShape = new CCircle(position, 7);
            // CShape = new CRectangle(position, 16, 6);

            animationState = AnimationState.Copy();
            animationState.SetState(BANDIT_STATE, BanditState.Idle);
            animationState.SetState(DIRECTION, Direction.Right);

            HitboxShape = new CRectangle(position, 20, 32);
            YHitboxOffset = 14;

            InitRays();
        }

        public override void Move(Vector2 goal, float dt)
        {
            Vector2 direction = (goal - Position).Normalized();
            if ((goal - Position).Length() < 10)
            {
                Velocity = Velocity.MoveTowards(Vector2.Zero, FRICTION * dt);
            }
            else
            {
                Velocity = Velocity.MoveTowards(direction * MAX_SPEED, ACCELERATION * dt);
            }
            DecideDirection(goal);
        }

        private const float COHESION_DIST = 60;
        private const float COHESION_FACTOR = 10f;
        private const float COHESION_SENSTIVITY = 0.01f;

        private const float ALIGNMENT_DIST = 60;
        private const float ALIGNMENT_FACTOR = 0.00f;
        private const float ALIGNMENT_SENSTIVITY = 0.3f;

        private const float SEPARATION_DIST = 60;
        private const float SEPARATION_FACTOR = 3.2f;
        private const float SEPARATION_SENSTIVITY = 100f;

        private const float WALL_DIST = 60;
        private const float WALL_FACTOR = 200f;
        private const float WALL_SENSITIVITY = 1f;
        private readonly float WALL_FOV = MathHelper.ToRadians(60f);
        private Vector2 wallSteeringDeubug;
        private Vector2? intersect;

        public override void ApplyFlocking(float dt, SpatialHashGrid SHGFlocking, SpatialHashGrid SHGBuildings, Vector2 goal)
        {
            var enemiesToCheck = SHGFlocking.QueryEntities(Position, SEPARATION_DIST);
            enemiesToCheck.Remove(this);

            var cohesion = Vector2.Zero;
            var alignment = Vector2.Zero;
            var separation = Vector2.Zero;

            Parallel.ForEach(enemiesToCheck, e =>
            {
                var sqdist = Vector2.DistanceSquared(Position, e.Position);
                var dist = MathF.Sqrt(sqdist);

                if (sqdist < MathF.Pow(COHESION_DIST, 2))
                {
                    cohesion += (e.Position - Position) /
                    (dist / COHESION_SENSTIVITY + sqdist);
                }
                if (sqdist < MathF.Pow(ALIGNMENT_DIST, 2))
                {
                    alignment += (e.Velocity - Velocity).Normalized() /
                    (1 / ALIGNMENT_SENSTIVITY + dist);
                }
                if (sqdist < MathF.Pow(SEPARATION_DIST, 2))
                {
                    separation += (Position - e.Position) /
                    (dist / SEPARATION_SENSTIVITY + sqdist);
                }
            });

            var buildingsToCheck = SHGBuildings.QueryEntities(Position, WALL_DIST);

            // position setting for two FOV
            var velDirection = Velocity.Normalized();
            float radius = 8f;
            float ang = MathF.PI / 2 - WALL_FOV / 2;
            var offset = -velDirection * MathF.Tan(ang) * radius;

            var starts = new[] {
                Position + offset,
                Position + offset
            };
            var ends = new[] {
                Position + velDirection.Rotate(WALL_FOV / 2) * WALL_DIST,
                Position + velDirection.Rotate(-WALL_FOV / 2) * WALL_DIST
            };

            (float sqdist, Vector2 intersection, Vector2 normal)? collData = null;
            foreach (var b in buildingsToCheck)
            {
                var sqdist = Vector2.DistanceSquared(Position, b.Position);

                if (sqdist < MathF.Pow(WALL_DIST, 2))
                {
                    for (int i = 0; i < starts.Length; i++)
                    {
                        var start = starts[i];
                        var end = ends[i];
                        if (IsColliding((CPolygon)b.CShape, start, end, out var tempCollData))
                        {
                            if (collData == null || tempCollData.Value.sqdist < collData.Value.sqdist)
                            {
                                collData = tempCollData;
                            }
                        }
                    }
                }
            }

            // debug
            intersect = null;

            var wallSteering = Vector2.Zero;
            if (collData.HasValue)
            {
                var data = collData.Value;
                var normal = data.normal;

                // dir will be positive if enemy is clockwise of normal
                // if enemy is clockwise (dir is positive) of normal, then turn enemy clockwise
                // to turn enemy clockwise, steer enemy towards 90 degrees counterclockise of normal
                Vector2 wallDir;
                if (Cross(normal, velDirection) > 0)
                {
                    wallDir = new Vector2(-normal.Y, normal.X);
                    if (Cross(wallDir, velDirection) > 0)
                    {
                        wallDir = Vector2.Reflect(velDirection, normal);
                    }
                }
                else
                {
                    wallDir = new Vector2(normal.Y, -normal.X);
                    if (Cross(wallDir, velDirection) < 0)
                    {
                        wallDir = Vector2.Reflect(velDirection, normal);
                    }
                }

                var sqdist = data.sqdist;
                // var sqdist = Vector2.DistanceSquared(data.intersection, Position);
                var dist = MathF.Sqrt(sqdist);

                wallSteering = wallDir /
                (dist / WALL_SENSITIVITY + sqdist);

                // debug
                intersect = data.intersection;
            }

            wallSteeringDeubug = wallSteering;
            // Console.WriteLine(wallSteering);

            var targetDirection = (goal - Position).Normalized();
            DecideDirection(goal);

            var force =
            cohesion * COHESION_FACTOR +
            alignment * ALIGNMENT_FACTOR +
            separation * SEPARATION_FACTOR +
            wallSteering * WALL_FACTOR +
            targetDirection * 0.2f;

            Velocity = Velocity.MoveTowards(MAX_SPEED * force.Normalized(), ACCELERATION * dt);

            // Velocity += force * 1000 * dt;

            // if (Velocity.Length() > MAX_SPEED)
            // {
            //     Velocity = Velocity.Normalized() * MAX_SPEED;
            // }

            // randomize velocity if its undefined
            if (!float.IsFinite(Velocity.X))
            {
                var rand = new Random();
                var angle = (float)rand.NextDouble() * MathF.PI * 2;
                Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * MAX_SPEED;
            }
        }

        private const float STEERING_DIST = 60f;
        private const int NUM_RAYS = 16;
        private Vector2[] rays = new Vector2[NUM_RAYS];

        private void InitRays()
        {
            for (int i = 0; i < NUM_RAYS; i++)
            {
                var angle = (float)i / NUM_RAYS * MathF.PI * 2;
                rays[i] = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            }
        }

        private struct DebugInfo
        {
            public Vector2 start;
            public Vector2 end;
            public Color color;
        }
        private DebugInfo[] debugInfo = new DebugInfo[NUM_RAYS];

        public override void Steer(float dt, SpatialHashGrid SHGBuildings, Vector2 goal)
        {
            var buildingsToCheck = SHGBuildings.QueryEntities(Position, STEERING_DIST);

            var direction = (goal - Position).Normalized();
            var raysDot = new float[NUM_RAYS];
            for (int i = 0; i < NUM_RAYS; i++)
            {
                var ray = rays[i];
                var rayDot = Vector2.Dot(ray, direction);
                raysDot[i] = rayDot;
            }
            // normalize
            raysDot = raysDot.Select(x => (x + 1) / 2).ToArray();

            // create boolean array for each ray
            var rayCollision = new bool[NUM_RAYS];
            for (int i = 0; i < NUM_RAYS; i++)
            {
                var start = Position;
                var end = Position + rays[i] * STEERING_DIST;
                foreach (var b in buildingsToCheck)
                {
                    if (IsColliding((CPolygon)b.CShape, start, end))
                    {
                        rayCollision[i] = true;
                        break;
                    }
                }
            }

            // select ray with highest dot product and no collision
            var maxDot = float.MinValue;
            var maxDotIndex = -1;
            for (int i = 0; i < NUM_RAYS; i++)
            {
                if (!rayCollision[i] && raysDot[i] > maxDot)
                {
                    maxDot = raysDot[i];
                    maxDotIndex = i;
                }
            }

            // if no ray found, steer to zero, else steer towards that ray
            Vector2 steering;
            if (maxDotIndex == -1)
            {
                steering = Vector2.Zero;
            }
            else
            {
                // steering is the direction of the largest dot product ray
                steering = rays[maxDotIndex].Normalized();

                // steering is the sum all non-colliding rays
                // steering = Vector2.Zero;
                // for (int i = 0; i < NUM_RAYS; i++)
                // {
                //     if (!rayCollision[i])
                //     {
                //         steering += rays[i] * raysDot[i];
                //     }
                // }
                // steering = steering.Normalized();
            }

            Velocity = Velocity.MoveTowards(MAX_SPEED * steering, ACCELERATION * dt);

            // add debug info
            for (int i = 0; i < NUM_RAYS; i++)
            {
                var start = Position;
                var end = Position + rays[i] * raysDot[i] * STEERING_DIST;
                debugInfo[i] = new DebugInfo
                {
                    start = start,
                    end = end,
                    color = rayCollision[i] ? Color.Red : Color.Green
                };
            }

            if (maxDotIndex != -1)
            {
                debugInfo[maxDotIndex].color = Color.Purple;
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            animationState.Sprite.Draw(spriteBatch, Position, new Vector2(16, 32));
        }

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            base.DrawDebug(spriteBatch);

            /* old debug
            var velDirection = Velocity.Normalized();
            float radius = 8f;
            float ang = MathF.PI / 2 - WALL_FOV / 2;
            Vector2 offset = -velDirection * MathF.Tan(ang) * radius;

            var starts = new[] {
                Position + offset,
                Position + offset
            };
            var ends = new[] {
                Position + velDirection.Rotate(WALL_FOV / 2) * WALL_DIST,
                Position + velDirection.Rotate(-WALL_FOV / 2) * WALL_DIST
            };

            for (int i = 0; i < starts.Length; i++)
            {
                spriteBatch.DrawLine(starts[i], ends[i], Color.Red);
            }

            spriteBatch.DrawLine(Position, Position + wallSteeringDeubug.Normalized() * WALL_DIST, Color.Green);
            if (intersect.HasValue)
                spriteBatch.DrawPoint(intersect.Value, Color.Blue, 4);
            */

            // new debug
            foreach (var info in debugInfo)
            {
                spriteBatch.DrawLine(info.start, info.end, info.color);
            }
        }
    }
}