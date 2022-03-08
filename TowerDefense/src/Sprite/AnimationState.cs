using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;
using System.Linq;

namespace TowerDefense.Sprite
{
    public class AnimationState<T>
    {
        Dictionary<Array, AnimatedSprite> stateSprites;

        public AnimatedSprite Sprite
        {
            get { return currSprite; }
        }

        private AnimatedSprite currSprite;
        private AnimatedSprite lastSprite;

        private Dictionary<string, T> states;

        public AnimationState(params string[] identifiers)
        {
            stateSprites = new Dictionary<Array, AnimatedSprite>();

            states = new Dictionary<string, T>();
            foreach (string identifier in identifiers)
            {
                states.Add(identifier, default(T));
            }
        }

        public void AddSprite(AnimatedSprite sprite, params T[] values)
        {
            stateSprites.Add(values, sprite);
        }

        public void SetState(string identifier, T value)
        {
            states[identifier] = value;
        }

        public void Update(float dt)
        {
            currSprite = stateSprites[states.Values.ToArray()];

            if (lastSprite != currSprite)
                currSprite.Reset();

            currSprite.Update(dt);

            lastSprite = currSprite;
        }
    }
}