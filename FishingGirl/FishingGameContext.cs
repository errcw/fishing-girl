using System;

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
        public readonly FishingGame Game;

        /// <summary>
        /// The input state.
        /// </summary>
        public readonly Input Input;

        /// <summary>
        /// The storage manager.
        /// </summary>
        public readonly StorageDeviceManager Storage;

        /// <summary>
        /// Creates a new context.
        /// </summary>
        public FishingGameContext(FishingGame game, Input input, StorageDeviceManager storage)
        {
            Game = game;
            Input = input;
            Storage = storage;
        }
    }
}