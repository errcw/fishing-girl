using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;
using Library.Input;

using FishingGirl.Gameplay;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Draws fishing states.
    /// </summary>
    public class FishingView
    {
        /// <summary>
        /// Creates a new fishing state view.
        /// </summary>
        /// <param name="state">The fishing state to draw.</param>
        /// <param name="context">The interface context.</param>
        public FishingView(FishingState state, FishingGameContext context)
        {
            _state = state;
            _state.ActionChanged += OnActionChanged;
            _state.Event += OnFishingEvent;

            _lure = _state.LureSprites[(int)_state.Lure].Sprite;
            _rod = _state.RodSprites[(int)_state.Rod].Sprite;
            _line = _state.LineSprite.Sprite;

            _context = context;
        }

        /// <summary>
        /// Updates the view of the fishing state.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            UpdateRod(time);
            UpdateLure(time);
            UpdateLine(time);
        }

        /// <summary>
        /// Draws the fishing state.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch to draw in.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            _rod.Draw(spriteBatch);
            _line.Draw(spriteBatch);
            _lure.Draw(spriteBatch);
        }

        private void OnFishingEvent(object stateObj, FishingEventArgs e)
        {
            if (e.Event == FishingEvent.FishHooked || e.Event == FishingEvent.FishEaten)
            {
                Vector2 vibration = GetLureFeedback(e.Fish);
                _context.Input.AddVibration(Vibrations.FadeOut(vibration.X, vibration.Y, 1f, Easing.Uniform));
            }
            else if (e.Event == FishingEvent.LureBroke)
            {
                _context.Input.AddVibration(Vibrations.FadeOut(0.15f, 0.2f, 1f, Easing.Uniform));
                _lureAnimation = new ColorAnimation(_lure, Color.TransparentWhite, 0.5f, Interpolation.InterpolateColor(Easing.Uniform));
            }
            else if (e.Event == FishingEvent.LureChanged)
            {
                _lure = _state.LureSprites[(int)_state.Lure].Sprite;
            }
        }

        private void OnActionChanged(object stateObj, FishingActionEventArgs e)
        {
            if (e.Action == FishingAction.Idle)
            {
                _rod = _state.RodSprites[(int)_state.Rod].Sprite;
                _lureAnimation = new ColorAnimation(_lure, Color.White, 0.5f, Interpolation.InterpolateColor(Easing.Uniform));
            }
        }

        /// <summary>
        /// Updates the rod sprite.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        private void UpdateRod(float time)
        {
            _rod.Rotation = _state.RodRotation;
        }

        /// <summary>
        /// Updates the lure sprite.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        private void UpdateLure(float time)
        {
            _lure.Position = _state.LurePosition;
            if (_lureAnimation != null)
            {
                if (!_lureAnimation.Update(time))
                {
                    _lureAnimation = null;
                }
            }
        }

        /// <summary>
        /// Updates the line sprite.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        private void UpdateLine(float time)
        {
            Vector2 tipPosition = GetRodTipPosition();
            Vector2 lurePosition = _lure.Position;
            float angle = (float)Math.Atan2(lurePosition.Y - tipPosition.Y, lurePosition.X - tipPosition.X);
            _line.Position = tipPosition;
            _line.Rotation = -angle;
            _line.Scale = new Vector2(Vector2.Distance(tipPosition, lurePosition), 1f);
        }

        /// <summary>
        /// Returns the position of the tip of the fishing rod.
        /// </summary>
        private Vector2 GetRodTipPosition()
        {
            Vector2 tip = new Vector2(199, 10) - new Vector2(16, 11);
            tip = Vector2.Transform(tip, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -_rod.Rotation));
            tip = tip + _rod.Position;
            return tip;
        }


        /// <summary>
        /// Returns the strength of the feedback for a fish biting the lure.
        /// </summary>
        /// <param name="fish"></param>
        private Vector2 GetLureFeedback(Fish fish)
        {
            switch (fish.Description.Size)
            {
                case FishSize.Small: return new Vector2(0f, 0.2f);
                case FishSize.Medium: return new Vector2(0.15f, 0.2f);
                case FishSize.Large: return new Vector2(0.5f, 0.2f);
                case FishSize.VeryLarge: return new Vector2(0.75f, 0.5f);
            }
            return Vector2.Zero;
        }

        private FishingState _state;

        private Sprite _rod;
        private Sprite _lure;
        private Sprite _line;

        private IAnimation _lureAnimation;

        private FishingGameContext _context;
    }
}
