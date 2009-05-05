using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;

using FishingGirl.Gameplay;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Draws caught fish.
    /// </summary>
    public class FishCaughtView
    {
        /// <summary>
        /// Creates a new caught fish view.
        /// </summary>
        /// <param name="state">The fishing state producing caught fish.</param>
        public FishCaughtView(FishingState state)
        {
            state.ActionChanged += OnActionChanged;
            state.Event += OnFishingEvent;
        }

        /// <summary>
        /// Updates the caught fish.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            if (_fish != null)
            {
                _fish.Update(time);
                _fish.Sprite.Sprite.Position = _fish.Position;
                if (!_fishAnimation.Update(time))
                {
                    _fish = null;
                    _fishAnimation = null;
                }
            }
        }

        /// <summary>
        /// Draws the caught fish.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (_fish != null)
            {
                _fish.Sprite.Sprite.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Hide the fish after showing it off.
        /// </summary>
        private void OnFishingEvent(object stateObj, FishingEventArgs e)
        {
            if (e.Event == FishingEvent.FishCaught)
            {
                _fish = e.Fish;
                _fishAnimation = new SequentialAnimation(
                    new DelayAnimation(4f),
                    new ColorAnimation(_fish.Sprite.Sprite, Color.TransparentWhite, 1f, Interpolation.InterpolateColor(Easing.Uniform)));
            }
        }

        /// <summary>
        /// Hide the fish quickly for a new cast.
        /// </summary>
        private void OnActionChanged(object stateObj, FishingActionEventArgs e)
        {
            if (e.Action == FishingAction.PullBack && _fish != null)
            {
                _fishAnimation =
                    new ColorAnimation(_fish.Sprite.Sprite, Color.TransparentWhite, 0.25f, Interpolation.InterpolateColor(Easing.Uniform));
            }
        }

        private Fish _fish;
        private IAnimation _fishAnimation;
    }
}
