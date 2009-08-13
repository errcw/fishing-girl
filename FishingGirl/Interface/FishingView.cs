using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Library.Animation;
using Library.Sprite;
using Library.Sprite.Pipeline;
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
        /// <param name="context">The context from which to load the content.</param>
        public FishingView(FishingState state, FishingGameContext context)
        {
            _state = state;
            _state.ActionChanged += OnActionChanged;
            _state.Event += OnFishingEvent;

            _rod = _state.RodSprites[_state.Rod];
            _lure = _state.LureSprites[_state.Lure];
            _line = _state.LineSprite;

            _context = context;
        }

        /// <summary>
        /// Loads the fishing sprites.
        /// </summary>
        public void LoadContent(ContentManager content)
        {
            _releaseEffect = content.Load<SoundEffect>("Sounds/Whoosh");
            _splashEffect = content.Load<SoundEffect>("Sounds/Splash");
            _caughtEffect = content.Load<SoundEffect>("Sounds/Caught");
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
        /// Hides all the elements of the fishing state.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void UpdateHide(float time)
        {
            if (_hideAnimation == null)
            {
                _hideAnimation = new CompositeAnimation(
                    new ColorAnimation(_lure, Color.TransparentWhite, 1f, InterpolateColor),
                    new ColorAnimation(_rod, Color.TransparentWhite, 1f, InterpolateColor),
                    new ColorAnimation(_line, Color.TransparentWhite, 1f, InterpolateColor));
            }
            else
            {
                if (!_hideAnimation.Update(time))
                {
                    _hideAnimation = null;
                }
            }
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
            switch (e.Event)
            {
                case FishingEvent.FishHooked: goto case FishingEvent.FishEaten;
                case FishingEvent.FishEaten:
                    Vector2 vibration = GetLureFeedback(e.Fish);
                    _context.Input.AddVibration(Vibrations.Constant(vibration.X, vibration.Y, 0.5f));
                    break;

                case FishingEvent.FishCaught:
                    _caughtEffect.Play(0.4f, 0f, 0f);
                    break;

                case FishingEvent.LureBroke:
                    _context.Input.AddVibration(Vibrations.FadeOut(0.2f, 0.3f, 1f, Easing.Uniform));
                    _lureAnimation = new ColorAnimation(_lure, Color.TransparentWhite, 0.5f, InterpolateColor);
                    break;

                case FishingEvent.LureChanged:
                    _lure = _state.LureSprites[_state.Lure];
                    break;

                case FishingEvent.RodChanged:
                    _rod = _state.RodSprites[_state.Rod];
                    break;
            }
        }

        private void OnActionChanged(object stateObj, FishingActionEventArgs e)
        {
            switch (e.Action)
            {
                case FishingAction.Idle:
                    _lureAnimation = new ColorAnimation(_lure, Color.White, 0.5f, InterpolateColor);
                    break;

                case FishingAction.Cast:
                    _releaseEffect.Play(0.4f, 0f, 0f);
                    break;

                case FishingAction.Reel:
                    _splashEffect.Play(0.2f, 0f, 0f);
                    break;
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
                case FishSize.Medium: return new Vector2(0.2f, 0.3f);
                case FishSize.Large: return new Vector2(0.6f, 0.4f);
                case FishSize.VeryLarge: return new Vector2(0.75f, 0.5f);
            }
            return Vector2.Zero;
        }

        private FishingState _state;

        private Sprite _rod;
        private Sprite _lure;
        private Sprite _line;
        private IAnimation _lureAnimation;
        private IAnimation _hideAnimation;

        private SoundEffect _releaseEffect;
        private SoundEffect _splashEffect;
        private SoundEffect _caughtEffect;

        private FishingGameContext _context;

        private readonly Interpolate<Color> InterpolateColor = Interpolation.InterpolateColor(Easing.Uniform);
    }
}
