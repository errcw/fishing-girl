using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FishingGirl.Gameplay
{
    /// <summary>
    /// Event data for the ocean fish added and removed event.
    /// </summary>
    public sealed class FishEventArgs : EventArgs
    {
        /// <summary>
        /// The fish added or removed.
        /// </summary>
        public readonly Fish Fish;

        public FishEventArgs(Fish fish)
        {
            Fish = fish;
        }
    }

    /// <summary>
    /// Event data for the action changed event.
    /// </summary>
    public sealed class FishingActionEventArgs : EventArgs
    {
        /// <summary>
        /// The new action.
        /// </summary>
        public readonly FishingAction Action;

        /// <summary>
        /// Creates a new action event.
        /// </summary>
        public FishingActionEventArgs(FishingAction action)
        {
            Action = action;
        }
    }

    /// <summary>
    /// Event data for the fishing event.
    /// </summary>
    public sealed class FishingEventArgs : EventArgs
    {
        /// <summary>
        /// The event.
        /// </summary>
        public readonly FishingEvent Event;

        /// <summary>
        /// The fish.
        /// </summary>
        public readonly Fish Fish;

        /// <summary>
        /// Creates a new fish event.
        /// </summary>
        /// <param name="evt">The event type.</param>
        /// <param name="fish">The fish.</param>
        public FishingEventArgs(FishingEvent evt, Fish fish)
        {
            Event = evt;
            Fish = fish;
        }
    }
}
