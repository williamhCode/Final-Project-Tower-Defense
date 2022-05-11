using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using MonoGame.Extended;

using TowerDefense.Collision;
using TowerDefense.Sprite;
using TowerDefense.Maths;
using Towerdefense.Entities.Components;

using System;

namespace TowerDefense.Entities.Buildings
{
    public abstract class Resource : Building, IHitboxComponent
    {
        public Entity Obj => this;
        
        public CShape HitboxShape { get; set; }
        public Vector2 HitboxOffset { get; set; }

        public Resource(Vector2 position) : base(position)
        {

        }

        public void UpdateHitbox() => this._UpdateHitbox();

        public override void DrawDebug(SpriteBatch spriteBatch)
        {
            base.DrawDebug(spriteBatch);
            HitboxShape.Draw(spriteBatch, Color.Blue, 1);
        }
    }
}