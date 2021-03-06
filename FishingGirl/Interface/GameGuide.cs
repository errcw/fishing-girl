﻿using System;

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
    public class GameGuide
    {
        /// <summary>
        /// Creates a new guide.
        /// </summary>
        /// <param name="text">The text used to display the guide.</param>
        /// <param name="fishing">The game state to monitor.</param>
        public GameGuide(GameGuideView text, FishingState fishing, Store store, Money money)
        {
            _text = text;
            _text.Show(Resources.GuideCastingStart); // initial state
            _fishing = fishing;
            _fishing.ActionChanged += OnActionChanged;
            _fishing.Event += OnFishingEvent;
            store.Hit += (s, a) => _showStoreGuide = false;
            _money = money;
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
            else if (_fishing.Action == FishingAction.Idle)
            {
                OnIdle();
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
                        _showChainGuide = true; // show how to catch better fish now
                        break;
                }
            }
            if (e.Action != FishingAction.Idle && _showingIdleGuide)
            {
                // message is only valid in idle state
                _text.Hide();
                _showingIdleGuide = false;
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
                    if (_showChainGuide && e.Fish.Description.Size == FishSize.Small && state.Lure != Lures.Basic)
                    {
                        _text.Show(Resources.GuideLureSmallFish);
                        _textTimeout = new DelayAnimation(GuideTime);
                    }
                    if (e.Fish.Description.Size == FishSize.Large)
                    {
                        _text.Show(Resources.GuideReelInLarge);
                        _textTimeout = new DelayAnimation(GuideTime);
                    }
                    break;
                case FishingEvent.FishEaten:
                    if (_showChainGuide && e.Fish.Description.Size == FishSize.Small)
                    {
                        _showChainGuide = false;
                        _text.Show(Resources.GuideLureMediumFish);
                        _textTimeout = new DelayAnimation(GuideTime);
                    }
                    if (e.Fish.Description.Size == FishSize.Medium)
                    {
                        _text.Show(Resources.GuideReelInLarge);
                        _textTimeout = new DelayAnimation(GuideTime);
                    }
                    break;
                case FishingEvent.LureBroke:
                    _text.Show(Resources.GuideLureBroke);
                    _textTimeout = new DelayAnimation(GuideTime);
                    break;
                case FishingEvent.LureChanged:
                    _text.Hide();
                    _showLureGuide = false;
                    break;
                case FishingEvent.FishCaught:
                    _text.Show(String.Format(Resources.Caught, e.Fish.Description.Name), e.Fish.Description.Value);
                    _textTimeout = new DelayAnimation(CaughtTime);
                    break;
                case FishingEvent.LureIsland:
                    _text.Show(Resources.GuideReelInIsland);
                    _textTimeout = new DelayAnimation(GuideTime);
                    break;
            }
        }

        /// <summary>
        /// Displays appropriate help between casts.
        /// </summary>
        private void OnIdle()
        {
            if (_showStoreGuide && _money.Amount > 0)
            {
                _text.Show(Resources.GuideStore);
                _showingIdleGuide = true;
            }
            else if (_showLureGuide && _fishing.Lures.Count > 1)
            {
                _text.Show(Resources.GuideChangeLures);
                _showingIdleGuide = true;
            }
        }

        private bool _showCastingGuide = true;
        private bool _showChainGuide = false;
        private bool _showStoreGuide = true;
        private bool _showLureGuide = true;
        private bool _showingIdleGuide = false;

        private GameGuideView _text;
        private DelayAnimation _textTimeout;

        private FishingState _fishing;
        private Money _money;

        private const float ReelInGuideTime = 3f;
        private const float GuideTime = 3f;
        private const float CaughtTime = 4f;
    }
}
