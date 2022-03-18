using TowerDefense.Entities;
using Microsoft.Xna.Framework;
using TowerDefense.Sprite;
using System;

namespace TowerDefense.Components
{
    public interface IEntityComponent
    {
        Entity Obj { get; }

        Vector2 Position => Obj.Position;
        Vector2 Velocity => Obj.Velocity;
        Collision.CShape CShape => Obj.CShape;
    }

    public interface IFaceable : IEntityComponent
    {
        AnimationState<Enum> animationState { get; set; }
        const string DIRECTION = "Direction";
        enum Direction
        {
            Left,
            Right
        }
        
        void DecideDirection(Vector2 coords);
    }

    public static class IFaceableExtensions
    {
        public static void _DecideDirection(this IFaceable obj, Vector2 coords)
        {
            Vector2 direction = coords - obj.Position;
            if (Vector2.Dot(direction, Vector2.UnitX) > 0)
            {
                obj.animationState.SetState("Direction", IFaceable.Direction.Right);
            }
            else
            {
                obj.animationState.SetState("Direction", IFaceable.Direction.Left);
            }
        }
    }

    /*
    public abstract class EntityComponent
    {
        protected Entity obj { get; set; }

        protected Vector2 Position => obj.Position;
        protected Vector2 Velocity => obj.Velocity;
        protected Collision.CShape CShape => obj.CShape;

        public EntityComponent(Entity other)
        {
            this.obj = other;
        }
    }

    public class Faceable : EntityComponent, IFaceable
    {
        public Faceable(Entity other) : base(other)
        {
        }

        public const string DIRECTION = "Direction";
        public enum Direction
        {
            Left,
            Right
        }

        public AnimationState<Enum> animationState;

        public void DecideDirection(Vector2 coords)
        {
            Vector2 direction = coords - Position;
            if (Vector2.Dot(direction, Vector2.UnitX) > 0)
            {
                animationState.SetState("Direction", Direction.Right);
            }
            else
            {
                animationState.SetState("Direction", Direction.Left);
            }
        }
    }
    */
}