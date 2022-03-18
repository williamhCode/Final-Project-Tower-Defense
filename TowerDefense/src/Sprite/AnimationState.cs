using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System;
using System.Linq;

namespace TowerDefense.Sprite
{
    public class AnimationState<T>
    {
        private Dictionary<string, AnimatedSprite> stateSprites;

        public AnimatedSprite Sprite => currSprite;

        private AnimatedSprite currSprite;
        private AnimatedSprite lastSprite;

        private Dictionary<string, T> states;

        /// <summary>
        /// Note: params are identifiers for the state names.
        /// </summary>
        public AnimationState(params string[] identifiers)
        {
            stateSprites = new Dictionary<string, AnimatedSprite>();

            states = new Dictionary<string, T>();
            foreach (string identifier in identifiers)
            {
                states.Add(identifier, default(T));
            }
        }

        /// <summary>
        /// Note: values correspond in the same order to the identifiers in the constructor, and have the Generic Type.
        /// </summary>
        public void AddSprite(AnimatedSprite sprite, params T[] values)
        {
            stateSprites.Add(string.Join(",", values), sprite);
        }

        public void SetState(string identifier, T value)
        {
            states[identifier] = value;
        }

        public T GetState(string identifier)
        {
            return states[identifier];
        }

        public void Update(float dt)
        {
            currSprite = stateSprites[string.Join(",", states.Values.ToArray())];

            if (lastSprite != currSprite)
                currSprite.Reset();

            currSprite.Update(dt);

            lastSprite = currSprite;
        }

        public AnimationState<T> Copy()
        {
            AnimationState<T> copy = new AnimationState<T>(states.Keys.ToArray());

            copy.stateSprites = stateSprites;

            return copy;
        }
    }
}