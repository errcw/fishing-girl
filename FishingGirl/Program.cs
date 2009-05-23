using System;
using Library.Components;

namespace FishingGirl
{
    static class Program
    {
        /// <summary>
        /// Entry point for the game.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {
                using (FishingGame game = new FishingGame())
                {
                    game.Run();
                }
            }
            catch (Exception e)
            {
                using (ExceptionDebugGame game = new ExceptionDebugGame(e))
                {
                    game.Run();
                }
            }
        }
    }
}

