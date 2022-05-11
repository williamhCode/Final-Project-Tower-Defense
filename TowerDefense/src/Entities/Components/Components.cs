using Microsoft.Xna.Framework;
using TowerDefense.Entities;
using TowerDefense.Sprite;
using TowerDefense.Collision;

using System;

namespace Towerdefense.Entities.Components
{
    public interface IEntityComponent
    {
        Entity Obj { get; }

        Vector2 Position => Obj.Position;
        Vector2 Velocity => Obj.Velocity;
        CShape CShape => Obj.CShape;
    }

    public interface IHitboxComponent : IEntityComponent
    {
        CShape HitboxShape { get; set; }
        Vector2 HitboxOffset { get; set; }
        void UpdateHitbox();
    }

    public static class IHitboxComponentExtensions
    {
        public static void _UpdateHitbox(this IHitboxComponent obj)
        {
            obj.HitboxShape.Position = obj.Position - obj.HitboxOffset;
            obj.HitboxShape.Update();
        }
    }

    public interface IFaceableComponent : IEntityComponent
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

    public static class IFaceableComponentExtensions
    {
        public static void _DecideDirection(this IFaceableComponent obj, Vector2 coords)
        {
            Vector2 direction = coords - obj.Position;
            if (Vector2.Dot(direction, Vector2.UnitX) > 0)
            {
                obj.animationState.SetState("Direction", IFaceableComponent.Direction.Right);
            }
            else
            {
                obj.animationState.SetState("Direction", IFaceableComponent.Direction.Left);
            }
        }
    }
}