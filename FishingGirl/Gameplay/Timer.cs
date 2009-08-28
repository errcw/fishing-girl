using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// A running timer.
    /// </summary>
    public class Timer
    {
        /// <summary>
        /// The time, in seconds.
        /// </summary>
        public float Time
        {
            get { return _time; }
            set { _time = Math.Max(0, value); }
        }

        /// <summary>
        /// Creates a new timer.
        /// </summary>
        /// <param name="state">The fishing state to track.</param>
        public Timer(FishingState state)
        {
            Time = InitialTime;
            state.Event += OnFishingEvent;
        }

        /// <summary>
        /// Updates the timer.
        /// </summary>
        /// <param name="time">The elapsed time, in seconds, since the last update.</param>
        public void Update(float time)
        {
            Time -= time;
        }

        /// <summary>
        /// Adds or removes time based on the event.
        /// </summary>
        private void OnFishingEvent(object stateObj, FishingEventArgs e)
        {
            switch (e.Event)
            {
                case FishingEvent.FishCaught:
                    Time += GetTimeBonus(e.Fish);
                    break;
                case FishingEvent.LureBroke:
                    Time -= LureBreakPenalty;
                    break;
            }
        }

        /// <summary>
        /// Returns the time bonus for catching the specified fish.
        /// </summary>
        private float GetTimeBonus(Fish fish)
        {
            switch (fish.Description.Size)
            {
                case FishSize.Small: return 10f;
                case FishSize.Medium: return 20f;
                case FishSize.Large: return 30f;
                default: return 0f;
            }
        }

        private float _time;

        private const float InitialTime = 10f * 60f;
        private const float LureBreakPenalty = 15f;
    }
}
