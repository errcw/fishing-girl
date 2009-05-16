using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// Tracks the money earned by the player.
    /// </summary>
    public class Money
    {
        /// <summary>
        /// The amount of money available to the player.
        /// </summary>
        public int Amount { get; private set; }

        /// <summary>
        /// Creates a new set of money.
        /// </summary>
        /// <param name="state">The fishing state to track.</param>
        public Money(FishingState state)
        {
            Amount = 0;
            state.Event += OnFishingEvent;
        }

        /// <summary>
        /// Adds the value of the caught fish to the money available
        /// </summary>
        private void OnFishingEvent(object stateObj, FishingEventArgs e)
        {
            if (e.Event == FishingEvent.FishCaught)
            {
                Amount += e.Fish.Description.Value;
            }
        }
    }
}
