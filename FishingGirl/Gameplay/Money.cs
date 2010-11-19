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
        /// Occurs when money is gained or lost.
        /// </summary>
        public event EventHandler<MoneyEventArgs> AmountChanged;

        /// <summary>
        /// The amount of money available to the player.
        /// </summary>
        public int Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                int oldAmount = _amount;
                _amount = value;
                if (AmountChanged != null)
                {
                    AmountChanged(this, new MoneyEventArgs(_amount - oldAmount));
                }
            }
        }

        /// <summary>
        /// Creates a new set of money.
        /// </summary>
        /// <param name="state">The fishing state to track.</param>
        public Money(FishingState state)
        {
            _amount = 0;
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

        private int _amount;
    }

    /// <summary>
    /// Event arguments for when the money changes.
    /// </summary>
    public sealed class MoneyEventArgs : EventArgs
    {
        public readonly int ChangeInAmount;

        public MoneyEventArgs(int change)
        {
            ChangeInAmount = change;
        }
    }
}
