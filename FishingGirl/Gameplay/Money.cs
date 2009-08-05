﻿using System;

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
        /// Occurs when money is gained or lost.
        /// </summary>
        public event EventHandler<MoneyEventArgs> AmountChanged;

        /// <summary>
        /// The amount of money available to the player.
        /// </summary>
        public int Amount
        {
            get;
            set;
        }

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

        /// <summary>
        /// Occurs when the amount of money changed.
        /// </summary>
        private void OnAmountChanged()
        {
            if (AmountChanged != null)
            {
                AmountChanged(this, null);
            }
        }
    }

    public sealed class MoneyEventArgs : EventArgs
    {
        public readonly int ChangeInAmount;
    }
}
