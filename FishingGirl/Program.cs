using System;

using Library.Extensions;

namespace FishingGirl
{
    static class Program
    {
        /// <summary>
        /// Entry point for the game.
        /// </summary>
        static void Main(string[] args)
        {
            GameExtensions.Run<FishingGame>();
        }
    }
}

