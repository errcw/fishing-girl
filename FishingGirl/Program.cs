using System;

namespace FishingGirl
{
    static class Program
    {
        /// <summary>
        /// Entry point for the game.
        /// </summary>
        static void Main(string[] args)
        {
            using (FishingGame game = new FishingGame())
            {
                game.Run();
            }
        }
    }
}

