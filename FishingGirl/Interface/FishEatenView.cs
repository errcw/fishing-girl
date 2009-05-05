using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;
using Library.Extensions;

using FishingGirl.Gameplay;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Draws eaten fish.
    /// </summary>
    public class FishEatenView
    {
        /// <summary>
        /// Creates a new eaten fish view.
        /// </summary>
        /// <param name="state">The fishing state producing eaten fish.</param>
        public FishEatenView(FishingState state)
        {
            _fishes = new Dictionary<Fish, IAnimation>();
            state.Event += OnFishingEvent;
        }

        /// <summary>
        /// Updates the caught fish.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            var toRemove = _fishes.Where(kv => !kv.Value.Update(time)).ToList();
            toRemove.ForEach(kv => _fishes.Remove(kv.Key));
        }

        /// <summary>
        /// Draws the caught fish.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _fishes.Keys.ForEach(fish => fish.Sprite.Sprite.Draw(spriteBatch));
        }

        private void OnFishingEvent(object stateObj, FishingEventArgs e)
        {
            if (e.Event == FishingEvent.FishEaten)
            {
                _fishes.Add(
                    e.Fish,
                    new ColorAnimation(e.Fish.Sprite.Sprite, Color.TransparentWhite, 0.25f, Interpolation.InterpolateColor(Easing.Uniform)));
            }
        }

        private IDictionary<Fish, IAnimation> _fishes;
    }
}
