using System;

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
        /// Creates a new context.
        /// </summary>
        public FishingGameContext(FishingGame game, Input input)
        {
            Game = game;
            Input = input;
        }
    }
}