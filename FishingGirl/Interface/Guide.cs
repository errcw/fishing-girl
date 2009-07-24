using System;

using Microsoft.Xna.Framework;

using Library.Animation;

using FishingGirl.Gameplay;
using FishingGirl.Screens;
using FishingGirl.Properties;

namespace FishingGirl.Interface
{
    /// <summary>
    /// Guides the player's actions by displaying context-sensitive help text.
    /// </summary>
    public class Guide
    {
        /// <summary>
        /// Creates a new guide.
        /// </summary>
        /// <param name="text">The text used to display the guide.</param>
        /// <param name="fishing">The game state to monitor.</param>
        public Guide(GuideView text, GameplayScreen game, FishingState fishing)
        {
            _text = text;
            _text.Show(Resources.GuideCastingStart); // initial state
            fishing.ActionChanged += OnActionChanged;
            fishing.Event += OnFishingEvent;
        }

        /// <summary>
        /// Updates this guide for the current frame.
        /// </summary>
        public void Update(float time)
        {
            if (_textTimeout != null)
            {
                if (!_textTimeout.Update(time))
                {
                    _text.Hide();
                    _textTimeout = null;
                }
            }
        }

        /// <summary>
        /// Displays appropriate help based on the current action.
        /// </summary>
        private void OnActionChanged(object stateObj, FishingActionEventArgs e)
        {
            FishingState state = (FishingState)stateObj;
            if (_showCastingGuide)
            {
                switch (e.Action)
                {
                    case FishingAction.Idle:
                        _text.Show(Resources.GuideCastingStart);
                        break;
                    case FishingAction.Swing:
                        _text.Show(Resources.GuideCastingRelease);
                        break;
                    case FishingAction.Cast:
                        _text.Hide();
                        break;
                    case FishingAction.Reel:
                        _text.Show(Resources.GuideReelIn);
                        _textTimeout = new DelayAnimation(ReelInGuideTime);
                        _showCastingGuide = false; // no guide after first successful cast
                        _showLureGuide = true; // show how to catch better fish now
                        break;
                }
            }
            else if (_showChangeGuide && e.Action == FishingAction.Idle && state.Lures.Count > 1 && _textTimeout == null)
            {
                _text.Show(Resources.GuideChangeLures);
            }
        }

        /// <summary>
        /// Displays the type of fish caught and its value.
        /// </summary>
        private void OnFishingEvent(object stateObj, FishingEventArgs e)
        {
            FishingState state = (FishingState)stateObj;
            switch (e.Event)
            {
                case FishingEvent.FishHooked:
                    if (_showLureGuide && e.Fish.Description.Size == FishSize.Small && state.Lure != Lures.Basic)
                    {
                        _text.Show(Resources.GuideLureSmallFish);
                        _textTimeout = new DelayAnimation(GuideTime);
                    }
                    break;
                case FishingEvent.FishEaten:
                    if (_showLureGuide && e.Fish.Description.Size == FishSize.Small)
                    {
                        _showLureGuide = false;
                        _text.Show(Resources.GuideLureMediumFish);
                        _textTimeout = new DelayAnimation(GuideTime);
                    }
                    break;
                case FishingEvent.LureBroke:
                    if (_showBrokenGuide)
                    {
                        _showBrokenGuide = false;
                        _text.Show(Resources.GuideLureBroke);
                        _textTimeout = new DelayAnimation(GuideTime);
                    }
                    break;
                case FishingEvent.LureChanged:
                    _text.Hide();
                    _showChangeGuide = false;
                    break;
                case FishingEvent.FishCaught:
                    _text.Show(String.Format(Resources.Caught, e.Fish.Description.Name), e.Fish.Description.Value);
                    _textTimeout = new DelayAnimation(CaughtTime);
                    break;
            }
        }

        private void OnGuideHidden()
        {
        }

        private bool _showCastingGuide = true;
        private bool _showBrokenGuide = true;
        private bool _showLureGuide = false;
        private bool _showChangeGuide = true;

        private GuideView _text;
        private DelayAnimation _textTimeout;

        private const float ReelInGuideTime = 3f;
        private const float GuideTime = 3f;
        private const float CaughtTime = 4f;
    }
}
