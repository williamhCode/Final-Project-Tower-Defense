using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;

namespace TowerDefense.Sprite
{
    public class AnimationState
    {
        Dictionary<string, Dictionary<string, AnimatedSprite>> stateSprites;

        private AnimatedSprite currSprite;
        private AnimatedSprite lastSprite;

        private string state;
        private string direction;

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

        public void SetState(string state)
        {
            this.state = state;
        }

        public void SetDirection(string direction)
        {
            this.direction = direction;
        }

        public void Update(float dt)
        {
            currSprite = stateSprites[state][direction];

            if (lastSprite != currSprite)
                currSprite.Reset();
                
            currSprite.Update(dt);

            lastSprite = currSprite;
        }

        public AnimatedSprite GetSprite()
        {
            return currSprite;
        }
    }
}