using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;

namespace TowerDefense.Sprite
{
    public class AnimationState
    {
        Dictionary<string, Dictionary<string, AnimatedSprite>> stateSprites;

        public AnimatedSprite Sprite
        {
            get { return currSprite; }
        }

        private AnimatedSprite currSprite;
        private AnimatedSprite lastSprite;

        public string State { get; set; }
        public string Direction { get; set; }

        public AnimationState()
        {
            stateSprites = new Dictionary<string, Dictionary<string, AnimatedSprite>>();
        }

        public void AddSprite(AnimatedSprite sprite, string state, string direction)
        {
            if (stateSprites.TryGetValue(state, out Dictionary<string, AnimatedSprite> stateDict))
            {
                stateDict.Add(direction, sprite);
            }
            else
            {
                stateDict = new Dictionary<string, AnimatedSprite>();
                stateDict.Add(direction, sprite);
                stateSprites.Add(state, stateDict);
            }
        }

        public void Update(float dt)
        {
            currSprite = stateSprites[State][Direction];

            if (lastSprite != currSprite)
                currSprite.Reset();

            currSprite.Update(dt);

            lastSprite = currSprite;
        }
    }
}