using System;

using Library.Components;
using Library.Storage;

namespace FishingGirl
{
    /// <summary>
    /// Global game interfaces.
    /// </summary>
    public class FishingGameContext
    {
        /// <summary>
        /// The enclosing game instance.
        /// </summary>
        public FishingGame Game { get; set; }

        /// <summary>
        /// The input state.
        /// </summary>
        public Input Input { get; set; }

        /// <summary>
        /// The storage manager.
        /// </summary>
        public Storage Storage { get; set; }

        /// <summary>
        /// The trial mode observer.
        /// </summary>
        public TrialModeObserverComponent Trial { get; set; }

        /// <summary>
        /// Creates a new context.
        /// </summary>
        public FishingGameContext(FishingGame game, Input input, Storage storage, TrialModeObserverComponent trial)
        {
            Game = game;
            Input = input;
            Storage = storage;
            Trial = trial;
        }
    }
}